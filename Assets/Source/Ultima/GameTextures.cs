using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Source.Ultima
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

        static readonly int _textureIdOffset = 100000;

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

        public static unsafe bool Import(string uvfile, string texfile)
        {
            if (!File.Exists(uvfile))
                return false;

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

        public static Vector2[] GetUVsTile(int id)
        {
            if (TryGetUVs(id, out Vector2[] uvs))
                return uvs;

            Debug.Log($"Tile art not found {id}, returning NoDraw");

            return AddNoDraw();
        }

        public static Vector2[] GetUVsTexture(int id)
        {
            if (TryGetUVs(id + _textureIdOffset, out Vector2[] uvs))
                return uvs;

            Debug.Log($"Texture art not found {id}, returning NoDraw");

            return AddNoDraw();
        }

        public static Vector2[] AddTile(int id)
        {
            if (id < 0)
                return null;

            var img = ClassicUO.IO.Resources.ArtLoader.Instance.GetLandTexture((uint)id);

            return AddTexture(id, img, false, 0);
        }

        public static Vector2[] AddTexture(int id)
        {
            if (id <= 0)
                return null;

            var img = ClassicUO.IO.Resources.ArtLoader.Instance.GetLandTexture((uint)id);

            if (img == null)
                return null;

            id += _textureIdOffset;
            return AddTexture(id, img, false);
        }

        public static void ApplyChanges()
        {
            GameTexture.Apply();
        }

        static void Export(string folder)
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

        static bool TryGetUVs(int id, out Vector2[] uvs)
        {
            return UVs.TryGetValue(id, out uvs);
        }

        static Vector2[] AddTexture(int id, Texture2D tex, bool rotateIsometric, int rotations = 1)
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
