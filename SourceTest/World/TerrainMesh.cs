//#define LOG_MESH_VERTS_DEBUG

using Assets.Source.Game.Map;
using Assets.Source.IO;
using Assets.Source.Textures;
//using Assets.SourceTest.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.SourceTest.World
{
    public class TerrainMesh : MonoBehaviour
    {
        public const float TILE_OFFSET_FULL = .44f;
        public const float TILE_OFFSET_HALF = .22f;
        public const float TILE_OFFSET_SMALL = .11f;
        public const float TILE_OFFSET_STEP = TILE_OFFSET_FULL * .1f;

        public const float CAMERA_TO_MESH_OFFSET = 1 / .44f;

        [SerializeField] MeshFilter _mFilter;
        [SerializeField] MeshRenderer _mRenderer;
        [SerializeField] MeshCollider _mCollider;

        //TextureAtlas _textures;
        MapTiles _tiles;
        Vertex[] _vertices;

        [SerializeField] int _mXOffset;
        [SerializeField] int _mYOffset;
        int _mWidth;
        int _mHeight;
        Vertex[] _mVertexCache;
        Mesh _mesh;

        [SerializeField] bool _rebuildMesh;

        private void Update()
        {
            if (!_rebuildMesh)
                return;

            _rebuildMesh = false;
            WorldCamera.Instance.MoveToWorld(new Vector3(_mXOffset, _mYOffset));
            //BuildMesh(_mWidth, _mHeight, _mXOffset, _mYOffset);
        }


        /// <summary>
        /// Builds the mesh from ground up, needed incase we resize or initialize the mesh
        /// </summary>
        /// <param name="width">map width</param>
        /// <param name="height">map height</param>
        /// <param name="xOffset">map x offset</param>
        /// <param name="yOffset">map y offset</param>
        public void BuildMesh(int width, int height, int xOffset, int yOffset)
        {
            Clear();

            _mWidth = width;
            _mHeight = height;
            _mXOffset = xOffset;
            _mYOffset = yOffset;

            int totalTiles = width * height;
            int totalVertices = totalTiles * 4;
            int totalIndices = totalTiles * 6;

            _mesh.SetVertexBufferParams(totalVertices, VertexLayout.Layout);
            _mVertexCache = new Vertex[totalVertices];

            GetAreaVertices(_mVertexCache);
            ApplyMesh(true);

            Matrix4x4 m = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, -180f));
            _mRenderer.material.SetMatrix("_RotationMatrix", m);
        }

        /// <summary>
        /// Moves the rendered part of the mesh to a specific map offset
        /// </summary>
        public void MoveToOffset(int xOffset, int yOffset)
        {
            yOffset = (int)(yOffset * CAMERA_TO_MESH_OFFSET);
            xOffset = (int)(xOffset * CAMERA_TO_MESH_OFFSET);

            int wy = yOffset + -xOffset;
            int wx = yOffset - -xOffset;

            BuildMesh(_mWidth, _mHeight, wx, wy);

            // TODO: Fix updating mesh without having to rebuild the entire mesh
            //GetAreaVertices(_mVertexCache);
            //ApplyMesh(false);
        }

        public void BuildCache(MapTiles tiles/*, TextureAtlas textures*/)
        {
            _tiles = tiles;
            //_textures = textures;

            if (_mesh == null)
                _mesh = _mFilter.mesh = new Mesh();

            // make sure we copy it and not just use it
            _mRenderer.material = new Material(_mRenderer.material);
            //_mRenderer.material.mainTexture = _textures.Texture;
            _mRenderer.material.mainTexture = GameTextures.GameTexture;

            Vertex[] vertices = new Vertex[_tiles.Width * _tiles.Depth * 4];
            Color[] minimapColors = new Color[_tiles.Width * _tiles.Depth];

            Parallel.For(0, _tiles.Depth, x =>
            {
                Parallel.For(0, _tiles.Width, y =>
                {
                    ref Tile tileBL = ref _tiles.GetTile(x, y);

                    sbyte hTL = 0;
                    sbyte hTR = 0;
                    sbyte hBR = 0;

                    if (y < _tiles.Width - 1)
                    {
                        ref Tile tileTL = ref _tiles.GetTile(x, y + 1);
                        hTL = tileTL.Z;

                        if (x < _tiles.Depth - 1)
                        {
                            ref Tile tileTR = ref _tiles.GetTile(x + 1, y + 1);
                            hTR = tileTR.Z;
                        }
                        else
                            hTR = tileBL.Z;
                    }
                    else
                        hTL = tileBL.Z;

                    if (x < _tiles.Depth - 1)
                    {
                        ref Tile tileBR = ref _tiles.GetTile(x + 1, y);
                        hBR = tileBR.Z;
                    }
                    else
                        hBR = tileBL.Z;


                    bool isEvenTile = tileBL.Z == hTL &&
                                      tileBL.Z == hTR &&
                                      tileBL.Z == hBR;

                    int vertexIndex = PositionToIndex(x, y);
                    Vector2[] uvs = GetTileUVs(tileBL.TileId, !isEvenTile);
                    float wx = (x - y) * TILE_OFFSET_HALF;
                    float wy = (x + y) * TILE_OFFSET_HALF;

                    for (int i = 0; i < 4; i++)
                    {
                        ref Vertex vertex = ref vertices[vertexIndex + i];
                        ref Vector2 uv = ref uvs[i];

                        switch (i)
                        {
                            default:
                            case 0:
                                vertex.X = wx;
                                vertex.Y = wy;

                                vertex.Z = -(tileBL.Z * 0.039f);
                                break;

                            case 1:
                                vertex.X = wx - TILE_OFFSET_HALF;
                                vertex.Y = wy + TILE_OFFSET_HALF;

                                vertex.Z = -(hTL * 0.039f);
                                break;

                            case 2:
                                vertex.X = wx;
                                vertex.Y = wy + TILE_OFFSET_FULL;

                                vertex.Z = -(hTR * 0.039f);
                                break;

                            case 3:
                                vertex.X = wx + TILE_OFFSET_HALF;
                                vertex.Y = wy + TILE_OFFSET_HALF;

                                vertex.Z = -(hBR * 0.039f);
                                break;
                        }

                        vertex.UvX = uv.x;
                        vertex.UvY = uv.y;
                    }
                });
            });

            _vertices = vertices;
        }

        public void Clear()
        {

        }

        void ApplyMesh(bool applyIndices)
        {
#if LOG_MESH_VERTS_DEBUG
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 32; i++)
            {
                ref Vertex v = ref _mVertexCache[i];
                sb.AppendLine($"Vertex {i}:\t\t{v.X}\t\t{v.Y}\t\t{v.Z}\t\t{v.UvX}\t\t{v.UvY}");
            }

            System.IO.File.WriteAllText("meshVertDump.txt", sb.ToString());
