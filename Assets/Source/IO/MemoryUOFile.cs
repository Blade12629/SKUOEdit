using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.IO
{
    public abstract unsafe class MemoryUOFile
    {
        public string FilePath { get; set; }

        public MemoryUOFile(string filePath)
        {
            FilePath = filePath;
        }

        public void Load()
        {
            FileInfo fileToLoad = new FileInfo(FilePath);

            if (!fileToLoad.Exists)
                throw new FileNotFoundException($"Could not find file {FilePath}");

            using (FileStream fstream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(fstream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true))
            using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
            {
                byte* start = null;
                accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref start);

                if (start == null)
                    throw new OperationCanceledException("Unable to acquire pointer for MemoryMappedFile");

                MemoryManager mem = new MemoryManager(start, fstream.Length);
                OnLoad(mem);
            }
        }

        protected abstract void OnLoad(MemoryManager mem);
    }
}
