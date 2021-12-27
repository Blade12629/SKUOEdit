//#define LOG_MISSING_STATIC_IDS

using Assets.Source.IO;
using Assets.Source.Textures;
using Assets.Source.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    public sealed class GameMap : MonoBehaviour
    {
        public static GameMap Instance { get; private set; }
        public static event Action OnMapFinishLoading;
        public static event Action OnMapDestroyed;

        public int Width { get; private set; }
        public int BlockWidth { get; private set; }
        public int Depth { get; private set; }
        public int BlockDepth { get; private set; }

        public string MapFile { get; private set; }
        public string StaticsFile { get; private set; }
        public string StaticsIdxFile { get; private set; }

        public bool IsMapLoaded { get; private set; }

        public MapChunk Chunk => _chunk;

        bool _toggleGrid;
        bool _firstMapCreation;

        MapChunk _chunk;
        Rect _renderedArea;

        Vertex[] _vertices;
        MapTiles _mapTiles;

        public GameMap()
        {
            Instance = this;
            CameraController.OnCameraMoved += args => MoveToWorld(args.NewPosition);
            _firstMapCreation = true;
        }

        /// <summary>
        /// Moves the rendered map area to a specified position
        /// </summary>
        /// <param name="newPos">Position for <see cref="_renderedArea"/> to start</param>
        public void MoveToWorld(Vector3 newPos)
        {
            _renderedArea = new Rect(newPos.x, newPos.z, _renderedArea.width, _renderedArea.height);

            if (!IsMapLoaded)
                return;

            _chunk.MoveToWorld((Point)newPos);
        }

        /// <summary>
        /// Destroys the map including the gameobject which holds this script
        /// </summary>
        public void Destroy()
        {
            Debug.Log("Destroying GameMap");

            EditorInput.ClearActions();

            Width = 0;
            BlockWidth = 0;
            Depth = 0;
            BlockDepth = 0;
            MapFile = null;

            _vertices = null;
            _mapTiles = null;
            _renderedArea = default;

            if (_chunk != null)
            {
                Destroy(_chunk.gameObject);
                _chunk = null;
            }

            Destroy(gameObject);

            Instance = null;
            IsMapLoaded = false;

            OnMapDestroyed?.Invoke();
        }

        /// <summary>
        /// Generates a map
        /// </summary>
        /// <param name="option">Generation option</param>
        /// <param name="width">Map width</param>
        /// <param name="depth">Map depth</param>
        /// <param name="tileHeights">Only used with <see cref="GenerationOption.Converted"/></param>
        /// <param name="tileIds">Only used with <see cref="GenerationOption.Converted"/></param>
        /// <param name="loadingbar">Loadingbar to use</param>
        public void Generate(GenerationOption option, int width, int depth, int[] tileHeights, int[] tileIds, LoadingBar loadingbar)
        {
            SetupDefaults(null, width, depth);

            StartCoroutine(LoadMap());

            IEnumerator LoadMap()
            {
                loadingbar.Setup(0, 7, 0, "Loading map");
                yield return new WaitForEndOfFrame();

                yield return PrepareMapLoad(loadingbar, width, depth); // loadingbar: 2

                switch (option)
                {
                    case GenerationOption.Flatland:
                        loadingbar.Increment("Generating flatland..."); // loadingbar: 3
                        yield return new WaitForEndOfFrame();
                        _mapTiles.GenerateFlatland(width, depth);
                        break;

                    case GenerationOption.Converted:
                        loadingbar.Increment("Converting map..."); // loadingbar: 3
                        yield return new WaitForEndOfFrame();
                        _mapTiles.GenerateConverted(tileHeights, tileIds, width, depth);
                        break;
                }

                yield return FinishMapLoad(loadingbar); // loadingbar: 7
            }
        }

        /// <summary>
        /// Loads a specific map
        /// </summary>
        /// <param name="file">Map file</param>
        /// <param name="width">Map width</param>
        /// <param name="depth">Map depth</param>
        /// <param name="loadingbar">Loadingbar to use</param>
        public void Load(string file, int width, int depth, LoadingBar loadingbar)
        {
            SetupDefaults(file, width, depth);
            StartCoroutine(LoadMap());

            IEnumerator LoadMap()
            {
                loadingbar.Setup(0, 7, 0, "Loading map");
                yield return new WaitForEndOfFrame();

                yield return PrepareMapLoad(loadingbar, width, depth); // loadingbar: 2

                if (MapFile.EndsWith("uop", StringComparison.CurrentCultureIgnoreCase))
                {
                    loadingbar.Increment("Loading uop map file..."); // loadingbar: 3
                    yield return new WaitForEndOfFrame();
                    _mapTiles.Load(MapFile, true, width, depth);
                }
                else
                {
                    loadingbar.Increment("Loading mul map file..."); // loadingbar: 3
                    yield return new WaitForEndOfFrame();
                    _mapTiles.Load(MapFile, false, width, depth);
                }

                yield return FinishMapLoad(loadingbar); // loadingbar: 7
            }
        }

        /// <summary>
        /// Saves the current map to <see cref="MapFile"/>
        /// </summary>
        /// <param name="file">if not null sets the <see cref="MapFile"/> path before saving</param>
        public void Save(string file = null)
        {
            if (!string.IsNullOrEmpty(file))
                MapFile = file;

            if (file.EndsWith("uop", StringComparison.CurrentCultureIgnoreCase))
                _mapTiles.Save(file, true);
            else
                _mapTiles.Save(file, false);
        }

        /// <summary>
        /// Sets the height of tiles within a specific area
        /// </summary>
        public void SetTileAreaHeight(int x, int z, int width, int depth, int height)
        {
            int xend = x + width + 1;
            int zend = z + depth + 1;

            for (; x < xend; x++)
            {
                for (; z < zend; z++)
                {
                    SetTileCornerHeight(x,      z,      height, 0, false, true);
                    SetTileCornerHeight(x - 1,  z,      height, 3, false, true);
                    SetTileCornerHeight(x - 1,  z - 1,  height, 2, false, true);
                    SetTileCornerHeight(x,      z - 1,  height, 1, false, true);

                    _chunk.RebuildVertices();
                }
            }
        }

        /// <summary>
        /// Increases the height of tiles within a specific area
        /// </summary>
        public void IncreaseTileAreaHeight(int x, int z, int width, int depth, int height)
        {
            int xend = x + width + 1;
            int zend = z + depth + 1;

            for (; x < xend; x++)
            {
                for (; z < zend; z++)
                {
                    int h0 = GetTileCornerHeight(x,     z);
                    int h1 = GetTileCornerHeight(x - 1, z);
                    int h2 = GetTileCornerHeight(x - 1, z - 1);
                    int h3 = GetTileCornerHeight(x,     z - 1);

                    SetTileCornerHeight(x,      z,      h0 + height, 0, false, true);
                    SetTileCornerHeight(x - 1,  z,      h1 + height, 3, false, true);
                    SetTileCornerHeight(x - 1,  z - 1,  h2 + height, 2, false, true);
                    SetTileCornerHeight(x,      z - 1,  h3 + height, 1, false, true);

                    _chunk.RebuildVertices();
                }
            }
        }

        /// <summary>
        /// Increases the height of a specific tile
        /// </summary>
        public void IncreaseTileHeight(int x, int z, int height)
        {
            int h0 = GetTileCornerHeight(x,     z);
            int h1 = GetTileCornerHeight(x,     z + 1);
            int h2 = GetTileCornerHeight(x + 1, z + 1);
            int h3 = GetTileCornerHeight(x + 1, z);

            SetTileCornerHeight(x,      z,      h0 + height);
            SetTileCornerHeight(x,      z + 1,  h1 + height);
            SetTileCornerHeight(x + 1,  z + 1,  h2 + height);
            SetTileCornerHeight(x + 1,  z,      h3 + height);
        }

        /// <summary>
        /// Sets the height of a specific tile
        /// </summary>
        public void SetTileHeight(int x, int z, int height)
        {
            SetTileCornerHeight(x,      z,      height);
            SetTileCornerHeight(x,      z + 1,  height);
            SetTileCornerHeight(x + 1,  z + 1,  height);
            SetTileCornerHeight(x + 1,  z,      height);
        }

        /// <summary>
        /// Increases the height of a specific tile corner
        /// </summary>
        public void IncreaseTileCornerHeight(int x, int z, int height)
        {
            int h = GetTileCornerHeight(x, z);
            SetTileCornerHeight(x, z, h + height);
        }

        /// <summary>
        /// Sets the height of a specific point on the map
        /// </summary>
        public void SetTileCornerHeight(int x, int z, int height)
        {
            SetTileCornerHeight(x, z, height, 0, false, true);
            SetTileCornerHeight(x - 1, z, height, 3, false, true);
            SetTileCornerHeight(x - 1, z - 1, height, 2, false, true);
            SetTileCornerHeight(x, z - 1, height, 1, false, true);

            _chunk.RebuildVertices();
        }

        /// <summary>
        /// Gets the height of a specific point on the map
        /// </summary>
        /// <returns>Point height or 0 if point is out of bounds</returns>
        public int GetTileCornerHeight(int x, int z)
        {
            int index = PositionToIndex(x, z, IndexType.Vertice);

            if (index < 0 || index >= _vertices.Length)
                return 0;

            ref Vertex vertex = ref _vertices[index];

            return (int)Mathf.Ceil(vertex.Y * 10f);
        }

        /// <summary>
        /// Sets the tile id of a specific tile
        /// </summary>
        public void SetTileId(int x, int z, short id)
        {
            int vertexIndex = PositionToIndex(x, z, IndexType.Vertice);

            if (vertexIndex < 0 || vertexIndex >= _vertices.Length)
                return;

            ref Tile tileBL = ref _mapTiles.GetTile(x, z);
            tileBL.TileId = id;

            RefreshUVs(x, z);
        }

        /// <summary>
        /// Copies the vertex data of a specific area into the specified vertex array
        /// </summary>
        public void CopyAreaVertices(Vertex[] dest, int x, int z, int width, int depth)
        {
            Parallel.For(0, width, xCur =>
            {
                int indexSrc = PositionToIndex(x + xCur, z, IndexType.Vertice);
                int indexDest = PositionToIndex(xCur, 0, depth, IndexType.Vertice);
                int length = depth * 4;

                // our index cannot be less than 0, reduce length to make it fit
                if (indexSrc < 0)
                {
                    length += indexSrc;
                    indexSrc = 0;
                }

                // our length would be too big, simply get the new length
                if (indexDest + length >= dest.Length)
                {
                    length = dest.Length - indexDest;

                    if (indexSrc + length >= _vertices.Length)
                    {
                        length = Math.Min(length, _vertices.Length - indexSrc);
                    }
                }
                else if (indexSrc + length >= _vertices.Length)
                {
                    length = _vertices.Length - indexSrc;
                }

                // seems like we have no valid data to copy, just skip
                if (length <= 0)
                    return;

                // copy area vertice data
                Array.Copy(_vertices, indexSrc, dest, indexDest, length);
            });
        }

        /// <summary>
        /// Enables the map grid
        /// </summary>
        public void EnableGrid()
        {
            _chunk.IsRenderingGrid = true;
        }

        /// <summary>
        /// Disables the map grid
        /// </summary>
        public void DisableGrid()
        {
            _chunk.IsRenderingGrid = false;
        }

        /// <summary>
        /// Enables rendering the current selection
        /// </summary>
        public void EnableSelectedGrid()
        {
            _chunk.IsRenderingSelection = true;
        }

        /// <summary>
        /// Disables rendering the current selection
        /// </summary>
        public void DisableSelectedGrid()
        {
            _chunk.IsRenderingSelection = false;
        }

        /// <summary>
        /// Sets the map grid color
        /// </summary>
        public void SetGridColor(Color color)
        {
            _chunk.GridColor = color;
        }

        /// <summary>
        /// Sets the map grid size
        /// </summary>
        public void SetGridSize(float size)
        {
            _chunk.GridSize = size;
        }

        /// <summary>
        /// Sets the selection size
        /// </summary>
        public void SetSelectionSize(int size)
        {
            _chunk.SelectionSize = size;
        }

        /// <summary>
        /// Checks if the map grid is currently enabled
        /// </summary>
        public bool IsGridEnabled()
        {
            return _chunk.IsRenderingGrid;
        }

        /// <summary>
        /// Gets the map grid color
        /// </summary>
        public Color GetGridColor()
        {
            return _chunk.GridColor;
        }

        /// <summary>
        /// Gets the map grid size
        /// </summary>
        public float GetGridSize()
        {
            return _chunk.GridSize;
        }

        /// <summary>
        /// Sets the currently selected tile <see cref="SelectionRenderer"/>
        /// </summary>
        public void SetSelectedTile(int x, int z)
        {
            _chunk.SelectedTile = new Point(x + (int)_renderedArea.x, z + (int)_renderedArea.y);
        }

        /// <summary>
        /// Gets the tile id of the specified position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns>tile id or 0 if not found</returns>
        public short GetTileId(int x, int z)
        {
            int index = PositionToIndex(x, z, IndexType.Vertice);

            if (index < 0 || index >= _vertices.Length)
                return 0;

            ref Tile tile = ref _mapTiles.GetTile(x, z);
            return tile.TileId;
        }

        /// <summary>
        /// Converts a specific position to an index for array access
        /// </summary>
        /// <param name="indexType">Type of index we want to get, this affects the returned index</param>
        static int PositionToIndex(int x, int z, int depth, IndexType indexType)
        {
            return (x * depth + z) * (int)indexType;
        }

        void SetTileCornerHeight(int x, int z, int height, int indexOffset, bool updateChunks, bool refreshUVs)
        {
            int index = PositionToIndex(x, z, IndexType.Vertice);

            if (index < 0 || index >= _vertices.Length)
                return;

            ref Vertex vertex = ref _vertices[index + indexOffset];
            vertex.Y = height * .1f;

            ref Tile tile = ref _mapTiles.GetTile(x, z);
            tile.Z = (sbyte)height;

            if (refreshUVs)
                RefreshUVs(x, z, false);

            if (updateChunks)
                _chunk.RebuildVertices();
        }

        void RefreshUVs(int x, int z, bool updateChunks = true)
        {
            int vertexIndex = PositionToIndex(x, z, IndexType.Vertice);

            if (vertexIndex < 0 || vertexIndex >= _vertices.Length)
                return;

            int hBL = GetTileCornerHeight(x, z);
            int hTL = GetTileCornerHeight(x, z + 1);
            int hTR = GetTileCornerHeight(x + 1, z + 1);
            int hBR = GetTileCornerHeight(x + 1, z);

            bool isEvenTile = hBL == hTL &&
                              hBL == hTR &&
                              hBL == hBR;

            ref Tile tileBL = ref _mapTiles.GetTile(x, z);

            Vector2[] tileUVs = GetTileUVs(tileBL.TileId, !isEvenTile);

            for (int i = 0; i < 4; i++)
            {
                ref Vertex vertex = ref _vertices[vertexIndex + i];
                ref Vector2 uv = ref tileUVs[i];

                vertex.UvX = uv.x;
                vertex.UvY = uv.y;
            }

            if (updateChunks)
                _chunk.RebuildVertices();
        }

        int PositionToIndex(int x, int z, IndexType indexType)
        {
            return (x * Width + z) * (int)indexType;
        }

        void LoadMapVertices()
        {
            Vertex[] vertices = new Vertex[Width * Depth * 4];
            Color[] minimapColors = new Color[Width * Depth];

            Parallel.For(0, Depth, x =>
            {
                Parallel.For(0, Width, z =>
                {
                    ref Tile tileBL = ref _mapTiles.GetTile(x, z);

                    sbyte hTL = 0;
                    sbyte hTR = 0;
                    sbyte hBR = 0;

                    if (z < Width - 1)
                    {
                        ref Tile tileTL = ref _mapTiles.GetTile(x, z + 1);
                        hTL = tileTL.Z;

                        if (x < Depth - 1)
                        {
                            ref Tile tileTR = ref _mapTiles.GetTile(x + 1, z + 1);
                            hTR = tileTR.Z;
                        }
                        else
                            hTR = tileBL.Z;
                    }
                    else
                        hTL = tileBL.Z;

                    if (x < Depth - 1)
                    {
                        ref Tile tileBR = ref _mapTiles.GetTile(x + 1, z);
                        hBR = tileBR.Z;
                    }
                    else
                        hBR = tileBL.Z;


                    bool isEvenTile = tileBL.Z == hTL &&
                                      tileBL.Z == hTR &&
                                      tileBL.Z == hBR;

                    int vertexIndex = PositionToIndex(x, z, IndexType.Vertice);
                    Vector2[] uvs = GetTileUVs(tileBL.TileId, !isEvenTile);

                    for (int i = 0; i < 4; i++)
                    {
                        ref Vertex vertex = ref vertices[vertexIndex + i];
                        ref Vector2 uv = ref uvs[i];

                        switch (i)
                        {
                            default:
                            case 0:
                                vertex.X = x;
                                vertex.Z = z;

                                vertex.Y = tileBL.Z * .1f;
                                break;

                            case 1:
                                vertex.X = x;
                                vertex.Z = z + 1;

                                vertex.Y = hTL * .1f;
                                break;

                            case 2:
                                vertex.X = x + 1;
                                vertex.Z = z + 1;

                                vertex.Y = hTR * .1f;
                                break;

                            case 3:
                                vertex.X = x + 1;
                                vertex.Z = z;

                                vertex.Y = hBR * .1f;
                                break;
                        }

                        vertex.UvX = uv.x;
                        vertex.UvY = uv.y;
                    }
                });
            });

            _vertices = vertices;
        }

        void LoadMinimap()
        {
            for (int x = 0; x < Depth; x++)
            {
                for (int z = 0; z < Width; z++)
                {
                    ref Tile tile = ref _mapTiles.GetTile(x, z);
                    Minimap.Instance.SetMapTile(new Vector2(x, z), tile.TileId, false);
                }
            }

            Minimap.Instance.ApplyMapChanges();
        }

        void LoadMapMesh()
        {
            _chunk.Initialize(Client.Instance.DefaultMaterial, GameTextures.GameTexture);
            _renderedArea = new Rect(0, 0, _chunk.Size, _chunk.Size);
            _chunk.Build();

            //for (int x = 0; x < 3; x++)
            //{
            //    for (int z = 0; z < 3; z++)
            //    {
            //        int index = x * 3 + z;
            //        int xRender = x * Chunk.MeshSize;
            //        int zRender = z * Chunk.MeshSize;

            //        if (xRender >= Width || zRender >= Depth)
            //            continue;

            //        GameObject chunkObj = new GameObject($"Chunk {x}/{z}", typeof(Chunk), typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer));
            //        Chunk chunk = chunkObj.GetComponent<Chunk>();

            //        Rect areaToRender = new Rect(xRender, zRender, Chunk.MeshSize, Chunk.MeshSize);

            //        chunk.Build(areaToRender);

            //        _chunks[index] = chunk;
            //    }
            //}
        }

        Vector2[] GetTileUVs(short tileId, bool getTexture)
        {
            Vector2[] artUVs = null;
            if (getTexture)
            {
                ref var landData = ref ClassicUO.IO.Resources.TileDataLoader.Instance.LandData[tileId];

                if (landData.TexID > 0)
                {
                    artUVs = GameTextures.GetUVsTexture(landData.TexID);
                }
                else
                {
                    artUVs = GameTextures.AddNoDraw();
                }
            }
            else
            {
                artUVs = GameTextures.GetUVsTile(tileId);
            }

            return artUVs;
        }

        bool InRenderedBounds(int x, int z)
        {
            return _renderedArea.Contains(new Vector2(x, z));
        }

        void SetupDefaults(string mapFile, int width, int depth)
        {
            MapFile = mapFile;
            Width = width;
            BlockWidth = width / 8;
            Depth = depth;
            BlockDepth = depth / 8;

            GameObject chunkObj = new GameObject("Map Chunk", typeof(MapChunk));
            _chunk = chunkObj.GetComponent<MapChunk>();
        }

        /// <summary>
        /// Prepares the map load process (loadingbar count: 2)
        /// </summary>
        IEnumerator PrepareMapLoad(LoadingBar loadingbar, int width, int depth)
        {
            _mapTiles = new MapTiles();

            loadingbar.Increment("Initializing UI..."); // loadingbar: 1
            yield return new WaitForEndOfFrame();

            EditorInput.InitializeUIPart(); 

            loadingbar.Increment("Initializing minimap..."); // loadingbar: 2
            yield return new WaitForEndOfFrame();
            Minimap.Instance.Initialize(width, depth); 
        }

        /// <summary>
        /// Finishes up the map loading process (loadingbar count: 4)
        /// </summary>
        IEnumerator FinishMapLoad(LoadingBar loadingbar)
        {
            loadingbar.Increment("Loading map vertices...");
            yield return new WaitForEndOfFrame();

            LoadMapVertices(); // loadingbar: 1

            loadingbar.Increment("Loading minimap...");
            yield return new WaitForEndOfFrame();

            LoadMinimap(); // loadingbar: 2

            loadingbar.Increment("Loading map mesh...");
            yield return new WaitForEndOfFrame();

            LoadMapMesh(); // loadingbar: 3
            IsMapLoaded = true;

            loadingbar.Increment("Finished map generation, initializing defaults...");  // loadingbar: 4
            yield return new WaitForEndOfFrame();

            if (_firstMapCreation)
            {
                _firstMapCreation = false;

                SetGridSize(GameConfig.GridSize);
                SetGridColor(GameConfig.GridColor);

                if (GameConfig.EnableGrid)
                    EnableGrid();
            }

            OnMapFinishLoading?.Invoke();
            CameraController.Instance.InitializePosition();

            yield return new WaitForEndOfFrame();
            loadingbar.gameObject.SetActive(false);
        }

        void Update()
        {
            EditorInput.UpdateInput();
        }

        enum IndexType
        {
            None = 1,
            Vertice = 4,
            Index = 6,
        }
    }

    public enum GenerationOption
    {
        Flatland,
        Converted
    }
}
