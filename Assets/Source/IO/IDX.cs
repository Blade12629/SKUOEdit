using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.IO
{
    public unsafe class IDX : MemoryUOFile
    {
        public Index[] Indices { get; private set; }

        public IDX(string filePath) : base(filePath)
        {

        }

        protected override void OnLoad(MemoryManager mem)
        {
            int totalIndices = (int)(mem.Length / 12);
            Index[] indices = new Index[totalIndices];

            mem.ReadArray(indices, 0, totalIndices);

            Indices = indices;
        }
    }

    [StructLayout(LayoutKind.Sequential, Size = 12)]
    public struct Index
    {
        public static Index InvalidIndex => new Index(-1, -1, -1);

        public int Lookup { get; set; }
        public int Length { get; set; }
        public int Extra { get; set; }

        public Index(int lookup, int length, int extra) : this()
        {
            Lookup = lookup;
            Length = length;
            Extra = extra;
        }
    }
}
