using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Terrain
{
    /// <summary>
    /// Heightmap to manage tile heights
    /// </summary>
    public class HeightTable
    {
        public int Width => _width;
        public int Depth => _depth;

        int _width;
        int _depth;
        int[] _heights;

        public HeightTable(int width, int depth)
        {
            _width = width;
            _depth = depth;
            _heights = new int[width * depth];

            for (int i = 0; i < _heights.Length; i++)
                _heights[i] = int.MinValue;
        }

        /// <summary>
        /// Checks if all points of this specific tile are at the same height
        /// </summary>
        public bool IsEvenHeight(float x, float z)
        {
            return IsEvenHeight((int)x, (int)z);
        }

        /// <summary>
        /// Checks if all points of this specific tile are at the same height
        /// </summary>
        public bool IsEvenHeight(int x, int z)
        {
            //if (x % 2 == 0)
            //    x--;
            //if (z % 2 == 0)
            //    z--;

            int h0 = GetHeight(x, z);
            int h1 = GetHeight(x, z + 1);

            if (h1 != h0)
                return false;

            int h2 = GetHeight(x + 1, z + 1);

            if (h2 != h1)
                return false;

            int h3 = GetHeight(x + 1, z);

            if (h3 != h2)
                return false;

            return true;
        }

        /// <summary>
        /// Gets the height of a specific point
        /// </summary>
        /// <returns>Returns the height or 0 if the point doesn't exist</returns>
        public int GetHeight(float x, float z)
        {
            return GetHeight((int)x, (int)z);
        }

        /// <summary>
        /// Gets the height of a specific point
        /// </summary>
        /// <returns>Returns the height or 0 if the point doesn't exist</returns>
        public int GetHeight(int x, int z)
        {
            TryGetHeight(x, z, out int h);
            return h;
        }

        /// <summary>
        /// Gets the height of a specific point
        /// </summary>
        /// <returns>False - Point doesn't exist</returns>
        public bool TryGetHeight(float x, float z, out int h)
        {
            return TryGetHeight((int)x, (int)z, out h);
        }

        /// <summary>
        /// Gets the height of a specific point
        /// </summary>
        /// <returns>False - Point doesn't exist</returns>
        public bool TryGetHeight(int x, int z, out int h)
        {
            int index = ToIndex(x, z);

            if (index < 0 || index >= _heights.Length)
            {
                h = 0;
                return false;
            }

            int hx = _heights[index];

            if (hx == int.MinValue)
            {
                h = 0;
                return false;
            }

            h = _heights[index];
            return true;
        }

        /// <summary>
        /// Increases the height of a specific point
        /// </summary>
        /// <returns>False - Point doesn't exist</returns>
        public bool IncreaseHeight(float x, float z, int h)
        {
            return IncreaseHeight((int)x, (int)z, h);
        }

        /// <summary>
        /// Increases the height of a specific point
        /// </summary>
        /// <returns>False - Point doesn't exist</returns>
        public bool IncreaseHeight(int x, int z, int h)
        {
            int index = ToIndex(x, z);

            if (index < 0 || index >= _heights.Length)
                return false;

            ref int height = ref _heights[index];

            if (height == int.MinValue)
                height = 0;

            height += h;
            return true;
        }

        /// <summary>
        /// Sets the height of a specific point
        /// </summary>
        /// <returns>False - Point doesn't exist</returns>
        public bool SetHeight(int x, int z, int h)
        {
            int index = ToIndex(x, z);

            if (index < 0 || index >= _heights.Length)
                return false;

            _heights[index] = h;
            return true;
        }

        /// <summary>
        /// Resizes the height map
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        public void Resize(int width, int depth)
        {
            Array.Resize(ref _heights, width * depth);
            _width = width;
            _depth = depth;
        }

        int ToIndex(int x, int z)
        {
            return x * _depth + z;
        }
    }
}
