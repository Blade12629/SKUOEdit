using System;
using UnityEngine;

namespace Assets.Source.Geometry
{
    public struct Point3 : IEquatable<Point3>
    {
        public int X;
        public int Y;
        public int Z;

        public Point3(int x, int y, int z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3(int x, int y) : this(x, y, 0)
        {

        }

        public static implicit operator Vector3(Point3 p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }

        public static implicit operator Point3(Vector3 v)
        {
            return new Point3((int)v.x, (int)v.y, (int)v.z);
        }

        public static implicit operator Point3(Vector2 v)
        {
            return new Point3((int)v.x, (int)v.y);
        }

        public static bool operator ==(Point3 left, Vector3 right)
        {
            return left.Equals((Point3)right);
        }

        public static bool operator !=(Point3 left, Vector3 right)
        {
            return !(left == right);
        }

        public static bool operator ==(Point3 left, Point3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point3 left, Point3 right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is Point3 point && Equals(point);
        }

        public bool Equals(Point3 other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Z == other.Z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
    }
}
