﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.IO
{
    public unsafe class MapTiles
    {
        public int Width { get; private set; }
        public int Depth { get; private set; }

        public int BlockWidth { get; private set; }
        public int BlockDepth { get; private set; }

        public TileBlock[] TileBlocks => _tileBlocks;

        static readonly int _headerStart = 4096;
        static readonly int _mapIndex = 6;

        TileBlock[] _tileBlocks;

        public void Load(string file, bool isUOP, int width, int depth)
        {
            SetSize(width, depth);

            if (isUOP)
            {
                FileInfo uopFile = new FileInfo(file);
                FileInfo tempMulFile = new FileInfo(Path.Combine(uopFile.Directory.FullName, $"{uopFile.Name}.mul"));

                if (File.Exists(tempMulFile.FullName))
                    File.Delete(tempMulFile.FullName);

                UoFiddler.Plugin.UopPacker.Classes.LegacyMulFileConverter converter = new UoFiddler.Plugin.UopPacker.Classes.LegacyMulFileConverter();
                converter.FromUOP(uopFile.FullName, tempMulFile.FullName, _mapIndex);

                LoadTiles(tempMulFile.FullName);

                if (File.Exists(tempMulFile.FullName))
                    File.Delete(tempMulFile.FullName);
            }
            else
            {
                LoadTiles(file);
            }
        }

        public void Save(string file, bool asUOP)
        {
            if (asUOP)
            {
                FileInfo uopFile = new FileInfo(file);
                FileInfo tempMulFile = new FileInfo(Path.Combine(uopFile.Directory.FullName, $"{uopFile.Name}.mul"));

                if (File.Exists(tempMulFile.FullName))
                    File.Delete(tempMulFile.FullName);

                Save(tempMulFile.FullName);


                FileBrowser.BackupFile(uopFile.FullName, 5);
                UoFiddler.Plugin.UopPacker.Classes.LegacyMulFileConverter.ToUOP(tempMulFile.FullName, uopFile.FullName, _mapIndex);

                if (File.Exists(tempMulFile.FullName))
                    File.Delete(tempMulFile.FullName);
            }
            else
            {
                Save(file);
            }
        }

        public void GenerateFlatland(int width, int depth)
        {
            SetSize(width, depth);

            TileBlock[] tileBlocks = new TileBlock[BlockWidth * BlockDepth];

            Parallel.For(0, tileBlocks.Length, tb =>
            {
                Tile[] tiles = new Tile[64];

                for (int i = 0; i < 64; i++)
                    tiles[i] = new Tile(3, 0);

                tileBlocks[tb] = new TileBlock(_headerStart + tb, tiles);
            });

            _tileBlocks = tileBlocks;
        }

        public void GenerateConverted(int[] tileHeights, int[] tileIds, int width, int depth)
        {
            SetSize(width, depth);

            TileBlock[] tileBlocks = new TileBlock[BlockWidth * BlockDepth];

            for (int xb = 0; xb < BlockWidth; xb++)
            {
                int wx = xb * 8;

                for (int zb = 0; zb < BlockDepth; zb++)
                {
                    int blockIndex = xb * BlockDepth + zb;
                    int wz = zb * 8;

                    Tile[] tiles = new Tile[64];
                    int tileIndex = 0;

                    for (int x = 0; x < 8; x++)
                    {
                        for (int z = 0; z < 8; z++)
                        {
                            int srcIndex = (wx + x) * Depth + (wz + z);
                            short tileId = (short)tileIds[srcIndex];
                            sbyte height = (sbyte)tileHeights[srcIndex];

                            tiles[tileIndex++] = new Tile(tileId, height);
                        }
                    }

                    tileBlocks[blockIndex] = new TileBlock(_headerStart + blockIndex, tiles);
                }
            }

            _tileBlocks = tileBlocks;
        }

        public ref Tile GetTile(int x, int z)
        {
            TileBlock block = GetBlock(z, x);
            return ref block.Tiles[((x & 0x7) << 3) + (z & 0x7)];
        }

        public bool IsInBounds(int x, int z)
        {
            return x >= 0 && z >= 0 &&
                   x < Depth && z < Width;
        }

        public bool IsEvenHeight(int x, int z)
        {
            ref Tile a = ref GetTile(x,     z);
            ref Tile b = ref GetTile(x,     z + 1);

            if (a.Z != b.Z)
                return false;

            ref Tile c = ref GetTile(x + 1, z + 1);

            if (b.Z != c.Z)
                return false;

            ref Tile d = ref GetTile(x + 1, z);

            if (c.Z != d.Z)
                return false;


            return true;
        }

        void Save(string file)
        {
            if (File.Exists(file))
                File.Delete(file);

            using (FileStream fstream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
            using (BinaryWriter writer = new BinaryWriter(fstream))
            {
                for (int i = 0; i < _tileBlocks.Length; i++)
                {
                    ref TileBlock block = ref _tileBlocks[i];
                    writer.Write(block.Header);

                    for (int x = 0; x < block.Tiles.Length; x++)
                    {
                        ref Tile tile = ref block.Tiles[x];
                        writer.Write(tile.TileId);
                        writer.Write(tile.Z);
                    }
                }

                fstream.Flush();
            }
        }

        void SetSize(int width, int depth)
        {
            Width = width;
            Depth = depth;
            BlockWidth = width / 8;
            BlockDepth = depth / 8;
        }

        void LoadTiles(string file)
        {
            TileBlock[] tileBlocks = new TileBlock[BlockWidth * BlockDepth];

            using (FileStream mapStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (MemoryMappedFile mapMMF = MemoryMappedFile.CreateFromFile(mapStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
            using (MemoryMappedViewAccessor mapAccessor = mapMMF.CreateViewAccessor())
            {
                byte* mapStart = null;
                mapAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref mapStart);

                try
                {
                    LoadTiles(mapStart, tileBlocks);
                }
                finally
                {
                    mapAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                }
            }

            _tileBlocks = tileBlocks;
        }

        void LoadTiles(byte* start, TileBlock[] blocks)
        {
            const int sizePerTile = 3;
            const int sizePerBlock = 64 * 3 + 4;

            long sizePerRow = sizePerBlock * BlockDepth;

            Parallel.For(0, BlockWidth, bx =>
            {
                byte* xblockPtr = start + bx * sizePerRow;
                int xblockIndex = bx * BlockDepth;

                for (int bz = 0; bz < BlockDepth; bz++)
                {
                    int header = Pointers.ReadValue<int>(ref xblockPtr, 4);

                    Tile[] tiles = new Tile[64];
                    for (int i = 0; i < 64; i++)
                        tiles[i] = Pointers.ReadValue<Tile>(ref xblockPtr, sizePerTile);

                    blocks[xblockIndex++] = new TileBlock(header, tiles);
                }
            });
        }

        TileBlock GetBlock(int x, int y)
        {
            return _tileBlocks[(x >> 3) * BlockDepth + (y >> 3)];
        }
    }


    public class TileBlock
    {
        public int Header { get; set; }
        public Tile[] Tiles { get; set; }

        public TileBlock(int header, Tile[] tiles)
        {
            Header = header;
            Tiles = tiles;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tile
    {
        public short TileId { get; set; }
        public sbyte Z { get; set; }

        public Tile(short tileId, sbyte z) : this()
        {
            TileId = tileId;
            Z = z;
        }
    }
}
