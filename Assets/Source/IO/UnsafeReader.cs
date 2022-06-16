using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Assets.Source.IO
{
    public unsafe class UnsafeReader : UnsafeReaderWriterBase
    {
        public UnsafeReader(string path, long size) : base(path, size, false)
        {

        }

        public T[] Read<T>(int length) where T : unmanaged
        {
            int size = sizeof(T) * length;
            EnsureSize(size);

            T* pcur = (T*)_pindex;
            T[] result = new T[length];

            fixed (T* presult = result)
                for (int i = 0; i < length; i++)
                    *(presult + i) = *(pcur++);

            Index += size;
            _pindex += size;

            return result;
        }

        public T Read<T>() where T : unmanaged
        {
            return *ReadPointer<T>();
        }

        public T* ReadPointer<T>() where T : unmanaged
        {
            EnsureSize(sizeof(T));

            T* pcur = (T*)_pindex;

            Index += sizeof(T);
            _pindex += sizeof(T);

            return pcur;
        }
    }
}
