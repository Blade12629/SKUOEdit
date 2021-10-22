using Assets.Source.Game.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.StaticsBuilder
{
    public class StaticBuilder : MonoBehaviour
    {
        Texture2D _texture;
        Material _material;
        MeshRenderer _renderer;
        MeshFilter _filter;
        Mesh _mesh;

        [SerializeField] Material _defaultMaterial;
        [SerializeField] List<Vertex> _vertices;
        [SerializeField] List<int> _indices;
        [SerializeField] bool _applyVertices;

        [SerializeField] int _staticToLoad;
        [SerializeField] bool _loadStatic;

        static readonly Vertex[] _defaultVertices = new Vertex[]
        {
            new Vertex(0, 0, 0, 0, 0),
            new Vertex(0, 1, 0, 0, 1),
            new Vertex(1, 1, 0, 1, 1),
            new Vertex(1, 0, 0, 1, 0),

            new Vertex(0, 1, 0, 0, 0),
            new Vertex(0, 1, 1, 0, 1),
            new Vertex(1, 1, 1, 1, 1),
            new Vertex(1, 1, 0, 1, 0),

            new Vertex(1, 0, 1, 0, 0),
            new Vertex(1, 1, 1, 0, 1),
            new Vertex(0, 1, 1, 1, 1),
            new Vertex(0, 0, 1, 1, 0),

            new Vertex(0, 0, 1, 0, 0),
            new Vertex(0, 1, 1, 0, 1),
            new Vertex(0, 1, 0, 1, 1),
            new Vertex(0, 0, 0, 1, 0),

            new Vertex(1, 0, 0, 0, 0),
            new Vertex(1, 1, 0, 0, 1),
            new Vertex(1, 1, 1, 1, 1),
            new Vertex(1, 0, 1, 1, 0)
        };

        static readonly int[] _defaultIndices = new int[]
        {
            0, 1, 2,
            0, 2, 3,

            4, 5, 6,
            4, 6, 7,

            8, 9, 10,
            8, 10, 11,

            12, 13, 14,
            12, 14, 15,

            16, 17, 18,
            16, 18, 19
        };

        [SerializeField] GameObject _content;
        [SerializeField] GameObject _rowPanelPrefab;
        [SerializeField] InputField _moveValueInput;

        [SerializeField] RawImage _texImg;
        [SerializeField] RectTransform _texImgTransform;
        [SerializeField] RectTransform _texImgBackgroundTransform;

        [SerializeField] InputField _staticIdInput;

        List<RowPanel> _rowPanels = new List<RowPanel>();

        int _currentVertice;
        float _moveValue = 1;
        uint _staticId;

        public void MirrorTextureClick()
        {
            int index = _currentVertice / 4;
            int indexIndice = index * 6;

            int index0 = _indices[indexIndice];
            int index1 = _indices[indexIndice + 1];
            int index2 = _indices[indexIndice + 2];
            int index3 = _indices[indexIndice + 3];
            int index4 = _indices[indexIndice + 4];
            int index5 = _indices[indexIndice + 5];

            _indices[indexIndice + 0] = index2;
            _indices[indexIndice + 1] = index1;
            _indices[indexIndice + 2] = index0;

            _indices[indexIndice + 3] = index5;
            _indices[indexIndice + 4] = index4;
            _indices[indexIndice + 5] = index3;

            _applyVertices = true;
        }

        public void OnRowClick(int row)
        {
            if (row >= _vertices.Count || row < 0)
                return;

            if (_currentVertice < _rowPanels.Count)
                _rowPanels[_currentVertice].GetComponent<Outline>().enabled = false;
            
            _currentVertice = row;
            _rowPanels[row].GetComponent<Outline>().enabled = true;

            Vertex pos = _vertices[_currentVertice];
            _material.SetVector("_SelectedPosition", new Vector4(pos.X, pos.Y, pos.Z, 1));
            _texImg.GetComponent<TexImage>().SetMarkerPosition(new Vector2(pos.UvX * _texImgTransform.rect.width, _texImgTransform.rect.height - pos.UvY * _texImgTransform.rect.height), false);
        }

        public void OnMoveValueChanged()
        {
            if (!float.TryParse(_moveValueInput.text, out float mv))
                return;

            _moveValue = mv;
        }

        public void AddRowClick(bool @new)
        {
            int indexStart = _vertices.Count;

            for (int i = 0; i < 4; i++)
            {
                Vertex v = _defaultVertices[i];

                if (@new)
                    v.X += 1;

                _vertices.Add(v);

                AddRowPanel(indexStart + i, v);
            }

            _indices.AddRange(indexStart, indexStart + 1, indexStart + 2,
                                  indexStart, indexStart + 2, indexStart + 3);

            OnRowClick(_rowPanels.Count - 1);

            _applyVertices = true;

        }


        public void DeleteRowClick()
        {
            if (_vertices.Count == 4)
                return;

            int index = _currentVertice / 4;
            _currentVertice = index * 4 + 3;
            int currentIndice = index * 6 + 5;

            for (int i = 0; i < 4; i++)
            {
                _vertices.RemoveAt(_currentVertice);

                RowPanel panel = _rowPanels[_currentVertice];
                Destroy(panel.gameObject);

                _rowPanels.RemoveAt(_currentVertice--);
                _indices.RemoveAt(currentIndice--);
            }

            for (int i = 0; i < 2; i++)
            {
                _indices.RemoveAt(currentIndice--);
            }

            OnRowClick(_currentVertice);
            _applyVertices = true;
        }

        public void UpClick()
        {
            Vertex v = _vertices[_currentVertice];
            v.Y += _moveValue;

            _vertices[_currentVertice] = v;
            _applyVertices = true;
            _rowPanels[_currentVertice].SetFields(y: v.Y);
        }

        public void DownClick()
        {
            Vertex v = _vertices[_currentVertice];
            v.Y -= _moveValue;

            _vertices[_currentVertice] = v;
            _applyVertices = true;
            _rowPanels[_currentVertice].SetFields(y: v.Y);
        }

        public void LeftClick()
        {
            Vertex v = _vertices[_currentVertice];
            v.X -= _moveValue;

            _vertices[_currentVertice] = v;
            _applyVertices = true;
            _rowPanels[_currentVertice].SetFields(x: v.X);
        }

        public void RightClick()
        {
            Vertex v = _vertices[_currentVertice];
            v.X += _moveValue;

            _vertices[_currentVertice] = v;
            _applyVertices = true;
            _rowPanels[_currentVertice].SetFields(x: v.X);
        }

        public void ForwardClick()
        {
            Vertex v = _vertices[_currentVertice];
            v.Z += _moveValue;

            _vertices[_currentVertice] = v;
            _applyVertices = true;
            _rowPanels[_currentVertice].SetFields(z: v.Z);
        }

        public void BackwardClick()
        {
            Vertex v = _vertices[_currentVertice];
            v.Z -= _moveValue;

            _vertices[_currentVertice] = v;
            _applyVertices = true;
            _rowPanels[_currentVertice].SetFields(z: v.Z);
        }

        public void CreateNewStatic(Texture2D tex, Material defaultMaterial = null)
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<MeshRenderer>();
                _filter = GetComponent<MeshFilter>();
            }

            if (_vertices == null)
            {
                _vertices = new List<Vertex>();
                _indices = new List<int>();
            }
            else
            {
                _vertices.Clear();
                _indices.Clear();
            }

            //_material = _renderer.sharedMaterial = new Material(defaultMaterial ?? _defaultMaterial);
            _material = _renderer.sharedMaterial;

            if (tex != null)
            {
                _texImg.texture = _material.mainTexture = _texture = tex;
                
                Vector2 size = new Vector2(tex.width * 8, tex.height * 8);

                _texImgTransform.sizeDelta = size;
                _texImgBackgroundTransform.sizeDelta = size + new Vector2(6, 6);
            }
            
            _mesh = _filter.mesh = new Mesh();

            AddRowClick(false);
            _applyVertices = true;
        }

        public void OnImageClick(Vector2 pos)
        {
            Vertex v = _vertices[_currentVertice];
            v.UvX = pos.x / _texImgTransform.rect.width;
            v.UvY = (_texImgTransform.rect.height - pos.y) / _texImgTransform.rect.height;
            _vertices[_currentVertice] = v;

            pos /= 1f;

            _texImg.GetComponent<TexImage>().SetMarkerPosition(pos);
            _applyVertices = true;
        }

        public void LoadStatic()
        {
            if (!uint.TryParse(_staticIdInput.text, out uint staticId))
                return;

            _currentVertice = 0;
            _mesh = null;

            while(_rowPanels.Count > 0)
            {
                Destroy(_rowPanels[0].gameObject);
                _rowPanels.RemoveAt(0);
            }

            _staticId = staticId;
            CreateNewStatic(ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(staticId));

            string dirPath = Path.Combine(Environment.CurrentDirectory, "Statics");

            if (Directory.Exists(dirPath))
            {
                string filePath = Path.Combine(dirPath, $"{_staticId}.stinfo");

                if (File.Exists(filePath))
                {
                    while (_rowPanels.Count > 0)
                    {
                        Destroy(_rowPanels[0].gameObject);
                        _rowPanels.RemoveAt(0);
                    }

                    List<int> indices = new List<int>();
                    List<Vertex> vertices = new List<Vertex>();

                    string[] lines = File.ReadAllLines(filePath);
                    bool isvertice = true;
                    int skippedLines = 0;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        if (string.IsNullOrEmpty(line))
                        {
                            skippedLines++;
                            continue;
                        }

                        if (isvertice)
                        {
                            if (line.Equals("---"))
                            {
                                isvertice = false;
                                continue;
                            }

                            string[] vertSplit = line.Split(',');
                            Vertex v = new Vertex(float.Parse(vertSplit[0]),
                                                  float.Parse(vertSplit[1]),
                                                  float.Parse(vertSplit[2]),
                                                  float.Parse(vertSplit[3]),
                                                  float.Parse(vertSplit[4]));

                            vertices.Add(v);
                            AddRowPanel(i - skippedLines, v);
                        }
                        else
                        {
                            string[] indicesSplit = line.Split(',');

                            for (int x = 0; x < indicesSplit.Length; x++)
                                indices.Add(int.Parse(indicesSplit[x]));
                        }
                    }

                    if (vertices.Count > 0)
                    {
                        _vertices = vertices;
                        _indices = indices;
                        _applyVertices = true;
                    }
                }
            }
        }

        public void SaveStatic()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _vertices.Count; i++)
            {
                Vertex v = _vertices[i];
                sb.AppendLine($"{v.X},{v.Y},{v.Z},{v.UvX},{v.UvY}");
            }

            sb.AppendLine("---");

            for (int i = 0; i < _indices.Count; i++)
            {
                sb.Append($"{_indices[i]},");
            }

            sb.Remove(sb.Length - 1, 1);

            string dirPath = Path.Combine(Environment.CurrentDirectory, "Statics");

            if (!Directory.Exists("Statics"))
                Directory.CreateDirectory("Statics");

            string filePath = Path.Combine(dirPath, $"{_staticId}.stinfo");

            if (File.Exists(filePath))
                File.Delete(filePath);

            File.WriteAllText(filePath, sb.ToString());
        }

        public void LoadMesh(Vertex[] vertices, int[] indices)
        {
            while (_rowPanels.Count > 0)
            {
                Destroy(_rowPanels[0].gameObject);
                _rowPanels.RemoveAt(0);
            }

            _currentVertice = 0;

            _vertices = new List<Vertex>(vertices);
            _indices = new List<int>(indices);

            for (int i = 0; i < vertices.Length; i++)
            {
                AddRowPanel(i, vertices[i]);
            }

            _applyVertices = true;
        }

        public (Vertex[], int[]) GetMeshData()
        {
            Vertex[] resultA = new Vertex[_vertices.Count];
            int[] resultB = new int[_indices.Count];

            for (int i = 0; i < _vertices.Count; i++)
                resultA[i] = _vertices[i];

            for (int i = 0; i < _indices.Count; i++)
                resultB[i] = _indices[i];

            return (resultA, resultB);
        }

        void AddRowPanel(int index, Vertex v)
        {
            GameObject rowPanelObj = Instantiate(_rowPanelPrefab);
            rowPanelObj.SetActive(true);
            rowPanelObj.transform.SetParent(_content.transform);

            RowPanel rowPanel = rowPanelObj.GetComponent<RowPanel>();
            _rowPanels.Add(rowPanel);
            rowPanel.OnXUpdated += x => UpdateVertex(index, x: x);
            rowPanel.OnYUpdated += y => UpdateVertex(index, y: y);
            rowPanel.OnZUpdated += z => UpdateVertex(index, z: z);

            rowPanel.SetRow(index, this);
            rowPanel.SetFields(v.X, v.Y, v.Z);

            void UpdateVertex(int index, float? x = null, float? y = null, float? z = null)
            {
                Vertex v = _vertices[index];

                if (x.HasValue)
                    v.X = x.Value;

                if (y.HasValue)
                    v.Y = y.Value;

                if (z.HasValue)
                    v.Z = z.Value;

                _vertices[index] = v;
                _applyVertices = true;
            }
        }

        void Update()
        {
            if (_applyVertices)
            {
                _applyVertices = false;

                _mesh.SetVertexBufferParams(_vertices.Count, VertexLayout.Layout);
                _mesh.SetVertexBufferData(_vertices, 0, 0, _vertices.Count);
                _mesh.SetIndices(_indices, MeshTopology.Triangles, 0);
                _mesh.RecalculateBounds();
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                        SaveStatic();

                    _staticIdInput.SetTextWithoutNotify((--_staticId).ToString());
                    LoadStatic();
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!Input.GetKey(KeyCode.LeftShift))
                        SaveStatic();

                    _staticIdInput.SetTextWithoutNotify((++_staticId).ToString());
                    LoadStatic();
                }
                else if (Input.GetKeyDown(KeyCode.Y))
                {
                    OnRowClick(_currentVertice - 1);
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    OnRowClick(_currentVertice + 1);
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    LoadStatic();
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    SaveStatic();
                }
            }
        }

        void Start()
        {
            if (GameConfig.ConfigNotFound)
            {
                Debug.LogError("Config not found, saving config and exiting client");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

                return;
            }

            GameFiles.LoadClientFiles();
        }
    }
}
