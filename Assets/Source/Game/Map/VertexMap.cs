using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    /// <summary>
    /// Vertexmap used to hold and manage vertices
    /// </summary>
    public sealed unsafe class VertexMap
    {
        public int Width { get; }
        public int Depth { get; }

        Vertex[] _vertices;

        /// <summary>
        /// Creates a new map with the specified sizes
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        public VertexMap(int width, int depth)
        {
            Width = width;
            Depth = depth;

            _vertices = new Vertex[width * depth * 4];
        }

        /// <summary>
        /// Creates a new map with the predefined vertices
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        public VertexMap(Vertex[] vertices, int width, int depth)
        {
            Width = width;
            Depth = depth;

            _vertices = vertices;
        }

        /// <summary>
        /// Gets the reference to a specific vertex
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ref Vertex Get(int x, int z, int offset = 0)
        {
            return ref _vertices[PositionToIndex(x, z) + offset];
        }

        ///// <summary>
        ///// Gets the height of a specific position
        ///// </summary>
        ///// <param name="x"></param>
        ///// <param name="z"></param>
        ///// <returns></returns>
        //public float GetHeight(int x, int z)
        //{
        //    ref Vertex v = ref _vertices[PositionToIndex(x, z)];
        //    return v.Y;
        //}

        public void SetUVs(int x, int z, Vector2[] uvs)
        {
            int startIndex = PositionToIndex(x, z);
            for (int i = 0; i < 4; i++)
            {
                ref Vector2 uv = ref uvs[i];
                ref Vertex v = ref _vertices[startIndex + i];

                v.UvX = uv.x;
                v.UvY = uv.y;
            }
        }

        /// <summary>
        /// Copies vertex data into the specified array
        /// </summary>
        /// <param name="dest">Array the data should be copied to</param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        public void Copy(Vertex[] dest, int x, int z, int width, int depth)
        {
            Parallel.For(0, width, xCur =>
            {
                int indexSrc = PositionToIndex(x + xCur, z);
                int indexDest = PositionToIndex(xCur, 0, depth);
                int length = depth * 4;

                // our index cannot be less than 0, reduce length to make it fit
                if (indexSrc < 0)
                {
                    length += indexSrc;
                    indexSrc = 0;
                }

                // our length would be too big, simply get the new length
                if (indexDest + length >= dest.Length)
                {
                    length = dest.Length - indexDest;

                    if (indexSrc + length >= _vertices.Length)
                    {
                        length = Math.Min(length, _vertices.Length - indexSrc);
                    }
                }
                else if (indexSrc + length >= _vertices.Length)
                {
                    length = _vertices.Length - indexSrc;
                }

                // seems like we have no valid data to copy, just skip
                if (length <= 0)
                    return;

                // copy area vertice data
                Array.Copy(_vertices, indexSrc, dest, indexDest, length);
            });
        }
        
        /// <summary>
        /// Copies vertex data into the specified array
        /// </summary>
        /// <param name="dest">Array the data should be copied to</param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        public void Copy(NativeArray<Vertex> dest, int x, int z, int width, int depth)
        {
            Vertex[] temp = new Vertex[dest.Length];
            Parallel.For(0, width, xCur =>
            {
                int indexSrc = PositionToIndex(x + xCur, z);
                int indexDest = PositionToIndex(xCur, 0, depth);
                int length = depth * 4;

                // our index cannot be less than 0, reduce length to make it fit
                if (indexSrc < 0)
                {
                    length += indexSrc;
                    indexSrc = 0;
                }

                // our length would be too big, simply get the new length
                if (indexDest + length >= temp.Length)
                {
                    length = temp.Length - indexDest;

                    if (indexSrc + length >= _vertices.Length)
                    {
                        length = Math.Min(length, _vertices.Length - indexSrc);
                    }
                }
                else if (indexSrc + length >= _vertices.Length)
                {
                    length = _vertices.Length - indexSrc;
                }

                // seems like we have no valid data to copy, just skip
                if (length <= 0)
                    return;

                // copy area vertice data
                Array.Copy(_vertices, indexSrc, temp, indexDest, length);
                NativeArray<Vertex>.Copy(temp, dest);
            });
        }

        /// <summary>
        /// Sets the height of a specfic point (sets all vertices at this point)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="height"></param>
        public void SetHeight(int x, int z, float height)
        {
            fixed (Vertex* vb = _vertices)
            {
                Vertex* v = vb + PositionToIndex(x, z);
                v->Y = height;

                if (x > 0)
                {
                    v = vb + PositionToIndex(x - 1, z) + 3;
                    v->Y = height;

                    if (z > 0)
                    {
                        v = vb + PositionToIndex(x - 1, z - 1) + 2;
                        v->Y = height;
                    }
                }

                if (z > 0)
                {
                    v = vb + PositionToIndex(x, z - 1) + 1;
                    v->Y = height;
                }
            }
        }

        /// <summary>
        /// Gets all indices for a specific width and depth
        /// </summary>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static int[] GetIndices(int width, int depth)
        {
            int[] indices = new int[width * depth * 6];

            int iv = 0;
            int i = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    indices[i++] = iv + 0;
                    indices[i++] = iv + 1;
                    indices[i++] = iv + 2;

                    indices[i++] = iv + 0;
                    indices[i++] = iv + 2;
                    indices[i++] = iv + 3;

                    iv += 4;
                }
            }

            return indices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int PositionToIndex(int x, int z)
        {
            return (x * Depth + z) * 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PositionToIndex(int x, int z, int depth)
        {
            return (x * depth + z) * 4;
        }
    }
}
