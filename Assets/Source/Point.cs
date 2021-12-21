using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source
{
    public struct Point
    {
        public static readonly Point Zero = new Point(0, 0);
        public static readonly Point Max = new Point(int.MaxValue, int.MaxValue);
        public static readonly Point Min = new Point(int.MinValue, int.MinValue);

        public int X { get; set; }
        public int Z { get; set; }

        public Point(int x, int z) : this()
        {
            X = x;
            Z = z;
        }

        public Point(float x, float z) : this((int)x, (int)z)
        {

        }

        /// <summary>
        /// Converts a vector to a point
        /// </summary>
        /// <param name="ceil">if true <see cref="Mathf.Ceil(float)"/> will be used</param>
        public static Point FromVector(Vector2 v, bool ceil)
        {
            int x = 0;
            int z = 0;

            if (ceil)
            {
                x = (int)Mathf.Ceil(v.x);
                z = (int)Mathf.Ceil(v.y);
            }
            else
            {
                x = (int)v.x;
                z = (int)v.y;
            }

            return new Point(x, z);
        }

        /// <summary>
        /// Converts a vector to a point
        /// <para>(<see cref="Vector3.x"/>, <see cref="Vector3.z"/> -> <see cref="Point.X"/>, <see cref="Point.Z"/>)</para>
        /// </summary>
        /// <param name="ceil">if true <see cref="Mathf.Ceil(float)"/> will be used</param>
        public static Point FromVector(Vector3 v, bool ceil)
        {
            int x = 0;
            int z = 0;

            if (ceil)
            {
                x = (int)Mathf.Ceil(v.x);
                z = (int)Mathf.Ceil(v.z);
            }
            else
            {
                x = (int)v.x;
                z = (int)v.z;
            }

            return new Point(x, z);
        }

        public override bool Equals(object obj)
        {
            return obj is Point point &&
                   X == point.X &&
                   Z == point.Z;
        }

        public override int GetHashCode()
        {
            int hashCode = 1911744652;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public static explicit operator Point(Vector2 v)
        {
            return FromVector(v, false);
        }

        public static explicit operator Point(Vector3 v)
        {
            return FromVector(v, false);
        }

        public static explicit operator Vector2(Point p)
        {
            return new Vector2(p.X, p.Z);
        }

        public static explicit operator Vector3(Point p)
        {
            return new Vector3(p.X, 0f, p.Z);
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Z + b.Z);
        }

        public static Point operator +(Point a, int v)
        {
            return new Point(a.X + v, a.Z + v);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Z - b.Z);
        }

        public static Point operator -(Point a, int v)
        {
            return new Point(a.X - v, a.Z - v);
        }

        public static Point operator *(Point a, Point b)
        {
            return new Point(a.X * b.X, a.Z * b.Z);
        }

        public static Point operator *(Point a, int v)
        {
            return new Point(a.X * v, a.Z * v);
        }

        public static Point operator /(Point a, Point b)
        {
            return new Point(a.X / b.X, a.Z / b.Z);
        }

        public static Point operator /(Point a, int v)
        {
            return new Point(a.X / v, a.Z / v);
        }

        public static Point operator %(Point a, Point b)
        {
            return new Point(a.X % b.X, a.Z % b.Z);
        }

        public static Point operator %(Point a, int v)
        {
            return new Point(a.X % v, a.Z % v);
        }

        public static Point operator ^(Point a, Point b)
        {
            return new Point(a.X ^ b.X, a.Z ^ b.Z);
        }

        public static Point operator ^(Point a, int v)
        {
            return new Point(a.X ^ v, a.Z ^ v);
        }
    }
}
