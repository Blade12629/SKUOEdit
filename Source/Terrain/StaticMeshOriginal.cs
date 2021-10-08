//using Assets.Source.IO;
//using Assets.Source.Textures;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Unity.Collections;
//using UnityEngine;

//namespace Assets.Source.Terrain
//{
//    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
//    public class StaticMeshOriginal : MonoBehaviour
//    {
//        Mesh _mesh;

//        [SerializeField] Vertex[] _vertices;
//        int[] _indices;

//        [SerializeField] int _verticeIndex;
//        [SerializeField] Vector2 _uvs;

//        [SerializeField] bool _writeToFile;
//        [SerializeField] string _defaultSaveFolder;
//        [SerializeField] bool _reapplyVertices;

//        [SerializeField] uint _loadItemId;

//        uint _currentItemId;
//        int _oldVerticeIndex;
//        Vector3 _oldUVs;

//        int _texWidth;
//        int _texHeight;
//        uint _oldLoadItemId;

//        public void Init(uint itemId)
//        {
//            _mesh = GetComponent<MeshFilter>().mesh = new Mesh();

//            Vertex[] vertices = _vertices = new Vertex[]
//            {
//                new Vertex(0, 0, 0, 0f, 0f), // front, bl 0
//                new Vertex(0, 1, 0, 0f, 1f), // tl 1
//                new Vertex(1, 1, 0, 1f, 1f), // tr 2
//                new Vertex(1, 0, 0, 1f, 0f), // br 3

//                new Vertex(0, 1, 0, 0f, 0f), // top, bl 4
//                new Vertex(0, 1, 1, 0f, 1f), // tl 5
//                new Vertex(1, 1, 1, 1f, 1f), // tr 6
//                new Vertex(1, 1, 0, 1f, 0f), // br 7

//                new Vertex(1, 0, 1, 0f, 0f), // back, bl 8
//                new Vertex(1, 1, 1, 0f, 1f), // tl 9
//                new Vertex(0, 1, 1, 1f, 1f), // tr 10
//                new Vertex(0, 0, 1, 1f, 0f), // br 11

//                new Vertex(1, 0, 0, 1f, 0f), // bottom, bl 12
//                new Vertex(1, 0, 1, 1f, 1f), // tl 13
//                new Vertex(0, 0, 1, 0f, 1f), // tr 14
//                new Vertex(0, 0, 0, 0f, 0f), // br 15

//                new Vertex(0, 0, 1, 0f, 0f), // left, bl 16
//                new Vertex(0, 1, 1, 0f, 1f), // tl 17
//                new Vertex(0, 1, 0, 1f, 1f), // tr 18
//                new Vertex(0, 0, 0, 1f, 0f), // br 19

//                new Vertex(1, 0, 0, 0f, 0f), // right, bl 20
//                new Vertex(1, 1, 0, 0f, 1f), // tl 21
//                new Vertex(1, 1, 1, 1f, 1f), // tr 22
//                new Vertex(1, 0, 1, 1f, 0f), // br 23
//            };

//            int[] indices = _indices = new int[]
//            {
//                0, 1, 2, // front
//                0, 2, 3,

//                4, 5, 6, // top
//                4, 6, 7,

//                8, 9, 10, // back
//                8, 10, 11,

//                12, 13, 14, // bottom
//                12, 14, 15,

//                16, 17, 18, // left
//                16, 18, 19,

//                20, 21, 22, // right
//                20, 22, 23
//            };

//            _mesh.SetVertexBufferParams(vertices.Length, VertexLayout.Layout);

//            NativeArray<Vertex> nvertices = new NativeArray<Vertex>(vertices, Allocator.Temp);
//            _mesh.SetVertexBufferData(nvertices, 0, 0, nvertices.Length);

//            _mesh.SetIndices(indices, MeshTopology.Triangles, 0);

//            //_mesh.SetNormals(new Vector3[]
//            //{
//            //    new Vector3(0, 0, -1),
//            //    new Vector3(0, 0, -1),
//            //    new Vector3(0, 0, -1),
//            //    new Vector3(0, 0, -1),

//            //    new Vector3(0, 1, 0),
//            //    new Vector3(0, 1, 0),
//            //    new Vector3(0, 1, 0),
//            //    new Vector3(0, 1, 0),

//            //    new Vector3(0, 0, 1),
//            //    new Vector3(0, 0, 1),
//            //    new Vector3(0, 0, 1),
//            //    new Vector3(0, 0, 1),

//            //    new Vector3(0, -1, 0),
//            //    new Vector3(0, -1, 0),
//            //    new Vector3(0, -1, 0),
//            //    new Vector3(0, -1, 0),

//            //    new Vector3(-1, 0, 0),
//            //    new Vector3(-1, 0, 0),
//            //    new Vector3(-1, 0, 0),
//            //    new Vector3(-1, 0, 0),

//            //    new Vector3(1, 0, 0),
//            //    new Vector3(1, 0, 0),
//            //    new Vector3(1, 0, 0),
//            //    new Vector3(1, 0, 0),
//            //});

//            _mesh.RecalculateBounds();
//            //_mesh.RecalculateNormals();

