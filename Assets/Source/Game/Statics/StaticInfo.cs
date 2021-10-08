using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Game.Statics
{
    public class StaticInfo
    {
        public int Id { get; }
        public Vertex[] Vertices { get; }

        public StaticInfo(int id, Vertex[] vertices)
        {
            Id = id;
            Vertices = vertices;
        }

        public static StaticInfo Load(string file)
        {
            string[] lines = File.ReadAllLines(file);

            int id = int.Parse(lines[0]);
            int length = int.Parse(lines[1]);

            Vertex[] vertices = new Vertex[length];

            for (int i = 0; i < length; i++)
            {
                string[] split = lines[i + 2].Split(',');

                vertices[i] = new Vertex(float.Parse(split[0]),
                                         float.Parse(split[1]),
                                         float.Parse(split[2]),
                                         float.Parse(split[3]),
                                         float.Parse(split[4]));
            }

            return new StaticInfo(id, vertices);
        }

        public void Save(string file)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{Id}\n{Vertices.Length}\n");

            for (int i = 0; i < Vertices.Length; i++)
            {
                ref Vertex v = ref Vertices[i];
                sb.Append($"{v.X},{v.Y},{v.Z},{v.UvX},{v.UvY}\n");
            }

            sb.Remove(sb.Length - 1, 1);

            if (File.Exists(file))
                File.Delete(file);

            File.WriteAllText(file, sb.ToString());
        }
    }
}
