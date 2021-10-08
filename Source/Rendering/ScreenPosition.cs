//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Assets.Source.Rendering
//{
//    public static class ScreenPosition
//    {
//        public static Vector3[] GetScreenPoints()
//        {
//            return GetScreenPoints(out Vector3 _, out Vector3 _);
//        }

//        public static Vector3[] GetScreenPoints(out Vector3 min, out Vector3 max)
//        {
//            Vector3[] screenPoints = new Vector3[]
//            {
//                new Vector3(0, 0, 1),
//                new Vector3(0, Screen.height, 1),
//                new Vector3(Screen.width, Screen.height, 1),
//                new Vector3(Screen.width, 0, 1)
//            };

//            float minX = int.MaxValue;
//            float maxX = int.MinValue;

//            float minZ = int.MaxValue;
//            float maxZ = int.MinValue;

//            for (int i = 0; i < screenPoints.Length; i++)
//            {
//                Ray ray = Camera.main.ScreenPointToRay(screenPoints[i]);
//                Vector3 origin = ray.origin;
//                Vector3 direction = ray.direction;

//                while (!ForwardRaycast(ref origin, ref direction))
//                    continue;

//                screenPoints[i] = origin;

//                if (minX > origin.x)
//                    minX = origin.x;
//                if (maxX < origin.x)
//                    maxX = origin.x;

//                if (minZ > origin.z)
//                    minZ = origin.z;
//                if (maxZ < origin.z)
//                    maxZ = origin.z;
//            }

//            min = new Vector3(minX, minZ);
//            max = new Vector3(maxX, maxZ);
//            return screenPoints;
//        }

//        static bool ForwardRaycast(ref Vector3 pos, ref Vector3 dir)
//        {
//            if (pos.y <= 0)
//                return true;

//            Vector3 nextPos = pos + dir;

//            if (nextPos.y == 0)
//                return true;
//            else if (nextPos.y < 0)
//            {
//                float diffPerc = 100f - GetPercentage(dir.y, nextPos.y);

//                dir.x = GetValueFromPercentage(dir.x, diffPerc);
//                dir.y = GetValueFromPercentage(dir.y, diffPerc);
//                dir.z = GetValueFromPercentage(dir.z, diffPerc);

//                pos += dir;
//                return true;
//            }

//            pos = nextPos;
//            return false;
//        }

//        static float GetPercentage(float max, float current)
//        {
//            return 100f / max * current;
//        }

//        static float GetValueFromPercentage(float max, float percent)
//        {
//            return max / 100f * percent;
//        }
//    }
//}
