using System;
using UnityEngine;

namespace Assets.Source.Geometry
{
    public struct Point2 : IEquatable<Point2>
    {
        public int X;
        public int Y;

        public Point2(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector2(Point2 p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static implicit operator Point2(Vector2 v)
        {
            return new Point2((int)v.x, (int)v.y);
        }

        public static implicit operator Point2(Vector3 v)
        {
            return new Point2((int)v.x, (int)v.y);
        }

        public static bool operator ==(Point2 left, Vector2 right)
        {
            return left.Equals((Point2)right);
        }

        public static bool operator !=(Point2 left, Vector2 right)
        {
            return !(left == right);
        }

        public static bool operator ==(Point2 left, Point2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point2 left, Point2 right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is Point2 point && Equals(point);
        }

        public bool Equals(Point2 other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
