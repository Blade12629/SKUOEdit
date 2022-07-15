using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.IO
{
    public class MapStatics : MapBase
    {
        public StaticBlock[] StaticBlocks => _staticBlocks;

        StaticBlock[] _staticBlocks;

        public void Load(string file, string idxFile, int width, int depth)
        {
            SetSize(width, depth);

            unsafe
            {
                List<IDX> idx = new List<IDX>();

                using (FileStream stIdxStream = new FileStream(idxFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (MemoryMappedFile stIdxMMF = MemoryMappedFile.CreateFromFile(stIdxStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
                using (MemoryMappedViewAccessor stIdxAccessor = stIdxMMF.CreateViewAccessor())
                {
                    byte* stIdxStart = null;
                    stIdxAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref stIdxStart);

                    try
                    {
                        byte* end = stIdxStart + stIdxStream.Length;

                        for (; stIdxStart < end; stIdxStart += 12)
                        {
                            idx.Add(*(IDX*)stIdxStart);
                        }
                    }
                    finally
                    {
                        stIdxAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                    }
                }

                using (FileStream stStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                using (MemoryMappedFile stMMF = MemoryMappedFile.CreateFromFile(stStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false))
                using (MemoryMappedViewAccessor stAccessor = stMMF.CreateViewAccessor())
                {
                    byte* stStart = null;
                    stAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref stStart);

                    StaticBlock[] staticBlocks = new StaticBlock[idx.Count];

                    try
                    {
                        for (int i = 0; i < idx.Count; i++)
                        {
                            IDX index = idx[i];

                            if (index.Lookup == -1 || index.Length < 7)
                                continue;

                            byte* cur = stStart + index.Lookup;
                            Static[] statics = new Static[index.Length / 7];

                            for (int x = 0; x < statics.Length; x++, cur += 7)
                                statics[x] = *(Static*)cur;

                            staticBlocks[i] = new StaticBlock(index.Lookup, statics);
                        }
                    }
                    finally
                    {
                        stAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                    }

                    _staticBlocks = staticBlocks;
                }
            }
        }

        public void Save(string file, string idxFile)
        {
            if (File.Exists(idxFile))
                File.Delete(idxFile);

            if (File.Exists(file))
                File.Delete(file);

            using (FileStream stIdxStream = new FileStream(idxFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
            using (FileStream stStream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
            using (BinaryWriter idxWriter = new BinaryWriter(stIdxStream))
            using (BinaryWriter stWriter = new BinaryWriter(stStream))
            {
                for (int i = 0; i < _staticBlocks.Length; i++)
                {
                    StaticBlock block = _staticBlocks[i];

                    if (block == null)
                    {
                        idxWriter.Write(-1);
                        idxWriter.Write(0);
                        idxWriter.Write(0);
                        continue;
                    }

                    idxWriter.Write(stStream.Position);
                    idxWriter.Write(block.Statics.Length * 7);
                    idxWriter.Write(0);

                    for (int x = 0; x < block.Statics.Length; x++)
                    {
                        ref Static st = ref block.Statics[x];

                        stWriter.Write(st.TileId);
                        stWriter.Write(st.X);
                        stWriter.Write(st.Y);
                        stWriter.Write(st.Z);
                        stWriter.Write(st.Hue);
                    }
                }

                idxWriter.Flush();
                stWriter.Flush();
            }
        }

        public StaticBlock GetStaticBlock(int x, int z)
        {
            return _staticBlocks[(z >> 3) * BlockDepth + (x >> 3)];
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IDX
        {
            public int Lookup { get; set; }
            public int Length { get; set; }
            public int Extra { get; set; }

            public IDX(int lookup, int length, int extra) : this()
            {
                Lookup = lookup;
                Length = length;
                Extra = extra;
            }
        }
    }

    public class StaticBlock
    {
        public Static[] Statics { get; set; }
        public long Lookup { get; set; }

        public StaticBlock(long lookup, Static[] statics)
        {
            Lookup = lookup;
            Statics = statics;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Static
    {
        public ushort TileId { get; set; }
        public byte X { get; set; } // Ranges from 0 to 7
        public byte Y { get; set; } // Ranges from 0 to 7
        public sbyte Z { get; set; }
        public short Hue { get; set; } // At one point this was the hue, but doesn't appear to be used anymore

        public Static(ushort tileId, byte x, byte y, sbyte z, short hue) : this()
        {
            TileId = tileId;
            X = x;
            Y = y;
            Z = z;
            Hue = hue;
        }
    }
}
