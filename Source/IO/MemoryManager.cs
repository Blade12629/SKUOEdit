//// You can enable out of bounds checks for memory access by uncommenting the below line
//// Note: enabling out of bounds check can result in reduced performance

////#define OUT_OF_BOUNDS_CHECK

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Assets.Source.IO
//{
//    public unsafe class MemoryManager
//    {
//        public long Position { get; set; }
//        public long Length { get; }

//        byte* _start;

//        public MemoryManager(byte* start, long length)
//        {
//            _start = start;
//            Length = length;
//        }

//        public byte* AcquireCurrentPointer()
//        {
//            return _start + Position;
//        }

//        public byte* ReadByte()
//        {
//            return GetNextPtr(1);
//        }

//        public sbyte* ReadSbyte()
//        {
//            return (sbyte*)GetNextPtr(1);
//        }

//        public short* ReadShort()
//        {
//            return (short*)GetNextPtr(2);
//        }

//        public ushort* ReadUShort()
//        {
//            return (ushort*)GetNextPtr(2);
//        }

//        public int* ReadInt()
//        {
//            return (int*)GetNextPtr(4);
//        }

//        public uint* ReadUInt()
//        {
//            return (uint*)GetNextPtr(4);
//        }

//        public long* ReadLong()
//        {
//            return (long*)GetNextPtr(8);
//        }

//        public ulong* ReadULong()
//        {
//            return (ulong*)GetNextPtr(8);
//        }

//        public bool* ReadBool()
//        {
//            return (bool*)GetNextPtr(1);
//        }

//        public float* ReadFloat()
//        {
//            return (float*)GetNextPtr(4);
//        }

//        public double* ReadDouble()
//        {
//            return (double*)GetNextPtr(8);
//        }

//        public T* Read<T>() where T : unmanaged
//        {
//            int sizeOf = Marshal.SizeOf<T>();
//            return (T*)GetNextPtr(sizeOf);
//        }

//        public void ReadArray<T>(T[] data, int index, int length) where T : unmanaged
//        {
//            fixed(T* dataPtr = data)
//            {
//                for (int i = index; i < index + length; i++)
//                {
//                    *(dataPtr + i) = *Read<T>();
//                }
//            }
//        }

//        byte* GetNextPtr(int ptrLength)
//        {
//            byte* ptr = _start + Position;

//#if OUT_OF_BOUNDS_CHECK
//            long nextPos = Position + ptrLength;

//            if (nextPos >= Length)
//                throw new ArgumentOutOfRangeException("Out of bounds");

//            Position = nextPos;
//#else
//            Position += ptrLength;
//#endif

//            return ptr;
//        }
//    }
//}