#endif

            using (NativeArray<Vertex> vertices = new NativeArray<Vertex>(_mVertexCache, Allocator.Temp))
                _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

            if (applyIndices)
                _mesh.SetIndices(GetIndices(), MeshTopology.Triangles, 0);

            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();

            _mCollider.sharedMesh = _mesh;
        }

        void GetAreaVertices(Vertex[] dest)
        {
            Parallel.For(0, _mWidth, xCur =>
            {
                int indexSrc = PositionToIndex(_mXOffset + xCur, _mYOffset);
                int indexDest = PositionToIndex(xCur, 0, _mHeight);
                int length = _mHeight * 4;

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

        int[] GetIndices()
        {
            int[] indices = new int[_mWidth * _mHeight * 6];

            int iv = 0;
            int i = 0;
            for (int x = 0; x < _mWidth; x++)
            {
                for (int z = 0; z < _mHeight; z++)
                {
                    indices[i++] = iv + 0;
                    indices[i++] = iv + 1;
                    indices[i++] = iv + 2;

                    indices[i++] = iv + 0;
                    indices[i++] = iv + 2;
                    indices[i++] = iv + 3;

                    iv += 4;
                }
            }

            return indices;
        }

        Vector2[] GetTileUVs(int tileId, bool isStretchedTexture)
        {
            Vector2[] uvs = null;

            if (isStretchedTexture)
            {
                ref var landData = ref ClassicUO.IO.Resources.TileDataLoader.Instance.LandData[tileId];

                if (landData.TexID > 0)
                {
                    uvs = GameTextures.GetUVsTexture(landData.TexID);
                    //uvs = _textures.AddOrGetTexture(landData.TexID);
                }
                else
                {
                    uvs = GameTextures.AddNoDraw();
                    //uvs = _textures.AddOrGetTile(int.MaxValue);
                }
            }
            else
            {
                uvs = GameTextures.GetUVsTile(tileId);
                //uvs = _textures.AddOrGetTile(tileId);
            }

            return uvs;
        }

        int PositionToIndex(int x, int z)
        {
            return PositionToIndex(x, z, _tiles.Width);
        }

        int PositionToIndex(int x, int z, int height)
        {
            return (x * height + z) * 4;
        }
    }
}
