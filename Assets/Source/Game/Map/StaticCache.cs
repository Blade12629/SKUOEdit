using Assets.Source.Game.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Game.Map
{
    /// <summary>
    /// Simple class which allows to get static entries while only loading each static id once
    /// </summary>
    public static class StaticCache
    {
        static Dictionary<uint, StaticCacheEntry> _entries = new Dictionary<uint, StaticCacheEntry>();

        /// <summary>
        /// Gets an already cached entry or loads, caches and then returns the entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns null if static entry does not exist</returns>
        public static StaticCacheEntry Get(uint id)
        {
            if (!_entries.TryGetValue(id, out StaticCacheEntry entry))
            {
                entry = Load(id);

                if (entry != null)
                    _entries.Add(id, entry);
            }

            return entry;
        }

        /// <summary>
        /// Clears the cache from all entries
        /// </summary>
        public static void ClearCache()
        {
            _entries.Clear();
        }

        static StaticCacheEntry Load(uint id)
        {
            string staticInfoFile = Path.Combine($"Statics", $"{id}.stinfo");

            if (!File.Exists(staticInfoFile))
                return null;

            List<int> indices = new List<int>();
            List<Vertex> vertices = new List<Vertex>();
            string[] lines = File.ReadAllLines(staticInfoFile);
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

            if (vertices.Count < 4)
                return null;

            return new StaticCacheEntry(id, indices.ToArray(), vertices.ToArray());
        }
    }

    public class StaticCacheEntry
    {
        public uint StaticId { get; }
        public int[] Indices { get; }
        public Vertex[] Vertices { get; }

        public StaticCacheEntry(uint staticId, int[] indices, Vertex[] vertices)
        {
            StaticId = staticId;
            Indices = indices;
            Vertices = vertices;
        }
    }
}
