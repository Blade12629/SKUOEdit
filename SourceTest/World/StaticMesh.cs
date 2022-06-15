using Assets.Source.Game.Map;
using Assets.Source.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.SourceTest.World
{
    public class StaticMesh : MonoBehaviour
    {
        [SerializeField] MeshFilter _mFilter;
        [SerializeField] MeshRenderer _mRenderer;
        [SerializeField] MeshCollider _mCollider;

        Mesh _mesh;

        public void RenderArea(Rect area, MapStatics statics)
        {
            if (_mesh == null)
            {
                _mFilter = GetComponent<MeshFilter>();
                _mRenderer = GetComponent<MeshRenderer>();
                _mCollider = GetComponent<MeshCollider>();

                _mesh = _mFilter.mesh = new Mesh();
            }
            else
            {
                _mesh.Clear();
            }

            Texture2D texture = new Texture2D(7000, 500);
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();
            List<long> lookups = new List<long>();

            int blockWidth = (int)Math.Ceiling(area.width / 8);
            int blockHeight = (int)Math.Ceiling(area.height / 8);
            int width = blockWidth * 8;
            int height = blockHeight * 8;
            int areaX = (int)(area.x / 8) * 8;
            int areaY = (int)(area.y / 8) * 8;

            for (int x = 0; x < width; x++)
            {
                float wx = (x + areaX) * .22f;

                for (int y = 0; y < height; y++)
                {
                    float wy = (y + areaY) *.22f;
                    StaticBlock block = statics.GetStaticBlock(y + areaY, x + areaX);

                    if (block == null || lookups.Contains(block.Lookup))
                        continue;

                    lookups.Add(block.Lookup);

                    for (int i = 0; i < block.Statics.Length; i++)
                    {
                        ref var s = ref block.Statics[i];
                        Vector2[] uvs = GetStatic(s.TileId, texture);

                        int vi = vertices.Count;
                        vertices.AddRange(new Vertex(wx + s.X * .22f,          wy + s.Y,           -(s.Z * 0.039f), uvs[0].x, uvs[0].y),
                                          new Vertex(wx + s.X * .22f - .22f,   wy + s.Y + .22f,    -(s.Z * 0.039f), uvs[1].x, uvs[1].y),
                                          new Vertex(wx + s.X * .22f,          wy + s.Y + .44f,    -(s.Z * 0.039f), uvs[2].x, uvs[2].y),
                                          new Vertex(wx + s.X * .22f + .22f,   wy + s.Y + .22f,    -(s.Z * 0.039f), uvs[3].x, uvs[3].y));

                        indices.AddRange(vi, vi + 1, vi + 2,
                                         vi, vi + 2, vi + 3);
                    }
                }
            }

            texture.Apply();

            Vertex[] varray = vertices.ToArray();

            _mesh.SetVertexBufferParams(varray.Length, VertexLayout.Layout);

            using (NativeArray<Vertex> verts = new NativeArray<Vertex>(varray, Allocator.Temp))
                _mesh.SetVertexBufferData(verts, 0, 0, verts.Length, 0);

            _mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();

            _mCollider.sharedMesh = _mesh;
            _mRenderer.material.mainTexture = texture;
        }

        int _nextXPos;
        Dictionary<uint, Vector2[]> _uvs;

        Vector2[] GetStatic(uint staticId, Texture2D atlasTexture)
        {
            Vector2[] uvs;

            if (_uvs == null)
                _uvs = new Dictionary<uint, Vector2[]>();
            else if (_uvs.TryGetValue(staticId, out uvs))
                return uvs;

            Texture2D sTex = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(staticId);

            if (sTex == null)
                return null;

            for (int x = 0; x < sTex.width; x++)
            {
                for (int y = 0; y < sTex.height; y++)
                {
                    Color px = sTex.GetPixel(x, y);
                    atlasTexture.SetPixel(x + _nextXPos, y, px);
                }
            }

            uvs = new Vector2[]
            {
                ToUV(_nextXPos, 0),
                ToUV(_nextXPos, sTex.height),
                ToUV(_nextXPos + sTex.width, sTex.height),
                ToUV(_nextXPos + sTex.width, 0)
            };

            _uvs[staticId] = uvs;
            _nextXPos += sTex.width;

            return uvs;

            Vector2 ToUV(int x, int y)
            {
                return new Vector2(x / atlasTexture.width, y / atlasTexture.height);
            }
        }

    }
}
