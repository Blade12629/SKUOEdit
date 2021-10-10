using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Game.Colorstores
{
    public sealed class ColorStore
    {
        public IReadOnlyDictionary<Color, int> Storage => _storage;

        Dictionary<Color, int> _storage;

        public ColorStore(int startCapacity)
        {
            _storage = new Dictionary<Color, int>(startCapacity);
        }

        public int this[Color color]
        {
            get
            {
                _ = TryGet(color, out int value);
                return value;
            }
            set
            {
                Update(color, value);
            }
        }

        public static ColorStore GetReverseHeightCS()
        {
            int r = 0;
            int g = 0;
            int b = 0;

            ColorStore result = new ColorStore(255 + 254);
            for (int z = -200; z < 255; z++)
            {
                r += 5;

                if (r >= 255)
                {
                    r = 0;
                    g += 5;

                    if (g >= 255)
                    {
                        g = 0;
                        b += 5;
                    }
                }

                result.Add(Color.FromArgb(r, g, b), z);
            }

            return result;
        }

        public static ColorStore GetReverseTileCS()
        {
            int r = 0;
            int g = 0;
            int b = 0;

            ColorStore result = new ColorStore(255 + 254);
            for (int id = 0; id < 512; id++)
            {
                r += 5;

                if (r >= 255)
                {
                    r = 0;
                    g += 5;

                    if (g >= 255)
                    {
                        g = 0;
                        b += 5;
                    }
                }

                result.Add(Color.FromArgb(r, g, b), id);
            }

            return result;
        }

#if DEBUG
        public Color GetByIndex(int index)
        {
            return _storage.Keys.ElementAt(index);
        }
#endif

        public bool Add(Color color, int value)
        {
            if (_storage.ContainsKey(color))
                return false;

            _storage.Add(color, value);
            return true;
        }

        public void AddRange(params (Color, int)[] values)
        {
            if (values == null)
                return;

            for (int i = 0; i < values.Length; i++)
                Add(values[i].Item1, values[i].Item2);
        }

        public bool TryGet(Color color, out int value)
        {
            return _storage.TryGetValue(color, out value);
        }

        public void Update(Color color, int value)
        {
            _storage[color] = value;
        }

        public void Clear()
        {
            _storage.Clear();
        }

        public void Save(string path)
        {
            using System.IO.FileStream fstream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
            using System.IO.StreamWriter w = new System.IO.StreamWriter(fstream, Encoding.UTF8);

            w.WriteLine("RGBA = Value");

            for (int i = 0; i < _storage.Count; i++)
            {
                var storagePair = _storage.ElementAt(i);
                w.WriteLine($"0x{storagePair.Key.ToArgb().ToString("X")} = {storagePair.Value}");
            }

            w.Flush();
        }

        public void Load(string path)
        {
            using System.IO.FileStream fstream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
            using System.IO.StreamReader reader = new System.IO.StreamReader(fstream, Encoding.UTF8);

            if (!reader.EndOfStream) // first line is just an explenation for the formatting
                _ = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Replace(" ", "");

                if (string.IsNullOrEmpty(line))
                    continue;
                else if (line.StartsWith("0x"))
                    line = line.Remove(0, 2);

                int index = line.IndexOf('=');

                if (index == -1)
                    continue;

                int colorValue = int.Parse(line.Substring(0, index), System.Globalization.NumberStyles.HexNumber);
                int value = int.Parse(line.Remove(0, index + 1));

                this[Color.FromArgb(colorValue)] = value;
            }
        }
    }
}
