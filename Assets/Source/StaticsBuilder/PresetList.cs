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
    public class PresetList : MonoBehaviour
    {
        [SerializeField] GameObject _content;
        [SerializeField] Button _presetButtonPrefab;
        [SerializeField] StaticBuilder _staticBuilder;

        public void AddCurrentPresetClick()
        {
            (Vertex[], int[]) meshData = _staticBuilder.GetMeshData();
            PresetEntry entry = new PresetEntry(null, meshData.Item1, meshData.Item2);
            entry.Save(() =>
            {
                AddPreset(entry);
            });
        }

        public void AddPreset(PresetEntry preset)
        {
            Button button = Instantiate(_presetButtonPrefab);
            button.onClick.AddListener(() => CreatePresetInWorld(preset));
            button.GetComponentInChildren<Text>().text = $"{preset.Name}";

            button.transform.SetParent(_content.transform);
            button.gameObject.SetActive(true);
        }

        public void CreatePresetInWorld(PresetEntry entry)
        {
            _staticBuilder.LoadMesh(entry.Vertices, entry.Indices);
        }

        private void Start()
        {
            if (Directory.Exists("Presets"))
            {
                foreach(FileInfo file in new DirectoryInfo("Presets").EnumerateFiles().Where(f => !f.Extension.EndsWith("meta")))
                {
                    PresetEntry entry = new PresetEntry(file.FullName);
                    AddPreset(entry);
                }
            }
        }
    }

    public class PresetEntry
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get => _vertices; set => _vertices = value; }
        public int[] Indices { get => _indices; set => _indices = value; }

        Vertex[] _vertices;
        int[] _indices;
        
        public PresetEntry(string filePath)
        {
            Name = new FileInfo(filePath).Name;

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
                _vertices = vertices.ToArray();
                _indices = indices.ToArray();
            }
        }

        /// <summary>
        /// Creates a preset out of an existing model, vertices and indices are copied into a new array
        /// </summary>
        public PresetEntry(string name, Vertex[] vertices, int[] indices)
        {
            Name = name;
            _vertices = new Vertex[vertices.Length];
            _indices = new int[indices.Length];

            Array.Copy(vertices, _vertices, vertices.Length);
            Array.Copy(indices, _indices, indices.Length);
        }

        public void Save(Action onCompleted)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _vertices.Length; i++)
            {
                Vertex v = _vertices[i];
                sb.AppendLine($"{v.X},{v.Y},{v.Z},{v.UvX},{v.UvY}");
            }

            sb.AppendLine("---");

            for (int i = 0; i < _indices.Length; i++)
            {
                sb.Append($"{_indices[i]},");
            }

            sb.Remove(sb.Length - 1, 1);

            string dirPath = Path.Combine(Environment.CurrentDirectory, "Presets");

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            IO.FileBrowser.SaveFile("Save Preset", filePath =>
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                File.WriteAllText(filePath, sb.ToString());
                Name = new FileInfo(filePath).Name;
                onCompleted?.Invoke();
            }, null);
        }
    }
}
