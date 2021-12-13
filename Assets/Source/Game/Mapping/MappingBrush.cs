using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Mapping
{
    public abstract class MappingBrush
    {
        public int Size
        {
            get => _size;
            set => Math.Max(1, value);
        }

        int _size;

        public MappingBrush(int size)
        {
            Size = size;
        }

        public abstract Vector3[] GetBrushPoints();

        public Vector3[] GetAllPointsWithLinePoints(Vector3 offset)
        {
            Vector3[] brushPoints = GetBrushPoints();

            if (Size <= 1)
                return brushPoints;

            List<Vector3> result = new List<Vector3>(brushPoints.Length);
        
            for (int i = 0; i < brushPoints.Length - 1; i++)
            {
                AddConnection(brushPoints[i], brushPoints[i + 1]);
            }

            AddConnection(brushPoints[brushPoints.Length - 1], brushPoints[0]);

            return result.ToArray();

            void AddConnection(Vector3 a, Vector3 b)
            {
                while(!a.Equals(b))
                {
                    int x = 0;
                    int y = 0;
                    int z = 0;

                    if (a.x < b.x)
                        x = 1;
                    else if (a.x > b.x)
                        x = -1;

                    if (a.y < b.y)
                        y = 1;
                    else if (a.y > b.y)
                        y = -1;

                    if (a.z < b.z)
                        z = 1;
                    else if (a.z > b.z)
                        z = -1;

                    if (x != 0 || y != 0 || z != 0)
                        result.Add(new Vector3(x, y, z) + offset);
                }
            }
        }
    }
}
