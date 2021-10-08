//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace Assets.Source.IO
//{
//    public unsafe class StaticTileMatrix : MemoryUOFile
//    {
//        public string IDXFilePath { get; set; }
//        public int Width { get; private set; }
//        public int Height { get; private set; }
//        public int BlockWidth { get; private set; }
//        public int BlockHeight { get; private set; }

//        public IReadOnlyList<StaticBlock> Statics => _staticBlocks;

//        StaticBlock[] _staticBlocks;

//        public StaticTileMatrix(string filePath, string idxFilePath, int width, int height) : base(filePath)
//        {
//            IDXFilePath = idxFilePath;
//            Width = width;
//            Height = height;

//            BlockWidth = width >> 3;
//            BlockHeight = height >> 3;
//        }

//        public StaticTileMatrix(int width, int height) : this(null, null, width, height)
//        {
//            int total = BlockWidth * BlockHeight;
//            StaticBlock[] blocks = new StaticBlock[total];

//            for (int i = 0; i < total; i++)
//            {
//                blocks[i] = new StaticBlock(-1, new Static[64]);
//            }

//            _staticBlocks = blocks;
//        }

//        protected override void OnLoad(MemoryManager mem)
//        {
//            List<StaticBlock> staticBlocks = new List<StaticBlock>();
//            IDX idx = new IDX(IDXFilePath);
//            idx.Load();

//            fixed (Index* indexb = idx.Indices)
//            {
//                int length = idx.Indices.Length;
//                for (int i = 0; i < length; i++)
//                {
//                    Index* index = indexb + i;

//                    if (index->Lookup == -1)
//                    {
//                        staticBlocks.Add(new StaticBlock(-1, Array.Empty<Static>()));
//                        continue;
//                    }

//                    mem.Position = index->Lookup;
//                    Static[] statics = new Static[index->Length / 7];

//                    for (int x = 0; x < statics.Length; x++)
//                    {
//                        statics[x] = new Static
//                        {
//                            TileId = *mem.ReadUShort(),
//                            X = *mem.ReadByte(),
//                            Y = *mem.ReadByte(),
//                            Z = *mem.ReadSbyte(),
//                            Hue = *mem.ReadShort(),
//                        };
//                    }

//                    staticBlocks.Add(new StaticBlock(index->Lookup, statics.OrderBy(s => s.X * BlockHeight + s.Y).ToArray()));
//                }
//            }

//            _staticBlocks = staticBlocks/*.OrderBy(b => b.Lookup)*/.ToArray();
//        }
//    }

//    public struct StaticBlock
//    {
//        /// <summary>
//        /// -1 if empty, otherwise last lookup or 0
//        /// </summary>
//        public int Lookup { get; set; }
//        public Static[] Statics { get; set; }

//        public StaticBlock(int lookup, Static[] statics) : this()
//        {
//            Lookup = lookup;
//            Statics = statics;
//        }
//    }

//    public struct Static : IEquatable<Static>
//    {
//        public ushort TileId { get; set; }
//        public byte X { get; set; }
//        public byte Y { get; set; }
//        public sbyte Z { get; set; }
//        public short Hue { get; set; }
//        public bool WriteZeroStaticId { get; set; }
//        public bool IsNullRef { get; set; }

//        public Static(ushort tileId, byte x, byte y, sbyte z, short hue, bool writeZeroStaticId = false)
//        {
//            TileId = tileId;
//            X = x;
//            Y = y;
//            Z = z;
//            Hue = hue;
//            WriteZeroStaticId = writeZeroStaticId;
//            IsNullRef = false;
//        }

//        public override bool Equals(object obj)
//        {
//            return obj is Static @static && Equals(@static);
//        }

//        public bool Equals(Static other)
//        {
//            return TileId == other.TileId &&
//                   X == other.X &&
//                   Y == other.Y &&
//                   Z == other.Z;
//        }

//        public override int GetHashCode()
//        {
//            int hashCode = -59855996;
//            hashCode = hashCode * -1521134295 + TileId.GetHashCode();
//            hashCode = hashCode * -1521134295 + X.GetHashCode();
//            hashCode = hashCode * -1521134295 + Y.GetHashCode();
//            hashCode = hashCode * -1521134295 + Z.GetHashCode();
//            return hashCode;
//        }

//        public static bool operator ==(Static left, Static right)
//        {
//            return left.Equals(right);
//        }

//        public static bool operator !=(Static left, Static right)
//        {
//            return !(left == right);
//        }
//    }
//}
