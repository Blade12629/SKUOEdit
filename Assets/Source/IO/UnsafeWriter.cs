using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Assets.Source.IO
{
    public unsafe class UnsafeWriter : UnsafeReaderWriterBase
    {
        public UnsafeWriter(string path, long size) : base(path, size, true)
        {
        }

        ~UnsafeWriter()
        {
            Dispose(false);
        }

        public void Write<T>(T* data, int length) where T : unmanaged
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(UnsafeWriter));

            EnsureSize(sizeof(T) * length);

            T* pcur = (T*)_pindex;

            for (int i = 0; i < length; i++)
                *(pcur++) = *(data++);

            _pindex += sizeof(T) * length;
        }

        public void Write<T>(T data) where T : unmanaged
        {
            Write(&data);
        }

        public void Write<T>(T* data) where T : unmanaged
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(UnsafeWriter));

            EnsureSize(sizeof(T));

            T* pcur = (T*)_pindex;
            *pcur = *data;

            Index += sizeof(T);
            _pindex += sizeof(T);
        }
    }
}