//            Texture2D tex = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture((_currentItemId = itemId));
//            _texWidth = tex.width;
//            _texHeight = tex.height;
//            GetComponent<MeshRenderer>().material.mainTexture = tex;

//            //transform.localScale = new Vector3(1f, 2f, 1f);

//            ref Vertex v0 = ref vertices[0];
//            _oldUVs = _uvs = new Vector2(v0.UvX * _texWidth, _texHeight - v0.UvY * _texHeight);
//        }

//        void Update()
//        {
//            if (_oldLoadItemId != _loadItemId)
//            {
//                Init(_oldLoadItemId = _loadItemId);
//                return;
//            }

//            if (_vertices != null)
//            {
//                if (_oldVerticeIndex != _verticeIndex)
//                {
//                    _oldVerticeIndex = _verticeIndex;

//                    ref Vertex v = ref _vertices[_verticeIndex];
//                    _oldUVs = _uvs = new Vector2(v.UvX * _texWidth, _texHeight - v.UvY * _texHeight);
//                }

//                if (!_uvs.Equals(_oldUVs) || _reapplyVertices)
//                {
//                    _reapplyVertices = false;

//                    int verticeIndex2;

//                    switch(_verticeIndex)
//                    {
//                        default:
//                            throw new InvalidOperationException("Invalid vertice index: " + _verticeIndex);

//                        case 0: // front
//                            verticeIndex2 = 8;
//                            break;
//                        case 1:
//                            verticeIndex2 = 9;
//                            break;
//                        case 2:
//                            verticeIndex2 = 10;
//                            break;
//                        case 3:
//                            verticeIndex2 = 11;
//                            break;

//                        case 4: // top
//                            verticeIndex2 = 12;
//                            break;
//                        case 5:
//                            verticeIndex2 = 13;
//                            break;
//                        case 6:
//                            verticeIndex2 = 14;
//                            break;
//                        case 7:
//                            verticeIndex2 = 15;
//                            break;

//                        case 8: // back
//                            verticeIndex2 = 0;
//                            break;
//                        case 9:
//                            verticeIndex2 = 1;
//                            break;
//                        case 10:
//                            verticeIndex2 = 2;
//                            break;
//                        case 11:
//                            verticeIndex2 = 3;
//                            break;

//                        case 12: // bottom
//                            verticeIndex2 = 4;
//                            break;
//                        case 13:
//                            verticeIndex2 = 5;
//                            break;
//                        case 14:
//                            verticeIndex2 = 6;
//                            break;
//                        case 15:
//                            verticeIndex2 = 7;
//                            break;

//                        case 16: // left
//                            verticeIndex2 = 20;
//                            break;
//                        case 17:
//                            verticeIndex2 = 21;
//                            break;
//                        case 18:
//                            verticeIndex2 = 22;
//                            break;
//                        case 19:
//                            verticeIndex2 = 23;
//                            break;

//                        case 20: // right
//                            verticeIndex2 = 16;
//                            break;
//                        case 21:
//                            verticeIndex2 = 17;
//                            break;
//                        case 22:
//                            verticeIndex2 = 18;
//                            break;
//                        case 23:
//                            verticeIndex2 = 19;
//                            break;

//                    }

//                    ref Vertex v = ref _vertices[_verticeIndex];
//                    v.UvX = (_oldUVs.x = _uvs.x) / _texWidth;
//                    v.UvY = (_texHeight - (_oldUVs.y = _uvs.y)) / _texHeight;

//                    ref Vertex v2 = ref _vertices[verticeIndex2];
//                    v2.UvX = v.UvX;
//                    v2.UvY = v.UvY;

//                    NativeArray<Vertex> vertices = new NativeArray<Vertex>(_vertices, Allocator.Temp);
//                    _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);

//                    _mesh.RecalculateBounds();
//                    //_mesh.RecalculateNormals();
//                }

//                if (_writeToFile)
//                {
//                    _writeToFile = false;

//                    StringBuilder toWrite = new StringBuilder($"{_currentItemId}\n{_texWidth},{_texHeight}\n");

//                    for (int i = 0; i < _vertices.Length; i++)
//                    {
//                        ref Vertex v = ref _vertices[i];
//                        toWrite.Append($"{v.X},{v.Y},{v.Z}\n{v.UvX * _texWidth},{v.UvY * _texHeight}\n");
//                    }

//                    for (int i = 0; i < _indices.Length; i++)
//                    {
//                        toWrite.Append($"{_indices[i]},");
//                    }

//                    toWrite.Remove(toWrite.Length - 1, 1);
//                    toWrite.Append($"\n{transform.localScale.x},{transform.localScale.y},{transform.localScale.z}\n");

//                    string path = System.IO.Path.Combine(_defaultSaveFolder, $"0x{_currentItemId:X4}.texinfo");


//                    if (!System.IO.Directory.Exists(_defaultSaveFolder))
//                    {
//                        System.IO.Directory.CreateDirectory(_defaultSaveFolder);
//                    }
//                    else if (System.IO.File.Exists(path))
//                    {
//                        System.IO.File.Delete(path);
//                    }

//                    using (var writer = System.IO.File.CreateText(path))
//                    {
//                        writer.Write(toWrite.ToString());
//                        writer.Flush();
//                    }
//                }
//            }
//        }
//    }
//}
