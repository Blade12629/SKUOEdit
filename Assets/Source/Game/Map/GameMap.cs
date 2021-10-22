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
using UnityEngine;

namespace Assets.Source.Game.Map
{
    public sealed class GameMap : MonoBehaviour
    {
        public static GameMap Instance { get; private set; }
        public static event Action OnMapFinishLoading;
        public static event Action OnMapDestroyed;

        static readonly FileShare FileShareFlags = FileShare.ReadWrite | FileShare.Delete;

        public int Width { get; private set; }
        public int BlockWidth { get; private set; }
        public int Depth { get; private set; }
        public int BlockDepth { get; private set; }

        public string MapFile { get; private set; }
        public string StaticsFile { get; private set; }
        public string StaticsIdxFile { get; private set; }

        public bool IsMapLoaded { get; private set; }

        Vertex[] _vertices;
        MapTiles _mapTiles;
        MapStatics _mapStatics;

        MapChunk[] _chunks;
        Rect _renderedArea;

        [SerializeField] bool _toggleGrid;

        bool _firstMapCreation;

        public GameMap()
        {
            Instance = this;
            CameraController.OnCameraMoved += args => MoveToWorld(args.NewPosition);
            _firstMapCreation = true;
        }

        public void MoveToWorld(Vector3 newPos)
        {
            float xDiff = newPos.x - _renderedArea.x;
            float zDiff = newPos.z - _renderedArea.y;

            _renderedArea = new Rect(newPos.x, newPos.z, _renderedArea.width, _renderedArea.height);

            if (!IsMapLoaded)
                return;

            for (int i = 0; i < _chunks.Length; i++)
            {
                MapChunk chunk = _chunks[i];
                Rect chunkRenderedArea = chunk.RenderedArea;

                chunk.MoveToWorld(new Vector3(chunkRenderedArea.x + xDiff, 0, chunkRenderedArea.y + zDiff));
            }
        }

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

            if (_chunks != null)
            {
                for (int i = 0; i < _chunks.Length; i++)
                    _chunks[i].DestroyChunk();

                _chunks = null;
            }

            GameObject.Destroy(gameObject);

            Instance = null;
            IsMapLoaded = false;

            OnMapDestroyed?.Invoke();
        }

        /// <summary>
        /// Loads a specific map
        /// </summary>
        /// <param name="file">Map file</param>
        /// <param name="width">Map width</param>
        /// <param name="depth">Map depth</param>
        /// <param name="genOption">Generation option</param>
        /// <param name="tileHeights">Only used with <see cref="GenerationOption.Converted"/></param>
        /// <param name="tileIds">Only used with <see cref="GenerationOption.Converted"/></param>
        public void Load(string file, int width, int depth, GenerationOption genOption, int[] tileHeights, int[] tileIds)
        {
            MapFile = file;
            Width = width;
            BlockWidth = width / 8;
            Depth = depth;
            BlockDepth = depth / 8;

            _chunks = new MapChunk[3 * 3];
            _mapTiles = new MapTiles();
            _mapStatics = new MapStatics();

            EditorInput.InitializeUIPart();

            Debug.Log("Initializing minimap");
            Minimap.Instance.Initialize(width, depth);

            StartCoroutine(LoadMap());

            IEnumerator LoadMap()
            {
                Debug.Log("Loading map");

                Debug.Log("Loading static blocks");
                yield return new WaitForEndOfFrame();

                int index = GetMapIndex(MapFile);
                string dir = new FileInfo(MapFile).Directory.FullName;

                StaticsIdxFile = Path.Combine(dir, $"staidx{index}.mul");
                StaticsFile = Path.Combine(dir, $"statics{index}.mul");

                try
                {
                    _mapStatics.Load(StaticsFile, StaticsIdxFile, width, depth);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    UnityEditor.EditorApplication.isPlaying = false;
                }

                Debug.Log("Loading tile blocks");
                yield return new WaitForEndOfFrame();

                switch (genOption)
                {
                    case GenerationOption.Default:
                        if (MapFile.EndsWith("uop", StringComparison.CurrentCultureIgnoreCase))
                        {
                            _mapTiles.Load(MapFile, true, width, depth);
                        }
                        else
                            _mapTiles.Load(MapFile, false, width, depth);
                        break;

                    case GenerationOption.Flatland:
                        _mapTiles.GenerateFlatland(width, depth);
                        break;

                    case GenerationOption.Converted:
                        _mapTiles.GenerateConverted(tileHeights, tileIds, width, depth);
                        break;
                }

                Debug.Log("Loading map vertices");
                yield return new WaitForEndOfFrame();

                LoadMapVertices();

                Debug.Log("Loading minimap");
                yield return new WaitForEndOfFrame();

                LoadMinimap();

                Debug.Log("Loading map mesh");
                yield return new WaitForEndOfFrame();

                LoadMapMesh();
                IsMapLoaded = true;

                Debug.Log("Spawning Statics");
                yield return new WaitForEndOfFrame();

                try
                {
                    SpawnStatics();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    UnityEditor.EditorApplication.isPlaying = false;
                }

                Debug.Log("Finished generating map");
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
                    SetTileCornerHeight(x,      z,      height, 0, false, false, true);
                    SetTileCornerHeight(x - 1,  z,      height, 3, false, false, true);
                    SetTileCornerHeight(x - 1,  z - 1,  height, 2, false, false, true);
                    SetTileCornerHeight(x,      z - 1,  height, 1, false, false, true);

                    UpdateChunks(x, z);
                }
            }

