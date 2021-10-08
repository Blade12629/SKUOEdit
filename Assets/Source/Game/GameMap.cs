using Assets.Source.Textures;
using Assets.Source.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game
{
    public class GameMap : MonoBehaviour
    {
        public static GameMap Instance { get; private set; }
        public static EditorInput Input
        {
            get
            {
                if (_input == null)
                    _input = new EditorInput();

                return _input;
            }
        }
        public static event Action OnMapFinishLoading;

        static EditorInput _input;

        public int Width { get; private set; }
        public int BlockWidth { get; private set; }
        public int Depth { get; private set; }
        public int BlockDepth { get; private set; }

        public string MapFile { get; private set; }

        Vertex[] _vertices;
        TileBlock[] _tileBlocks;

        MapChunk[] _chunks;
        Rect _renderedArea;
        
        public GameMap()
        {
            Instance = this;
            CameraController.OnCameraMoved += args => MoveToWorld(args.NewPosition);
        }

        public void MoveToWorld(Vector3 newPos)
        {
            float xDiff = newPos.x - _renderedArea.x;
            float zDiff = newPos.z - _renderedArea.y;

            _renderedArea = new Rect(newPos.x, newPos.z, _renderedArea.width, _renderedArea.height);

            for (int i = 0; i < _chunks.Length; i++)
            {
                MapChunk chunk = _chunks[i];
                Rect chunkRenderedArea = chunk.RenderedArea;

                chunk.MoveToWorld(new Vector3(chunkRenderedArea.x + xDiff, 0, chunkRenderedArea.y + zDiff));
            }
        }

        public void Destroy()
        {
            Width = 0;
            BlockWidth = 0;
            Depth = 0;
            BlockDepth = 0;
            MapFile = null;

            _vertices = null;
            _tileBlocks = null;
            _renderedArea = default;

            for (int i = 0; i < _chunks.Length; i++)
                _chunks[i].DestroyChunk();

            _chunks = null;

            GameObject.Destroy(gameObject);

            Instance = null;
        }

        public void Load(string file, int width, int depth)
        {
            MapFile = file;
            Width = width;
            BlockWidth = width / 8;
            Depth = depth;
            BlockDepth = depth / 8;

            _chunks = new MapChunk[3 * 3];

            Minimap.Instance.Initialize(width, depth);

            StartCoroutine(LoadMap());

            IEnumerator LoadMap()
            {
                Debug.Log("Loading map");

                Debug.Log("Loading map blocks");
                yield return new WaitForEndOfFrame();

                LoadMapBlocks();

                Debug.Log("Loading map vertices");
                yield return new WaitForEndOfFrame();

                LoadMapVertices();

                Debug.Log("Loading minimap");
                yield return new WaitForEndOfFrame();

                LoadMinimap();

                Debug.Log("Finished loading map");
                yield return new WaitForEndOfFrame();

                LoadMapMesh();

                OnMapFinishLoading?.Invoke();
            }
        }

        public void Save(string file = null)
        {
            if (!string.IsNullOrEmpty(file))
                MapFile = file;

            SaveMapBlocks();
        }

        public void SetTileHeight(int x, int z, int height)
        {
            SetTileCornerHeight(x,      z, height, false);
            SetTileCornerHeight(x,      z + 1, height, false);
            SetTileCornerHeight(x + 1,  z + 1, height, false);
            SetTileCornerHeight(x + 1,  z, height, false);

            UpdateChunks(x, z);
        }

        public void IncreaseTileHeight(int x, int z, int height)
        {
            IncreaseTileCornerHeight(x,     z, height, false);
            IncreaseTileCornerHeight(x,     z + 1, height, false);
            IncreaseTileCornerHeight(x + 1, z + 1, height, false);
            IncreaseTileCornerHeight(x + 1, z, height, false);

            UpdateChunks(x, z);
        }

        public void DecreaseTileHeight(int x, int z, int height)
        {
            IncreaseTileHeight(x, z, -height);
        }

        public void SetTileCornerHeight(int x, int z, int height, int indexOffset, bool updateChunks = true)
        {
            int index = PositionToIndex(x, z, IndexType.Vertice);

            if (index < 0 || index >= _vertices.Length)
                return;

            ref Vertex vertex = ref _vertices[index + indexOffset];
            vertex.Y = height * .1f;

            ref Tile tile = ref GetTile(z, x);
            tile.Z = (sbyte)height;

            if (updateChunks)
                UpdateChunks(x, z);
        }

        public void SetTileCornerHeight(int x, int z, int height, bool updateChunks = true)
        {
            SetTileCornerHeight(x,      z,      height, 0, false);
            SetTileCornerHeight(x - 1,  z,      height, 3, false);
            SetTileCornerHeight(x - 1,  z - 1,  height, 2, false);
            SetTileCornerHeight(x,      z - 1,  height, 1, false);

            if (updateChunks)
                UpdateChunks(x, z);
        }

        public void IncreaseTileCornerHeight(int x, int z, int amount, bool updateChunks = true)
        {
            int oldHeight = GetTileCornerHeight(x, z);
            int newHeight = oldHeight + amount;

            SetTileCornerHeight(x, z, newHeight, false);

            if (updateChunks)
                UpdateChunks(x, z);
        }

        public void DecreaseTileCornerHeight(int x, int z, int amount)
        {
            IncreaseTileCornerHeight(x, z, -amount);
        }

        public int GetTileCornerHeight(int x, int z)
        {
            int index = PositionToIndex(x, z, IndexType.Vertice);

            if (index < 0 || index >= _vertices.Length)
                return 0;

            ref Vertex vertex = ref _vertices[index];

            return (int)Mathf.Ceil(vertex.Y * 10f);
        }

        public bool TryGetTileCornerHeight(int x, int z, out int height)
        {
            height = 0;
            int index = PositionToIndex(x, z, IndexType.Vertice);

            if (index < 0 || index >= _vertices.Length)
                return false;

            ref Vertex vertex = ref _vertices[index];
            height = (int)(vertex.Y * 10);
            return true;
        }

        public void SetTileId(int x, int z, short id)
        {
            int vertexIndex = PositionToIndex(x, z, IndexType.Vertice);

            if (vertexIndex < 0 || vertexIndex >= _vertices.Length)
                return;

            ref Tile tileBL = ref GetTile(z, x);
            ref Tile tileTL = ref GetTile(z + 1, x);
            ref Tile tileTR = ref GetTile(z + 1, x + 1);
            ref Tile tileBR = ref GetTile(z, x + 1);

            tileBL.TileId = id;

            bool isEvenTile = tileBL.Z == tileTL.Z &&
                              tileBL.Z == tileTR.Z &&
                              tileBL.Z == tileBR.Z;

            Vector2[] tileUVs = GetTileUVs(tileBL.TileId, !isEvenTile);

            for (int i = 0; i < 4; i++)
            {
                ref Vertex vertex = ref _vertices[vertexIndex + i];
                ref Vector2 uv = ref tileUVs[i];

                vertex.UvX = uv.x;
                vertex.UvY = uv.y;
            }

            UpdateChunks(x, z);
        }

        /// <summary>
        /// Copies vertex data into the specified array
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

        void UpdateChunks(int x, int z)
        {
            if (!InRenderedBounds(x, z))
                return;

            for (int i = 0; i < _chunks.Length; i++)
            {
                MapChunk chunk = _chunks[i];

                //if (!chunk.IsInBounds(x, z))
                //    continue;

                chunk.RefreshChunk();
            }
        }

        int PositionToIndex(int x, int z, IndexType indexType)
        {
            return (x * Width + z) * (int)indexType;
        }

        static int PositionToIndex(int x, int z, int depth, IndexType indexType)
        {
            return (x * depth + z) * (int)indexType;
        }

        void LoadMapBlocks()
        {
            TileBlock[] tileBlocks = new TileBlock[BlockWidth * BlockDepth];

            unsafe
            {
                using (FileStream mapStream = new FileStream(MapFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (MemoryMappedFile mapMMF = MemoryMappedFile.CreateFromFile(mapStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
                using (MemoryMappedViewAccessor mapAccessor = mapMMF.CreateViewAccessor())
                {
                    byte* mapStart = null;
                    mapAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref mapStart);

                    if (mapStart == null)
                        throw new OperationCanceledException($"Unable to acquire pointer for map {MapFile}");

                    int blockIndex = 0;
                    int memSizePerBlockWidth = BlockDepth * (4 + 2 + 1);

                    for (int bx = 0; bx < BlockWidth; bx++)
                    {
                        for (int bz = 0; bz < BlockDepth; bz++)
                        {
                            int* header = ReadPtr<int>(ref mapStart, 4);
                            Tile[] tiles = new Tile[64];

                            for (int i = 0; i < 64; i++)
                                tiles[i] = new Tile(*ReadPtr<short>(ref mapStart, 2), *ReadPtr<sbyte>(ref mapStart, 1));

                            tileBlocks[blockIndex++] = new TileBlock(*header, tiles);
                        }
                    }
                }
            }

            _tileBlocks = tileBlocks;

            static unsafe T* ReadPtr<T>(ref byte* ptr, int length) where T : unmanaged
            {
                byte* cur = ptr;
                ptr += length;

                return (T*)cur;
            }
        }

        void LoadMapVertices()
        {
            Vertex[] vertices = new Vertex[Width * Depth * 4];
            Color[] minimapColors = new Color[Width * Depth];

            Parallel.For(0, Depth, x =>
            {
                Parallel.For(0, Width, z =>
                {
                    ref Tile tileBL = ref GetTile(z, x);

                    sbyte hTL = 0;
                    sbyte hTR = 0;
                    sbyte hBR = 0;

                    if (z < Width - 1)
                    {
                        ref Tile tileTL = ref GetTile(z + 1, x);
                        hTL = tileTL.Z;

                        if (x < Depth - 1)
                        {
                            ref Tile tileTR = ref GetTile(z + 1, x + 1);
                            hTR = tileTR.Z;
                        }
                        else
                            hTR = tileBL.Z;
                    }
                    else
                        hTL = tileBL.Z;

                    if (x < Depth - 1)
                    {
                        ref Tile tileBR = ref GetTile(z, x + 1);
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
                    ref Tile tile = ref GetTile(z, x);
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

                    GameObject chunkObj = new GameObject($"Chunk {x}/{z}", typeof(MapChunk), typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer));
                    MapChunk chunk = chunkObj.GetComponent<MapChunk>();

                    int xRender = x * MapChunk.MeshSize;
                    int zRender = z * MapChunk.MeshSize;
                    Rect areaToRender = new Rect(xRender, zRender, MapChunk.MeshSize, MapChunk.MeshSize);

                    chunk.BuildChunk(areaToRender);

                    _chunks[index] = chunk;
                }
            }
        }

        void SaveMapBlocks()
        {
            int memSizePerBlockWidth = BlockDepth * (sizeof(int) + sizeof(short) + sizeof(sbyte));
            long totalSize = BlockWidth * memSizePerBlockWidth;

            unsafe
            {
                using (MemoryMappedFile mapMMF = MemoryMappedFile.CreateNew(null, totalSize, MemoryMappedFileAccess.ReadWrite))
                using (MemoryMappedViewAccessor mapAccessor = mapMMF.CreateViewAccessor())
                {
                    byte* mapStart = null;
                    mapAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref mapStart);

                    if (mapStart == null)
                        throw new OperationCanceledException($"Unable to acquire pointer for map {MapFile}");

                    for (int i = 0; i < _tileBlocks.Length; i++)
                    {
                        ref TileBlock block = ref _tileBlocks[i];
                        WritePtr(ref mapStart, block.Header, 4);

                        for (int x = 0; x < block.Tiles.Length; x++)
                        {
                            ref Tile tile = ref block.Tiles[x];
                            WritePtr(ref mapStart, tile.TileId, 2);
                            WritePtr(ref mapStart, tile.Z, 1);
                        }
                    }
                }
            }

            static unsafe void WritePtr<T>(ref byte* ptr, T value, int length) where T : unmanaged
            {
                T* curPtr = (T*)ptr;
                *curPtr = value;

                ptr += length;
            }
        }

        ref TileBlock GetBlock(int x, int y)
        {
            try
            {
                return ref _tileBlocks[(x >> 3) * BlockDepth + (y >> 3)];
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        ref Tile GetTile(int x, int y)
        {
            ref TileBlock block = ref GetBlock(x, y);
            Tile[] tiles = block.Tiles;

            return ref tiles[((y & 0x7) << 3) + (x & 0x7)];
        }

        Vector2[] GetTileUVs(short tileId, bool getTexture)
        {
            Vector2[] artUVs = null;
            if (getTexture)
            {
                ref var landData = ref ClassicUO.IO.Resources.TileDataLoader.Instance.LandData[tileId];

                if (landData.TexID > 0)
                {
                    artUVs = UOAtlas.GetUVsTexture(landData.TexID);
                }
                else
                {
                    artUVs = UOAtlas.AddNoDraw();
                }
            }
            else
            {
                artUVs = UOAtlas.GetUVsTile(tileId);
            }

            return artUVs;
        }

        bool InRenderedBounds(int x, int z)
        {
            return _renderedArea.Contains(new Vector2(x, z));
        }

        void Update()
        {
            Input.Update();
        }

        enum IndexType
        {
            None = 1,
            Vertice = 4,
            Index = 6,
        }

        struct TileBlock
        {
            public int Header { get; set; }
            public Tile[] Tiles { get; set; }

            public TileBlock(int header, Tile[] tiles) : this()
            {
                Header = header;
                Tiles = tiles;
            }
        }

        struct Tile
        {
            public short TileId { get; set; }
            public sbyte Z { get; set; }

            public Tile(short tileId, sbyte z) : this()
            {
                TileId = tileId;
                Z = z;
            }
        }
    }
}
