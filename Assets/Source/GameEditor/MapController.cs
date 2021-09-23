using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Source.Rendering;
using Assets.Source.Terrain;
using Assets.Source.Textures;
//using SKMapGenerator.Ultima;
using System.Collections;
using Assets.Source.IO;
using UnityEngine.UI;

namespace Assets.Source.GameEditor
{
    public unsafe class MapController
    {
        /// <summary>
        /// Map offset
        /// </summary>
        public int XOffset
        {
            get => _xOffset;
            set
            {
                if (value == _xOffset/* || value < 0*/)
                    return;

                _xOffset = value;
                _hasMovedMap = true;
            }
        }

        /// <summary>
        /// Map offset
        /// </summary>
        public int ZOffset
        {
            get => _zOffset;
            set
            {
                if (value == _zOffset/* || value < 0*/)
                    return;

                _zOffset = value;
                _hasMovedMap = true;
            }
        }

        /// <summary>
        /// Offsets for 4 tile vertices (bottom left, top left, top right, bottom right)
        /// </summary>
        static readonly int[] _offsets = new int[]
        {
            0, 0,
            0, 1,
            1, 1,
            1, 0
        };

        public bool IsMapBuilt { get; private set; }

        public Minimap Minimap => _minimap;

        int _xOffset;
        int _zOffset;
        //int _mapWidth;
        //int _mapDepth;

        /// <summary>
        /// If true regenerates all heights and uvs on the next update
        /// </summary>
        bool _hasMovedMap;

        CameraController _camera;
        HeightTable _heights;
        TerrainMesh _terrainMesh;
        VertexMap _map;
        TileMatrix _tileMatrix;
        StaticTileMatrix _staticTileMatrix;

        Minimap _minimap;
        GameObject _staticsHolder;

        public MapController(TerrainMesh terrainMesh, GameObject staticsHolder)
        {
            _terrainMesh = terrainMesh;
            _staticsHolder = staticsHolder;
        }

        public void SetupMapCamera(CameraController camera)
        {
            _camera = camera;
            camera.OnCameraMove += v =>
            {
                XOffset += (int)v.x;
                ZOffset += (int)v.z;
            };
        }

        public void BuildMap(CameraController camera, TileMatrix tileMatrix, StaticTileMatrix staticTileMatrix, HeightTable heights, RawImage minimapImg, RawImage minimapMarkerImg, int mapWidth, int mapDepth, EditorSceneController controller)
        {
            controller.StartCoroutine(BuildMap(camera, tileMatrix, staticTileMatrix, heights, minimapImg, minimapMarkerImg, mapWidth, mapDepth));
        }

        public void SpawnArea(int x, int z, int width, int depth, StaticTileMatrix staticTileMatrix)
        {
            List<uint> missingItemIds = new List<uint>();

            int xblock = x / 8;
            int xblockEnd = xblock + (int)Math.Ceiling(width / 8.0);

            int zblock = z / 8;
            int zblockEnd = zblock + (int)Math.Ceiling(depth / 8.0);

            for (int xblockcur = xblock; xblockcur < xblockEnd; xblockcur++)
            {
                for (int zblockcur = zblock; zblockcur < zblockEnd; zblockcur++)
                {
                    StaticBlock stblock = staticTileMatrix.Statics[xblockcur * staticTileMatrix.BlockHeight + zblockcur];
                    SpawnStaticBlock(zblockcur * 8, xblockcur * 8, stblock.Statics, missingItemIds);
                }
            }

            missingItemIds = missingItemIds.OrderBy(v => v).ToList();

            StringBuilder missingIds = new StringBuilder();

            for (int i = 0; i < missingItemIds.Count; i++)
            {
                missingIds.Append($"{missingItemIds[i]}, ");

                if (i > 0 && i % 10 == 0)
                {
                    missingIds.Remove(missingIds.Length - 2, 2);
                    missingIds.Append("\n");
                }
            }

            Debug.Log($"Missing itemIds ({missingItemIds.Count}): {missingIds}");
        }

        public void Update()
        {
            if (_hasMovedMap)
            {
                _hasMovedMap = false;

                _terrainMesh.MapOffsetX = _xOffset;
                _terrainMesh.MapOffsetZ = _zOffset;
                _terrainMesh.ApplyNew(true);
            }
        }

        public void MoveToPosition(float x, float z, bool subtractCameraOffset = false)
        {
            if (subtractCameraOffset)
            {
                x -= _camera.CameraOffsetX;
                z -= _camera.CameraOffsetZ;
            }

            _xOffset = (int)x;
            _zOffset = (int)z;
            _hasMovedMap = true;
            _camera.InvalidatePosition(new Vector3(XOffset, 0f, ZOffset));

            //SpawnArea(XOffset, ZOffset, _terrainMesh.Width, _terrainMesh.Depth);
        }

