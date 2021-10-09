using Assets.Source.Textures;
using Assets.Source.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
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
        static readonly FileShare FileShareFlags = FileShare.ReadWrite | FileShare.Delete;

        public int Width { get; private set; }
        public int BlockWidth { get; private set; }
        public int Depth { get; private set; }
        public int BlockDepth { get; private set; }

        public string MapFile { get; private set; }

        public bool IsMapLoaded { get; private set; }

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

            Width = 0;
            BlockWidth = 0;
            Depth = 0;
            BlockDepth = 0;
            MapFile = null;

            _vertices = null;
            _tileBlocks = null;
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
        }

        public void Load(string file, int width, int depth, GenerationOption genOption, int[] tileHeights, int[] tileIds)
        {
            MapFile = file;
            Width = width;
            BlockWidth = width / 8;
            Depth = depth;
            BlockDepth = depth / 8;

            _chunks = new MapChunk[3 * 3];

            Debug.Log("Initializing minimap");
            Minimap.Instance.Initialize(width, depth);

            StartCoroutine(LoadMap());

            IEnumerator LoadMap()
            {
                Debug.Log("Loading map");

                Debug.Log("Loading map blocks");
                yield return new WaitForEndOfFrame();

                switch (genOption)
                {
                    case GenerationOption.Default:
                        // TODO: convert uop file to mul inside of temp directory then load mul file
                        // TODO: convert mul file uop file
                        if (MapFile.EndsWith("uop", StringComparison.CurrentCultureIgnoreCase))
                        {
                            LoadUOPMapBlocks(GetMapIndex(MapFile));
                        }
                        else 
                            LoadMapBlocks();
                        break;

                    case GenerationOption.Flatland:
                        GenerateFlatmap();
                        break;

                    case GenerationOption.Converted:
                        ConvertMapFromColors(tileHeights, tileIds);
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

                Debug.Log("Finished generating map");
                yield return new WaitForEndOfFrame();

                IsMapLoaded = true;
                OnMapFinishLoading?.Invoke();
            }
        }

        public void Save(string file = null)
        {
            if (!string.IsNullOrEmpty(file))
                MapFile = file;

            if (file.EndsWith("uop", StringComparison.CurrentCultureIgnoreCase))
                SaveUOPMapBlocks(GetMapIndex(file));
            else
                SaveMapBlocks();
        }

        public void ConvertMapFromColors(int[] tileHeights, int[] tileIds)
        {
            int headerStart = 4096;
            TileBlock[] tileBlocks = new TileBlock[BlockWidth * BlockDepth];

            for (int xb = 0; xb < BlockWidth; xb++)
            {
                int wx = xb * 8;

                for (int zb = 0; zb < BlockDepth; zb++)
                {
                    int blockIndex = xb * BlockDepth + zb;
                    int wz = zb * 8;

                    Tile[] tiles = new Tile[64];
                    int tileIndex = 0;

                    for (int x = 0; x < 8; x++)
                    {
                        for (int z = 0; z < 8; z++)
                        {
                            int srcIndex = (wx + x) * Depth + (wz + z);
                            short tileId = (short)tileIds[srcIndex];
                            sbyte height = (sbyte)tileHeights[srcIndex];

                            tiles[tileIndex++] = new Tile(tileId, height);
                        }
                    }

                    tileBlocks[blockIndex] = new TileBlock(headerStart + blockIndex, tiles);
                }
            }

            _tileBlocks = tileBlocks;
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

        void SetTileCornerHeight(int x, int z, int height, int indexOffset, bool updateChunks = true)
        {
            int index = PositionToIndex(x, z, IndexType.Vertice);

            if (index < 0 || index >= _vertices.Length)
                return;

            ref Vertex vertex = ref _vertices[index + indexOffset];
            vertex.Y = height * .1f;

            ref Tile tile = ref GetTile(z, x);
            tile.Z = (sbyte)height;

            RefreshUVs(x, z, false);

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
            tileBL.TileId = id;

            RefreshUVs(x, z);
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

        void RefreshUVs(int x, int z, bool updateChunks = true)
        {
            int vertexIndex = PositionToIndex(x, z, IndexType.Vertice);

            if (vertexIndex < 0 || vertexIndex >= _vertices.Length)
                return;

            ref Tile tileBL = ref GetTile(z, x);
            ref Tile tileTL = ref GetTile(z + 1, x);
            ref Tile tileTR = ref GetTile(z + 1, x + 1);
            ref Tile tileBR = ref GetTile(z, x + 1);

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
            const int sizePerTile = 3;
            const int sizePerBlock = 64 * 3 + 4;

            long sizePerRow = sizePerBlock * BlockDepth;
            TileBlock[] tileBlocks = new TileBlock[BlockWidth * BlockDepth];

            unsafe
            {
                File.SetAttributes(MapFile, FileAttributes.Normal);

                using (FileStream mapStream = new FileStream(MapFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (MemoryMappedFile mapMMF = MemoryMappedFile.CreateFromFile(mapStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
                using (MemoryMappedViewAccessor mapAccessor = mapMMF.CreateViewAccessor())
                {
                    byte* mapStart = null;
                    mapAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref mapStart);

                    if (mapStart == null)
                        throw new OperationCanceledException($"Unable to acquire pointer for map {MapFile}");

                    Parallel.For(0, BlockWidth, bx =>
                    {
                        byte* xblockPtr = mapStart + bx * sizePerRow;
                        int xblockIndex = bx * BlockDepth;

                        for (int bz = 0; bz < BlockDepth; bz++)
                        {
                            int header = *ReadPtr<int>(ref xblockPtr, 4);

                            Tile[] tiles = new Tile[64];
                            for (int i = 0; i < 64; i++)
                                tiles[i] = *ReadPtr<Tile>(ref xblockPtr, sizePerTile);

                            tileBlocks[xblockIndex++] = new TileBlock(header, tiles);
                        }
                    });

                    mapAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
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

        void LoadUOPMapBlocks(int mapIndex = 10)
        {
            FileInfo uopFile = new FileInfo(MapFile);
            FileInfo tempMulFile = new FileInfo(Path.Combine(uopFile.Directory.FullName, $"{uopFile.Name}.mul"));

            DeleteFile(tempMulFile.FullName);

            UoFiddler.Plugin.UopPacker.Classes.LegacyMulFileConverter converter = new UoFiddler.Plugin.UopPacker.Classes.LegacyMulFileConverter();
            converter.FromUOP(uopFile.FullName, tempMulFile.FullName, mapIndex);

            MapFile = tempMulFile.FullName;
            LoadMapBlocks();
            MapFile = uopFile.FullName;

            DeleteFile(tempMulFile.FullName);
        }

        void SaveMapBlocks()
        {
            if (File.Exists(MapFile))
                File.Delete(MapFile);

            using (FileStream fstream = new FileStream(MapFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
            using (BinaryWriter writer = new BinaryWriter(fstream))
            {
                for (int i = 0; i < _tileBlocks.Length; i++)
                {
                    ref TileBlock block = ref _tileBlocks[i];
                    writer.Write(block.Header);
                    
                    for (int x = 0; x < block.Tiles.Length; x++)
                    {
                        ref Tile tile = ref block.Tiles[x];
                        writer.Write(tile.TileId);
                        writer.Write(tile.Z);
                    }
                }

                fstream.Flush();
            }
        }

        void SaveUOPMapBlocks(int mapIndex = 10)
        {
            FileInfo uopFile = new FileInfo(MapFile);
            FileInfo tempMulFile = new FileInfo(Path.Combine(uopFile.Directory.FullName, $"{uopFile.Name}.mul"));

            DeleteFile(tempMulFile.FullName);

            MapFile = tempMulFile.FullName;
            SaveMapBlocks();
            MapFile = uopFile.FullName;

            BackupFile(uopFile.FullName, 5);
            UoFiddler.Plugin.UopPacker.Classes.LegacyMulFileConverter.ToUOP(tempMulFile.FullName, uopFile.FullName, mapIndex);

            DeleteFile(tempMulFile.FullName);
        }

        void BackupFile(string file, int maxBackups)
        {
            if (!File.Exists(file))
                return;

            // count backups
            for (int i = 0; i < maxBackups; i++)
            {
                // we have a backup slot available
                if (!File.Exists(file + i))
                {
                    File.Move(file, file + i);
                    return;
                }
            }

            // we reached max backups, delete oldest backup
            DeleteFile(file + (maxBackups - 1));

            // move every backup one iteration higher (1 -> 2)
            for (int i = maxBackups - 2; i > 0; i--)
                File.Move(file + i, file + (i + 1));

            // move file to iteration 0
            File.Move(file, file + 0);
        }

        void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
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
        void GenerateFlatmap()
        {
            int headerStart = 4096;
            TileBlock[] tileBlocks = new TileBlock[BlockWidth * BlockDepth];

            Parallel.For(0, tileBlocks.Length, tb =>
            {
                Tile[] tiles = new Tile[64];

                for (int i = 0; i < 64; i++)
                    tiles[i] = new Tile(3, 0);

                tileBlocks[tb] = new TileBlock(headerStart + tb, tiles);
            });

            _tileBlocks = tileBlocks;
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

        public enum GenerationOption
        {
            Default,
            Flatland,
            Converted
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

        [StructLayout(LayoutKind.Sequential)]
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
