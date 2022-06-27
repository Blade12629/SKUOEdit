using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Assets.Source.IO
{
    public unsafe abstract class UnsafeReaderWriterBase : IDisposable
    {
        public long Length { get; }
        public long Index
        {
            get => _index;
            set => Math.Min(value, Length - 1);
        }

        FileStream _stream;
        MemoryMappedFile _mmf;
        MemoryMappedViewAccessor _mmva;

        protected byte* _fileStart;
        protected long _index;
        protected byte* _pindex;
        protected bool _isDisposed;

        public UnsafeReaderWriterBase(string path, long size, bool deleteIfExists)
        {
            if (deleteIfExists && File.Exists(path))
                File.Delete(path);

            _stream = File.Create(path);
            _mmf = MemoryMappedFile.CreateFromFile(_stream, null, size, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true);
            _mmva = _mmf.CreateViewAccessor();
            _mmva.SafeMemoryMappedViewHandle.AcquirePointer(ref _fileStart);
            _pindex = _fileStart;
        }

        ~UnsafeReaderWriterBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void EnsureSize(int size)
        {
            if (Index + size > Length)
                throw new IndexOutOfRangeException();
        }

        protected byte* GetCurrentPointer()
        {
            return _fileStart + Index;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _mmva.SafeMemoryMappedViewHandle.ReleasePointer();
                _mmva.Dispose();
                _mmf.Dispose();
                _stream.Flush();
                _stream.Dispose();

                _isDisposed = true;
            }
        }
    }
}
