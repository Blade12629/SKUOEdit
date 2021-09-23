using Assets.Source.IO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Textures
{
    public static class GameTextures
    {
        public static Texture2D GameTexture { get; private set; }
        public static Dictionary<int, Vector2[]> UVs { get; private set; }

        static int _x;
        static int _y;
        static int _width;
        static int _height;
        static float _baseX;
        static float _baseY;
        static int _currentMaxWidth;

        public static void Initialize(bool drawTransparent)
        {
            GameTexture = new Texture2D(6000, 6000);
            UVs = new Dictionary<int, Vector2[]>();

            _width = GameTexture.width;
            _height = GameTexture.height;

            _baseX = 1f / _width;
            _baseY = 1f / _height;

            if (drawTransparent)
            {
                var pixels = GameTexture.GetPixels();

                for (int i = 0; i < pixels.Length; i++)
                {
                    ref var px = ref pixels[i];
                    px.a = 0;
                    px.r = 0;
                    px.g = 0;
                    px.b = 0;
                }

                GameTexture.SetPixels(pixels);
            }
        }

        public static void Export(string folder)
        {
            using (FileStream uvstream = new FileStream(Path.Combine(folder, "tileatlas.uvs"), FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter w = new BinaryWriter(uvstream))
                {
                    w.Write(_x);
                    w.Write(_y);
                    w.Write(_width);
                    w.Write(_height);
                    w.Write(_baseX);
                    w.Write(_baseY);
                    w.Write(_currentMaxWidth);

                    KeyValuePair<int, Vector2[]>[] uvs = UVs.ToArray();

                    w.Write(uvs.Length);
                    for (int i = 0; i < uvs.Length; i++)
                    {
                        ref KeyValuePair<int, Vector2[]> uv = ref uvs[i];

                        w.Write(uv.Key);

                        if (uv.Value != null)
                        {
                            w.Write((byte)1);

                            w.Write(uv.Value[0]);
                            w.Write(uv.Value[1]);
                            w.Write(uv.Value[2]);
                            w.Write(uv.Value[3]);
                        }
                        else
                            w.Write((byte)0);
                    }

                    w.Flush();
                }
            }

            string texPath = Path.Combine(folder, "tileatlas.tex");

            if (File.Exists(texPath))
                File.Delete(texPath);

            byte[] textureBytes = GameTexture.EncodeToPNG();
            File.WriteAllBytes(texPath, textureBytes);
        }

        public static unsafe bool Import(string uvfile, string texfile)
        {
            if (!File.Exists(uvfile))
                return false;

            // For some reason the following commented code results in unity getting stuck at the map building process,
            // need to further investigate this

            //ConcurrentDictionary<int, Vector2[]> uvs;

            //using (FileStream uvstream = new FileStream(uvfile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            //using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(uvstream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, true))
            //using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
            //{
            //    byte* start = null;
            //    accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref start);

            //    if (start == null)
            //        throw new OperationCanceledException("Unable to acquire pointer for MemoryMappedFile");

            //    MemoryManager mem = new MemoryManager(start, uvstream.Length);

            //    _x = *mem.ReadInt();
            //    _y = *mem.ReadInt();
            //    _width = *mem.ReadInt();
            //    _height = *mem.ReadInt();
            //    _baseX = *mem.ReadFloat();
            //    _baseY = *mem.ReadFloat();
            //    _currentMaxWidth = *mem.ReadInt();

            //    int length = *mem.ReadInt();
            //    uvs = new ConcurrentDictionary<int, Vector2[]>();

            //    Parallel.For(0, length, i =>
            //    {
            //        int id = *mem.ReadInt();

            //        if (*mem.ReadByte() == 0)
            //            return;

            //        Vector2[] verts = new Vector2[]
            //        {
            //            new Vector2(*mem.ReadFloat(), *mem.ReadFloat()),
            //            new Vector2(*mem.ReadFloat(), *mem.ReadFloat()),
            //            new Vector2(*mem.ReadFloat(), *mem.ReadFloat()),
            //            new Vector2(*mem.ReadFloat(), *mem.ReadFloat()),
            //        };

            //        uvs.TryAdd(id, verts);
            //    });
            //}

            Dictionary<int, Vector2[]> uvs;
            using (FileStream uvstream = new FileStream(uvfile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader r = new BinaryReader(uvstream))
                {
                    _x = r.ReadInt32();
                    _y = r.ReadInt32();
                    _width = r.ReadInt32();
                    _height = r.ReadInt32();
                    _baseX = r.ReadSingle();
                    _baseY = r.ReadSingle();
                    _currentMaxWidth = r.ReadInt32();

                    int length = r.ReadInt32();
                    uvs = new Dictionary<int, Vector2[]>(length);

                    for (int i = 0; i < length; i++)
                    {
                        int id = r.ReadInt32();

                        if (r.ReadByte() == 0)
                            continue;

                        Vector2[] verts = new Vector2[]
                        {
                            r.ReadVector2(),
                            r.ReadVector2(),
                            r.ReadVector2(),
                            r.ReadVector2(),
                        };

                        uvs.Add(id, verts);
                    }
                }
            }

            if (!File.Exists(texfile))
                return false;

            byte[] textureBytes = File.ReadAllBytes(texfile);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(textureBytes);

            UVs = uvs;
            GameTexture = texture;
            return true;
        }

        public static void ApplyChanges()
        {
            GameTexture.Apply();
        }

        public static bool TryGetUVs(int id, out Vector2[] uvs)
        {
            return UVs.TryGetValue(id, out uvs);
        }

        public static Vector2[] AddTexture(int id, Texture2D tex, bool rotateIsometric, int rotations = 1)
        {
            if (UVs.TryGetValue(id, out Vector2[] oldUvs))
                return oldUvs;

            if (_y + tex.height >= _height)
            {
                _y = 0;
                _x += _currentMaxWidth;
            }

            Vector2[] uvs = CalculateUVs(_x, _y, tex.width, tex.height, rotateIsometric, rotations, true);

            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    var px = tex.GetPixel(x, y);
                    GameTexture.SetPixel(x + _x, y + _y, px /*new UnityEngine.Color(px.R / 255f, px.G / 255f, px.B / 255f, px.A / 255f)*/);
                }
            }

            if (_currentMaxWidth < tex.width)
                _currentMaxWidth = tex.width;

            _y += tex.height;

            UVs.Add(id, uvs);
            return uvs;
        }

        public static unsafe Vector2[] AddTexture(int id, ushort[] colors, int width, int height)
        {
            if (UVs.TryGetValue(id, out Vector2[] oldUvs))
                return oldUvs;

            if (_y + height >= _height)
            {
                _y = 0;
                _x += _currentMaxWidth;
                _currentMaxWidth = 0;
            }

            Vector2[] uvs = CalculateUVs(_x, _y, width, height, true, mirror: true);

            int colorIndex = 0;
            try
            {
                int x = 22;
                int y = 0;
                for (int xrun = 0; xrun < 22; xrun++)
                {
                    x--;

                    int lineWidth = 2 + 2 * xrun;

                    for (int xline = 0; xline < lineWidth; xline++)
                    {
                        ushort color = colors[colorIndex++];
                        var px = FromHue(color);
                        GameTexture.SetPixel(_x + x + xline, _y + y, px);
                    }

                    y++;
                }

                for (int xrun = 0; xrun < 22; xrun++)
                {
                    int lineWidth = 44 - xrun * 2;

                    for (int xline = 0; xline < lineWidth; xline++)
                    {
                        ushort color = colors[colorIndex++];
                        var px = FromHue(color);
                        GameTexture.SetPixel(_x + x + xline, _y + y, px);
                    }

                    x++;
                    y++;
                }
            }
            catch (Exception)
            {
                Debug.Log(colorIndex);

                throw;
            }

            if (_currentMaxWidth < width)
                _currentMaxWidth = width;

            _y += height;

            UVs.Add(id, uvs);
            return uvs;
        }

        public static Vector2[] AddNoDraw()
        {
            if (UVs.TryGetValue(int.MaxValue, out Vector2[] oldUvs))
                return oldUvs;

            if (_y + 44 >= _height)
            {
                _y = 0;
                _x += _currentMaxWidth;
                _currentMaxWidth = 0;
            }

            Vector2[] uvs = CalculateUVs(_x, _y, 44, 44, false);
            UnityEngine.Color color = new UnityEngine.Color(0, 0, 0, 1);

            for (int x = 0; x < 44; x++)
            {
                for (int y = 0; y < 44; y++)
                {
                    GameTexture.SetPixel(x + _x, y + _y, color);
                }
            }

            GameTexture.Apply();

            if (_currentMaxWidth < 44)
                _currentMaxWidth = 44;

            _y += 44;

            UVs.Add(int.MaxValue, uvs);
            return uvs;
        }

        public static Vector2[] AddSquareTexture(int id, ushort[] colors, int width, int height)
        {
            if (UVs.TryGetValue(id, out Vector2[] oldUvs))
                return oldUvs;

            if (_y + height >= _height)
            {
                _y = 0;
                _x += _currentMaxWidth;
                _currentMaxWidth = 0;
            }

            Vector2[] uvs = CalculateUVs(_x, _y, width, height, false);

            int colorIndex = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ushort color = colors[colorIndex];
                    var px = FromHue(color);
                    GameTexture.SetPixel(x + _x, y + _y, px);
                }
            }

            if (_currentMaxWidth < width)
                _currentMaxWidth = width;

            _y += height;

            UVs.Add(id, uvs);
            return uvs;
        }

        public static UnityEngine.Color FromHue(ushort hue)
        {
            float r = ((hue & 0x7c00) >> 10) * (255 / 31);
            float g = ((hue & 0x3e0) >> 5) * (255 / 31);
            float b = (hue & 0x1f) * (255 / 31);
            float a = 1f;

            return new UnityEngine.Color(r / 255f, g / 255f, b / 255f, a);
        }

        static Vector2[] CalculateUVs(float x, float y, float bmpWidth, float bmpHeight, bool rotateIsometric, int rotations = 0, bool mirror = false)
        {
            float xStart = (float)x / GameTexture.width;
            float yStart = (float)y / GameTexture.height;

            float width = bmpWidth / GameTexture.width;
            float height = bmpHeight / GameTexture.height;

            Vector2 bl = new Vector2(xStart, yStart);
            Vector2 tl = new Vector2(xStart, yStart + height);
            Vector2 tr = new Vector2(xStart + width, yStart + height);
            Vector2 br = new Vector2(xStart + width, yStart);

            if (rotateIsometric)
            { 
                float halfX = bmpWidth / 2f / GameTexture.width;
                float halfY = bmpHeight / 2f / GameTexture.height;

                bl.y += halfY;
                tl.x += halfX;
                tr.y -= halfY;
                br.x -= halfX;
            }

            Vector2[] result = new Vector2[]
            {
                bl, tl, tr, br
            };

            if (rotateIsometric)
            {

            }

            if (mirror)
            {
                result.Switch(0, 3);
                result.Switch(1, 2);
            }

            Vector2 vEnd;
            for (int r = 0; r < rotations; r++)
            {
                vEnd = result[3];

                Array.Copy(result, 0, result, 1, 3);
                result[0] = vEnd;
            }

            return result;
        }
    }
}
