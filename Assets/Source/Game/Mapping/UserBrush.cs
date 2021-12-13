using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Mapping
{
    public class UserBrush : MappingBrush
    {
        Vector3[] _points;

        public UserBrush(string file) : base(2)
        {
            List<Vector3> points = new List<Vector3>();

            using (StreamReader sr = new StreamReader(file))
            {
                while(!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    string[] split = line.Split(',');

                    if (!int.TryParse(split[0], out int x) ||
                        !int.TryParse(split[1], out int z))
                        continue;

                    points.Add(new Vector3(x, 0, z));
                }
            }
            if (points.Count < 4)
                throw new InvalidOperationException("Brush cannot have less than 4 points, cancelled load");

            _points = points.ToArray();
        }

        public override Vector3[] GetBrushPoints()
        {
            Vector3[] result = new Vector3[_points.Length];
            Array.Copy(_points, result, result.Length);

            return result;
        }
    }
}
