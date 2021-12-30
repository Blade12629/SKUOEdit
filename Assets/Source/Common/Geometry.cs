//
//  TAKEN FROM: https://github.com/ServUO/ServUO/blob/master/Server/Geometry.cs
//  Contains changes
//

using System;
using UnityEngine;

namespace Server.Misc
{
    public static class Geometry
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static double RadiansToDegrees(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static double DegreesToRadians(double angle)
        {
            return angle * (Math.PI / 180.0);
        }

        public static Vector2Int ArcPoint(Vector3Int loc, int radius, int angle)
        {
            int sideA, sideB;

            if (angle < 0)
                angle = 0;

            if (angle > 90)
                angle = 90;

            sideA = (int)Math.Round(radius * Math.Sin(DegreesToRadians(angle)));
            sideB = (int)Math.Round(radius * Math.Cos(DegreesToRadians(angle)));

            return new Vector2Int(loc.x - sideB, loc.y - sideA);
        }

        public static void Circle2D(Vector3Int loc, int radius, Action<Vector3Int> effect)
        {
            Circle2D(loc, radius, effect, 0, 360);
        }

        public static void Circle2D(Vector3Int loc, int radius, Action<Vector3Int> effect, int angleStart, int angleEnd)
        {
            if (angleStart < 0 || angleStart > 360)
                angleStart = 0;

            if (angleEnd > 360 || angleEnd < 0)
                angleEnd = 360;

            if (angleStart == angleEnd)
                return;

            bool opposite = angleStart > angleEnd;

            int startQuadrant = angleStart / 90;
            int endQuadrant = angleEnd / 90;

            Vector2Int start = ArcPoint(loc, radius, angleStart % 90);
            Vector2Int end = ArcPoint(loc, radius, angleEnd % 90);

            if (opposite)
            {
                Swap(ref start, ref end);
                Swap(ref startQuadrant, ref endQuadrant);
            }

            CirclePoint startPoint = new CirclePoint(start, angleStart, startQuadrant);
            CirclePoint endPoint = new CirclePoint(end, angleEnd, endQuadrant);

            int error = -radius;
            int x = radius;
            int y = 0;

            while (x > y)
            {
                plot4points(loc, x, y, startPoint, endPoint, effect, opposite);
                plot4points(loc, y, x, startPoint, endPoint, effect, opposite);

                error += (y * 2) + 1;
                ++y;

                if (error >= 0)
                {
                    --x;
                    error -= x * 2;
                }
            }

            plot4points(loc, x, y, startPoint, endPoint, effect, opposite);
        }

        public static void plot4points(Vector3Int loc, int x, int y, CirclePoint start, CirclePoint end, Action<Vector3Int> effect, bool opposite)
        {
            Vector2Int pointA = new Vector2Int(loc.x - x, loc.y - y);
            Vector2Int pointB = new Vector2Int(loc.x - y, loc.y - x);

            int quadrant = 2;

            if (x == 0 && start.Quadrant == 3)
                quadrant = 3;

            if (WithinCircleBounds(quadrant == 3 ? pointB : pointA, quadrant, loc, start, end, opposite))
                effect(new Vector3Int(loc.x + x, loc.y + y, loc.z));

            quadrant = 3;

            if (y == 0 && start.Quadrant == 0)
                quadrant = 0;

            if (x != 0 && WithinCircleBounds(quadrant == 0 ? pointA : pointB, quadrant, loc, start, end, opposite))
                effect(new Vector3Int(loc.x - x, loc.y + y, loc.z));
            if (y != 0 && WithinCircleBounds(pointB, 1, loc, start, end, opposite))
                effect(new Vector3Int(loc.x + x, loc.y - y, loc.z));
            if (x != 0 && y != 0 && WithinCircleBounds(pointA, 0, loc, start, end, opposite))
                effect(new Vector3Int(loc.x - x, loc.y - y, loc.z));
        }

        public static bool WithinCircleBounds(Vector2Int pointLoc, int pointQuadrant, Vector3Int center, CirclePoint start, CirclePoint end, bool opposite)
        {
            if (start.Angle == 0 && end.Angle == 360)
                return true;

            int startX = start.Point.x;
            int startY = start.Point.y;
            int endX = end.Point.x;
            int endY = end.Point.y;

            int x = pointLoc.x;
            int y = pointLoc.y;

            if (pointQuadrant < start.Quadrant || pointQuadrant > end.Quadrant)
                return opposite;

            if (pointQuadrant > start.Quadrant && pointQuadrant < end.Quadrant)
                return !opposite;

            bool withinBounds = true;

            if (start.Quadrant == end.Quadrant)
            {
                if (startX == endX && (x > startX || y > startY || y < endY))
                    withinBounds = false;
                else if (startY == endY && (y < startY || x < startX || x > endX))
                    withinBounds = false;
                else if (x < startX || x > endX || y > startY || y < endY)
                    withinBounds = false;
            }
            else if (pointQuadrant == start.Quadrant && (x < startX || y > startY))
                withinBounds = false;
            else if (pointQuadrant == end.Quadrant && (x > endX || y < endY))
                withinBounds = false;

            return opposite ? !withinBounds : withinBounds;
        }

        public static void Line2D(Vector3Int start, Vector3Int end, Action<Vector3Int> effect)
        {
            bool steep = Math.Abs(end.y - start.y) > Math.Abs(end.x - start.x);

            int x0 = start.x;
            int x1 = end.x;
            int y0 = start.y;
            int y1 = end.y;

            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = deltax / 2;
            int ystep = y0 < y1 ? 1 : -1;
            int y = y0;

            for (int x = x0; x <= x1; x++)
            {
                if (steep)
                    effect(new Vector3Int(y, x, start.z));
                else
                    effect(new Vector3Int(x, y, start.z));

                error -= deltay;

                if (error < 0)
                {
                    y += ystep;
                    error += deltax;
                }
            }
        }

        public class CirclePoint
        {
            private readonly Vector2Int point;
            private readonly int angle;
            private readonly int quadrant;
            public CirclePoint(Vector2Int point, int angle, int quadrant)
            {
                this.point = point;
                this.angle = angle;
                this.quadrant = quadrant;
            }

            public Vector2Int Point => point;

            public int Angle => angle;

            public int Quadrant => quadrant;
        }
    }
}