        public void SetVerticeHeight(float x, float z, float h)
        {
            FixCoordinates(ref x, ref z);
            InternalSetHeight(x, z, h);
            _terrainMesh.ApplyNew(true);
        }

        public void IncreaseVerticeHeight(float x, float z, float h)
        {
            FixCoordinates(ref x, ref z);
            float oh = _map.GetHeight((int)x, (int)z);
            InternalSetHeight(x, z, oh + h);
            _terrainMesh.ApplyNew(true);
        }

        public void SetTileHeight(float x, float z, float h)
        {
            //FixCoordinates(ref x, ref z);
            //x--;
            //z--;

            x = (int)x;
            z = (int)z;

            InternalSetHeight(x,       z,       h);
            InternalSetHeight(x,       z + 1,   h);
            InternalSetHeight(x + 1,   z + 1,   h);
            InternalSetHeight(x + 1,   z,       h);

            _terrainMesh.ApplyNew(true);
        }

        public void SetTileId(float x, float z, short tileId)
        {
            FixCoordinates(ref x, ref z);

            bool isEven = _heights.IsEvenHeight((int)x, (int)z);
            _map.SetUVs((int)x, (int)z, GetTileUVs(tileId, isEven));

            ref var tile = ref _tileMatrix.GetTile((int)z, (int)x);
            tile.TileId = tileId;

            ushort radCol = ClassicUO.IO.Resources.HuesLoader.Instance.GetRadarColorData(tile.TileId);
            Color color = GameTextures.FromHue(radCol);

            _minimap.SetTile((int)x, (int)z, color);
            _minimap.Apply();
        }

        public void IncreaseTileHeight(float x, float z, float h)
        {
            //FixCoordinates(ref x, ref z);
            //x--;
            //z--;

            x = (int)x;
            z = (int)z;

            float h0 = _map.GetHeight((int)x,       (int)z);
            float h1 = _map.GetHeight((int)x,       (int)z + 1);
            float h2 = _map.GetHeight((int)x + 1,   (int)z + 1);
            float h3 = _map.GetHeight((int)x + 1,   (int)z);


            InternalSetHeight(x,        z,      h0 + h);
            InternalSetHeight(x,        z + 1,  h1 + h);
            InternalSetHeight(x + 1,    z + 1,  h2 + h);
            InternalSetHeight(x + 1,    z,      h3 + h);
            _terrainMesh.ApplyNew(true);
        }

        public void SpawnStatic(uint id, Vector3 position, List<uint> missingItemIds = null)
        {
#if UNITY_EDITOR
            string pathTexInfo = $"Assets\\TexInfo\\0x{id:X4}.texinfo";
#else
            string pathTexInfo = $"TexInfo\\0x{id:X4}.texinfo";
#endif

            if (!System.IO.File.Exists(pathTexInfo))
            {
                if (missingItemIds != null && !missingItemIds.Contains(id))
                {
                    missingItemIds.Add(id);
                }
                return;
            }

            Debug.Log("Static spawning not yet implemented");

            // TODO:    load texinfo for static
            //          create the mesh
            //          apply texinfo to mesh and object
            //          move object into world

            //GameObject st = new GameObject($"St {id}", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            //st.transform.position = position;
        }

        void SpawnStaticBlock(int worldX, int worldZ, Static[] statics, List<uint> missingItemIds)
        {
            for (int i = 0; i < statics.Length; i++)
            {
                ref Static st = ref statics[i];

                if (st.IsNullRef)
                {
                    continue;
                }

                SpawnStatic(st.TileId, new Vector3(worldX + st.Y, st.Z * 0.1f, worldZ + st.X), missingItemIds);
            }
        }

        void Internal_SetTilePhase1(ref int x, ref int z, TileMatrix tileMatrix, HeightTable heights)
        {
            ref var tile = ref tileMatrix.GetTile(z, x);
            heights.SetHeight(x, z, tile.Z);

            ushort radCol = ClassicUO.IO.Resources.HuesLoader.Instance.GetRadarColorData(tile.TileId);
            Color color = GameTextures.FromHue(radCol);

            _minimap.SetTile(x, z, color);
        }

        void Internal_SetTilePhase2(ref int x, ref int z, TileMatrix tileMatrix, HeightTable heights)
        {
            ref var tile = ref tileMatrix.GetTile(z, x);
            bool tryGetTexture = !heights.IsEvenHeight(x, z);

            Vector2[] artUVs = GetTileUVs(tile.TileId, tryGetTexture);

            // do this for each of our 4 vertices
            int ioffset = 0;
            for (int i = 0; i < 4; i++)
            {
                ref Vertex v = ref _map.Get(x, z, i);
                ref Vector2 uv = ref artUVs[i];
                v.X = x + _offsets[ioffset++];
                v.Z = z + _offsets[ioffset++];
                v.UvX = uv.x;
                v.UvY = uv.y;
                v.Y = heights.GetHeight(v.X, v.Z) * .1f;
            }
        }

