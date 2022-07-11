using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using Assets.Source.IO;
using Assets.Source.Ultima;
using Assets.Source.Textures;

namespace Assets.Source.Game.Map.Terrain
{
    public class UOTerrainCache
    {
        public int Width => _tiles.Width;
        public int Depth => _tiles.Depth;

        Vertex[] _cache;
        MapTiles _tiles;

        public UOTerrainCache(MapTiles tiles)
        {
            _cache = new Vertex[tiles.Width * tiles.Depth * 4];
            _tiles = tiles;

            BuildMapVertices();
        }

        public static int PositionToVertexIndex(int x, int z, int depth)
        {
            return (x * depth + z) * 4;
        }

        public ref Vertex GetRef(int x, int z)
        {
            return ref _cache[PositionToVertexIndex(x, z, _tiles.Depth)];
        }

        public void CopyTo(Vertex[] dest, int destX, int destZ, int areaSize)
        {
            Parallel.For(0, areaSize, xCur =>
            {
                int indexSrc = PositionToVertexIndex(destX + xCur, destZ, _tiles.Depth);
                int indexDest = PositionToVertexIndex(xCur, 0, areaSize);
                int length = areaSize * 4;

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

                    if (indexSrc + length >= _cache.Length)
                    {
                        length = Math.Min(length, _cache.Length - indexSrc);
                    }
                }
                else if (indexSrc + length >= _cache.Length)
                {
                    length = _cache.Length - indexSrc;
                }

                // seems like we have no valid data to copy, just skip
                if (length <= 0)
                    return;

                // copy area vertice data
                Array.Copy(_cache, indexSrc, dest, indexDest, length);
            });
        }

        void BuildMapVertices()
        {
            _cache = new Vertex[_tiles.Width * _tiles.Depth * 4];

            for (int x = 0; x < _tiles.Width; x++)
            {
                for (int z = 0; z < _tiles.Depth; z++)
                {
                    TileHeights heights = GetHeights(x, z);
                    Vector2[] uvs;

                    if (heights.IsEvenHeight())
                        uvs = TileAtlas.Instance.GetTileUVs(heights.BottomLeftTileId);
                    else
                        uvs = TileAtlas.Instance.GetTileTextureUVs(heights.BottomLeftTileId);

                    ApplyUVs(uvs, heights, x, z);
                }
            }
        }

        void ApplyUVs(Vector2[] uvs, TileHeights heights, int x, int z)
        {
            int index = PositionToVertexIndex(x, z, _tiles.Depth);

            for (int i = 0; i < 4; i++)
            {
                ref Vertex vertex = ref _cache[index + i];
                ref Vector2 uv = ref uvs[i];

                switch (i)
                {
                    default:
                    case 0:
                        vertex.X = x;
                        vertex.Z = z;
                        vertex.Y = UOToWorldHeight(heights.BottomLeft);
                        break;

                    case 1:
                        vertex.X = x;
                        vertex.Z = z + 1f;
                        vertex.Y = UOToWorldHeight(heights.TopLeft);
                        break;

                    case 2:
                        vertex.X = x + 1f;
                        vertex.Z = z + 1f;
                        vertex.Y = UOToWorldHeight(heights.TopRight);
                        break;

                    case 3:
                        vertex.X = x + 1f;
                        vertex.Z = z;
                        vertex.Y = UOToWorldHeight(heights.BottomRight);
                        break;
                }

                vertex.UvX = uv.x;
                vertex.UvY = uv.y;
            }
        }

        TileHeights GetHeights(int x, int z)
        {
            ref Tile tileBL = ref _tiles.GetTile(x, z);
            sbyte hTL = 0;
            sbyte hTR = 0;
            sbyte hBR = 0;

            if (z < _tiles.Depth)
            {
                ref Tile tileTL = ref _tiles.GetTile(x, z + 1);
                hTL = tileTL.Z;

                if (x < _tiles.Width - 1)
                {
                    ref Tile tileTR = ref _tiles.GetTile(x + 1, z + 1);
                    hTR = tileTR.Z;
                }
                else
                    hTR = tileBL.Z;
            }
            else
                hTL = tileBL.Z;

            if (x < _tiles.Width - 1)
            {
                ref Tile tileBR = ref _tiles.GetTile(x + 1, z);
                hBR = tileBR.Z;
            }
            else
                hBR = tileBL.Z;

            return new TileHeights((uint)tileBL.TileId, tileBL.Z, hTL, hTR, hBR);
        }

        float UOToWorldHeight(int uoZ)
        {
            return uoZ * Constants.TileHeightMod;
        }

        struct TileHeights
        {
            public uint BottomLeftTileId;
            public int BottomLeft;
            public int TopLeft;
            public int TopRight;
            public int BottomRight;

            public TileHeights(uint bottomLeftTileId, sbyte bottomLeft, sbyte topLeft, sbyte topRight, sbyte bottomRight) : this()
            {
                BottomLeftTileId = bottomLeftTileId;
                BottomLeft = bottomLeft;
                TopLeft = topLeft;
                TopRight = topRight;
                BottomRight = bottomRight;
            }

            public bool IsEvenHeight()
            {
                return BottomLeft == TopLeft &&
                       BottomLeft == TopRight &&
                       BottomLeft == BottomRight;
            }
        }
    }
}
