using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Assets.Source.IO;
using Assets.Source.Ultima;
using Assets.Source.Textures;

namespace Assets.Source.Game.Map
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class UltimaTerrain : MonoBehaviour
    {
        public int Size
        {
            get => _size;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Size cannot be <= 0");
                else if (value == _size)
                    return;

                _size = value;
                Clear();
                SetVertices(_lastPos, true);
            }
        }

        MeshFilter _filter;
        MeshRenderer _renderer;
        MeshCollider _collider;

        Mesh _mesh;
        Vertex[] _vertices;
        int _size;
        
        MapTiles _tiles;
        Vertex[] _vertexCache;
        Vector2 _lastPos;

        public void Initialize(int size, MapTiles tiles, Material material, Texture2D atlas)
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<MeshCollider>();

            _tiles = tiles;
            _mesh = _filter.mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            _size = size;
            _vertices = new Vertex[size * size * 4];
            _renderer.material = new Material(material);
            _renderer.material.mainTexture = atlas;

            BuildCache();
        }

        public void SetVertices(Vector3 position, bool recalculateIndices)
        {
            _lastPos = position;

            Parallel.For(0, _size, xCur =>
            {
                int indexSrc = PositionToIndex((int)position.x + xCur, (int)position.z);
                int indexDest = PositionToIndex(xCur, 0, _size);
                int length = _size * 4;

                // our index cannot be less than 0, reduce length to make it fit
                if (indexSrc < 0)
                {
                    length += indexSrc;
                    indexSrc = 0;
                }

                // our length would be too big, simply get the new length
                if (indexDest + length >= _vertices.Length)
                {
                    length = _vertices.Length - indexDest;

                    if (indexSrc + length >= _vertexCache.Length)
                    {
                        length = Math.Min(length, _vertexCache.Length - indexSrc);
                    }
                }
                else if (indexSrc + length >= _vertexCache.Length)
                {
                    length = _vertexCache.Length - indexSrc;
                }

                // seems like we have no valid data to copy, just skip
                if (length <= 0)
                    return;

                // copy area vertice data
                Array.Copy(_vertexCache, indexSrc, _vertices, indexDest, length);
            });

            if (_mesh.vertexBufferCount != _vertices.Length)
            {
                _mesh.SetVertexBufferParams(_vertices.Length, VertexLayout.Layout);

                using (NativeArray<Vertex> nvertices = new NativeArray<Vertex>(_vertices, Allocator.Temp))
                    _mesh.SetVertexBufferData(_vertices, 0, 0, _vertices.Length);

                _mesh.SetIndices(GetIndices(), MeshTopology.Triangles, 0);
            }
            else
            {
                using (NativeArray<Vertex> nvertices = new NativeArray<Vertex>(_vertices, Allocator.Temp))
                    _mesh.SetVertexBufferData(_vertices, 0, 0, _vertices.Length);
            }

            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            _collider.sharedMesh = _mesh;
        }

        public void Clear()
        {
            _mesh.Clear();
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        void BuildCache()
        {
            Vertex[] vertices = new Vertex[_tiles.Width * _tiles.Depth * 4];
            Color[] minimapColors = new Color[_tiles.Width * _tiles.Depth];


            for (int x = 0; x < _tiles.Depth; x++)
            //Parallel.For(0, _tiles.Depth, x =>
            {
                for (int y = 0; y < _tiles.Width; y++)
                    //Parallel.For(0, _tiles.Width, y =>
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
                    Vector2[] uvs;

                    if (isEvenTile)
                        uvs = TileAtlas.Instance.GetTileUVs((uint)tileBL.TileId);
                    else
                        uvs = TileAtlas.Instance.GetTileTextureUVs((uint)tileBL.TileId);

                    for (int i = 0; i < 4; i++)
                    {
                        ref Vertex vertex = ref vertices[vertexIndex + i];
                        ref Vector2 uv = ref uvs[i];

                        switch (i)
                        {
                            default:
                            case 0:
                                vertex.X = x;
                                vertex.Z = y;

                                vertex.Y = tileBL.Z * 0.039f;
                                break;

                            case 1:
                                vertex.X = x;
                                vertex.Z = y + 1f;

                                vertex.Y = hTL * 0.039f;
                                break;

                            case 2:
                                vertex.X = x + 1f;
                                vertex.Z = y + 1f;

                                vertex.Y = hTR * 0.039f;
                                break;

                            case 3:
                                vertex.X = x + 1f;
                                vertex.Z = y;

                                vertex.Y = hBR * 0.039f;
                                break;
                        }

                        vertex.UvX = uv.x;
                        vertex.UvY = uv.y;
                    }
                }/*);*/
            }/*);*/

            _vertexCache = vertices;
        }

        int PositionToIndex(int x, int y)
        {
            return PositionToIndex(x, y, _tiles.Width);
        }

        int PositionToIndex(int x, int y, int height)
        {
            return (x * height + y) * 4;
        }

        int[] GetIndices()
        {
            int[] indices = new int[Size * Size * 6];

            int iv = 0;
            int i = 0;
            for (int x = 0; x < Size; x++)
            {
                for (int z = 0; z < Size; z++)
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
    }
}