        void Internal_BuildStaticsPhase(ref StaticBlock block, ref int xblock, ref int zblock)
        {
            Vector3 worldPos = new Vector3(zblock * 8, 0, xblock * 8);

            int total = block.Statics.Length;
            fixed (Static* stb = block.Statics)
            {
                for (int i = 0; i < total; i++)
                {
                    Static st = *(stb + i);

                    if (st.IsNullRef || st.WriteZeroStaticId)
                        continue;

                    Vector3 localStaticPos = new Vector3(st.Y, st.Z, st.X);
                    Vector3 worldStaticPos = worldPos + localStaticPos;

                    SpawnStatic(st.TileId, worldStaticPos);
                }
            }
        }

        IEnumerator BuildMap(CameraController camera, TileMatrix tileMatrix, StaticTileMatrix staticTileMatrix, HeightTable heights, RawImage minimapImg, RawImage minimapMarkerImg, int mapWidth, int mapDepth)
        {
            Debug.Log("Setting up vertex map");
            IsMapBuilt = false;

            _heights = heights;
            _tileMatrix = tileMatrix;
            _staticTileMatrix = staticTileMatrix;
            _map = new VertexMap(mapWidth, mapDepth);
            _minimap = new Minimap(minimapImg, minimapMarkerImg, mapWidth, mapDepth);


            Debug.Log("Loading map heights");
            // first we loop through once so we set our heighttable
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapDepth; z++)
                {
                    // bypass since we cannot have references in enumerators
                    Internal_SetTilePhase1(ref x, ref z, tileMatrix, heights);
                }
            }

            Debug.Log("Creating minimap");
            _minimap.Apply();

            yield return new WaitForEndOfFrame();

            Debug.Log($"Loading map tiles");
            // now we loop through again and set our tiles based on the heights we received before
            for (int x = 0; x < mapWidth; x++)
            {
                for (int z = 0; z < mapDepth; z++)
                {
                    Internal_SetTilePhase2(ref x, ref z, tileMatrix, heights);
                }
            }

            Debug.Log("Vertex map successfully setup, generating map");
            yield return new WaitForEndOfFrame();

            // generate the base mesh
            ScreenPosition.GetScreenPoints(out Vector3 min, out Vector3 max);
            _terrainMesh.SetBase(UOAtlas.AtlasTexture, _map, (int)Math.Ceiling(max.x - min.x), (int)Math.Ceiling(max.y - min.y));

            Debug.Log("Map built");

            Debug.Log("Updating camera offset");

            // move and fit the mesh into our screen
            camera.CameraOffsetX = (_terrainMesh.Width - _terrainMesh.Width / 4);
            camera.CameraOffsetZ = (_terrainMesh.Depth - _terrainMesh.Depth / 4);

            camera.InvalidatePosition(new Vector3(XOffset, 0f, ZOffset));

            // Update the map on next frame
            _hasMovedMap = true;
            IsMapBuilt = true;

            Debug.Log("--- Finished map building process ---");
        }

        void InternalSetHeight(float x, float z, float h)
        {
            h = Mathf.Min(12.7f, Mathf.Max(-12.7f, h));

            int ix = (int)x;
            int iz = (int)z;

            if (!_heights.SetHeight(ix, iz, (int)(h * 10f)))
            {
                Debug.LogError($"Position does not exist in heighttable ({ix}/{iz})");
                return;
            }

            bool tryGetTexture = !_heights.IsEvenHeight(x, z);
            ref var tile = ref _tileMatrix.GetTile((int)z, (int)x);
            tile.Z = (sbyte)h;

            _map.SetUVs(ix, iz, GetTileUVs(tile.TileId, tryGetTexture));

            if (ix > 0)
            {
                UpdateUV(ix - 1, iz);

                if (iz > 0)
                {
                    UpdateUV(ix - 1, iz - 1);
                }
            }
            if (iz > 0)
            {
                UpdateUV(ix, iz - 1);
            }

            _map.SetHeight(ix, iz, h);

            void UpdateUV(int x2, int z2)
            {
                bool tryGetTexture2 = !_heights.IsEvenHeight(x2, z2);
                ref var tile2 = ref _tileMatrix.GetTile(z2, x2);

                _map.SetUVs(x2, z2, GetTileUVs(tile2.TileId, tryGetTexture2));
            }
        }

        Vector2[] GetTileUVs(short tileId, bool tryGetTexture)
        {
            Vector2[] artUVs;
            if (tryGetTexture)
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

        void FixCoordinates(ref float x, ref float z)
        {
            x = Mathf.Ceil(x);
            z = Mathf.Ceil(z);
        }
    }
}