            SelectionRenderer.Instance.Refresh();
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

                    SetTileCornerHeight(x,      z,      h0 + height, 0, false, false, true);
                    SetTileCornerHeight(x - 1,  z,      h1 + height, 3, false, false, true);
                    SetTileCornerHeight(x - 1,  z - 1,  h2 + height, 2, false, false, true);
                    SetTileCornerHeight(x,      z - 1,  h3 + height, 1, false, false, true);

                    UpdateChunks(x, z);
                }
            }

            SelectionRenderer.Instance.Refresh();
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
            SetTileCornerHeight(x, z, height, 0, false, false, true);
            SetTileCornerHeight(x - 1, z, height, 3, false, false, true);
            SetTileCornerHeight(x - 1, z - 1, height, 2, false, false, true);
            SetTileCornerHeight(x, z - 1, height, 1, false, false, true);

            SelectionRenderer.Instance.Refresh();
            UpdateChunks(x, z);
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
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i]?.EnableGrid();
        }

        /// <summary>
        /// Disables the map grid
        /// </summary>
        public void DisableGrid()
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i]?.DisableGrid();
        }

        /// <summary>
        /// Sets the map grid color
        /// </summary>
        public void SetGridColor(Color color)
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i]?.SetGridColor(color);
        }

        /// <summary>
        /// Sets the map grid size
        /// </summary>
        public void SetGridSize(float size)
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i]?.SetGridSize(size);
        }

        /// <summary>
        /// Checks if the map grid is currently enabled
        /// </summary>
        public bool IsGridEnabled()
        {
            for (int i = 0; i < _chunks.Length; i++)
            {
                if (_chunks[i] == null)
                    continue;

                return _chunks[i].IsGridEnabled();
            }

            return false;
        }

        /// <summary>
        /// Gets the map grid color
        /// </summary>
        public Color GetGridColor()
        {
            for (int i = 0; i < _chunks.Length; i++)
            {
                if (_chunks[i] == null)
                    continue;

                return _chunks[i].GetGridColor();
            }

            return default;
        }

        /// <summary>
        /// Gets the map grid size
        /// </summary>
        public float GetGridSize()
        {
            for (int i = 0; i < _chunks.Length; i++)
            {
                if (_chunks[i] == null)
                    continue;

                return _chunks[i].GetGridSize();
            }

            return 0;
        }

        /// <summary>
        /// Sets the currently selected tile <see cref="SelectionRenderer"/>
        /// </summary>
        public void SetSelectedTile(int x, int z)
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i]?.SetSelectedTile(x, z);
        }

        /// <summary>
        /// Sets the currently selected tile area size <see cref="SelectionRenderer"/>
        /// </summary>
        public void SetSelectedAreaSize(int size)
        {
            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i]?.SetSelectionSize(size);
        }

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

        void SetTileCornerHeight(int x, int z, int height, int indexOffset, bool updateChunks, bool refreshSelection, bool refreshUVs)
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

            if (refreshSelection)
                SelectionRenderer.Instance.Refresh();

            if (updateChunks)
                UpdateChunks(x, z);
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
                UpdateChunks(x, z);
        }

        void UpdateChunks(int x, int z)
        {
            if (!InRenderedBounds(x, z))
                return;

            for (int i = 0; i < _chunks.Length; i++)
            {
                MapChunk chunk = _chunks[i];

                if (!chunk.IsInBounds(x, z))
                    continue;

                chunk.RefreshChunk();
            }
        }

        int PositionToIndex(int x, int z, IndexType indexType)
        {
            return (x * Width + z) * (int)indexType;
        }

        int GetMapIndex(string fileName)
        {
            StringBuilder sb = new StringBuilder(2);

            bool wasNumber = false;
            for (int i = fileName.Length - 1; i >= 0; i--)
            {
                char c = fileName[i];

                if (!char.IsNumber(c))
                {
                    if (wasNumber)
                        break;
                }
                else
                {
                    wasNumber = true;

                    if (sb.Length == 0)
                        sb.Append(c);
                    else
                        sb.Insert(0, c);
                }
            }

            return int.Parse(sb.ToString());
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
            _renderedArea = new Rect(0, 0, MapChunk.MeshSize * 3, MapChunk.MeshSize * 3);

            for (int x = 0; x < 3; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    int index = x * 3 + z;
                    int xRender = x * MapChunk.MeshSize;
                    int zRender = z * MapChunk.MeshSize;

                    if (xRender >= Width || zRender >= Depth)
                        continue;

                    GameObject chunkObj = new GameObject($"Chunk {x}/{z}", typeof(MapChunk), typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer));
                    MapChunk chunk = chunkObj.GetComponent<MapChunk>();

                    Rect areaToRender = new Rect(xRender, zRender, MapChunk.MeshSize, MapChunk.MeshSize);

                    chunk.BuildChunk(areaToRender);

                    _chunks[index] = chunk;
                }
            }
        }

        void SpawnStatics()
        {
            //for (int bx = 0; bx < BlockWidth; bx++)
            //{
            //    int wx = bx * 8;

            //    for (int bz = 0; bz < BlockDepth; bz++)
            //    {
            //        int wz = bz * 8;
            //        StaticBlock sb = _mapStatics.GetStaticBlock(wx, wz);

            //        if (sb == null)
            //            continue;

            //        for (int i = 0; i < sb.Statics.Length; i++)
            //        {
            //            ref var st = ref sb.Statics[i];
            //            GameObject stObj = LoadStatic(st.TileId, new Vector3(wx + st.Y, st.Z * .1f, wz + st.X));
            //        }
            //    }
            //}

            //Test for luna bank (malas) area
            for (int x = 55; x <= 137; x++)
            {
                for (int z = 55; z <= 137; z++)
                {
                    int wx = x * 8;
                    int wz = z * 8;
                    StaticBlock sb = _mapStatics.GetStaticBlock(wx, wz);

                    if (sb == null)
                        continue;

                    for (int i = 0; i < sb.Statics.Length; i++)
                    {
                        ref var st = ref sb.Statics[i];
                        GameObject stObj = LoadStatic(st.TileId, new Vector3(wx + st.Y, st.Z * .1f, wz + st.X));
                    }
                }
            }
        }

        GameObject LoadStatic(uint stId, Vector3 pos)
        {
            StaticCacheEntry entry = StaticCache.Get(stId);

            if (entry == null)
                return null;

            GameObject staticObj = StaticPool.Rent();
            staticObj.name = stId.ToString();
            Mesh mesh = staticObj.GetComponent<MeshFilter>().mesh = new Mesh();

            mesh.SetVertexBufferParams(entry.Vertices.Length, VertexLayout.Layout);
            mesh.SetVertexBufferData(entry.Vertices, 0, 0, entry.Vertices.Length);
            mesh.SetIndices(entry.Indices, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();

            MeshRenderer renderer = staticObj.GetComponent<MeshRenderer>();
            renderer.material = new Material(Client.Instance.DefaultStaticMaterial);
            renderer.material.mainTexture = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(stId);
            staticObj.transform.position = pos;

            return staticObj;
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
        Default,
        Flatland,
        Converted
    }
}
