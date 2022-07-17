using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using Assets.Source.Ultima;

namespace Assets.Source.Textures
{
    public class TileAtlas
    {
        /// <summary>
        /// Nodraw texture id
        /// </summary>
        const uint NODRAW_ID = uint.MaxValue;
        /// <summary>
        /// Texture offset for tile textures
        /// </summary>
        const uint TEXTURE_OFFSET = 500000;

        public static TileAtlas Instance { get; } 

        public Texture2D Texture { get; private set; }

        Dictionary<uint, Vector2[]> _uvs;
        int _x;
        int _xWidth;
        int _y;

        TileAtlas(int width, int height)
        {
            _uvs = new Dictionary<uint, Vector2[]>();
            Initialize(width, height);
        }

        static TileAtlas()
        {
            Instance = new TileAtlas(5000, 5000);
        }

        public Vector2[] GetTileUVs(uint id)
        {
            return GetUVs(id);
        }

        public Vector2[] GetTileTextureUVs(uint id)
        {
            return GetUVs(id + TEXTURE_OFFSET);
        }

        public Vector2[] GetNoDraw()
        {
            return GetUVs(NODRAW_ID);
        }

        public void ApplyChanges()
        {
            Texture.Apply();
        }

        void AddTile(Texture2D tex, uint id)
        {
            AddTexture(tex, id, true, true, false, false);
        }

        void AddTileTexture(Texture2D tex, uint id)
        {
            AddTexture(tex, id + TEXTURE_OFFSET, true, false, true, true);
        }

        bool AddTexture(Texture2D tex, uint id, bool mirror, bool toIsometric, bool rotate, bool mirrorFirst)
        {
            if (tex == null)
                return false;

            return AddTexture(tex.GetPixels32(), tex.width, tex.height, id, mirror, toIsometric, rotate, mirrorFirst);
        }

        bool AddTexture(Color32[] texData, int texWidth, int texHeight, uint id, bool mirror, bool toIsometric, bool rotate, bool mirrorFirst)
        {
            if (_uvs.ContainsKey(id) || texData.Length != texWidth * texHeight)
                return false;

            if (_y + texHeight >= Texture.height)
            {
                _y = 0;
                _x += _xWidth;
                _xWidth = 0;
            }

            float bx = _x / (float)Texture.width;
            float by = _y / (float)Texture.height;
            float bxEnd = (_x + texWidth) / (float)Texture.width;
            float byEnd = (_y + texHeight) / (float)Texture.height;

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(bx,     by),
                new Vector2(bx,     byEnd),
                new Vector2(bxEnd,  byEnd),
                new Vector2(bxEnd,  by),
            };

            TransformUVs(uvs, mirror, toIsometric, rotate, mirrorFirst);
            Texture.SetPixels32(_x, _y, texWidth, texHeight, texData);

            _uvs.Add(id, uvs);

            _y += texHeight;

            if (_xWidth < texWidth)
                _xWidth = texWidth;

            return true;
        }

        void TransformUVs(Vector2[] uvs, bool mirror, bool toIsometric, bool rotate, bool mirrorFirst)
        {
            if (mirrorFirst)
            {
                Mirror();
                ToIsometric();
                Rotate();
            }
            else
            {
                Rotate();
                ToIsometric();
                Mirror();
            }


            void Rotate()
            {
                if (rotate)
                {
                    Vector2 r = uvs[3];
                    Array.Copy(uvs, 0, uvs, 1, 3);
                    uvs[0] = r;
                }
            }

            void ToIsometric()
            {
                if (toIsometric)
                {
                    float w = 22f / Texture.width;
                    float h = 22f / Texture.height;

                    uvs[0].y += h;
                    uvs[1].x += w;
                    uvs[2].y -= h;
                    uvs[3].x -= w;
                }
            }

            void Mirror()
            {
                if (mirror)
                {
                    uvs.Switch(0, 3);
                    uvs.Switch(1, 2);
                }
            }

        }

        void Initialize(int width, int height)
        {
            if (!Load(GameConfig.TileAtlasFile))
            {
                Texture = new Texture2D(width, height);

                GenerateAtlas();
                ApplyChanges();

                Save(GameConfig.TileAtlasFile, true);

                return;
            }
            else
            {
                ApplyChanges();
                if (!_uvs.ContainsKey(NODRAW_ID))
                    AddNoDraw();
            }
        }

        Vector2[] GetUVs(uint id)
        {
            if (_uvs.TryGetValue(id, out Vector2[] uvs))
                return uvs;

            return GetNoDraw();
        }

        void AddNoDraw()
        {
            Color32[] nodrawData = new Color32[44 * 44];
            Color32 nodrawColor = new Color32(0, 0, 0, 1);

            for (int i = 0; i < nodrawData.Length; i++)
                nodrawData[i] = nodrawColor;

            AddTexture(nodrawData, 44, 44, NODRAW_ID, false, false, false, false);
        }

        void GenerateAtlas()
        {
            AddNoDraw();

            int tileCount = ClassicUO.IO.Resources.TileDataLoader.Instance.LandData.Length;

            for (uint i = 0; i < tileCount; i++)
                AddTile(UltimaArt.GetTile(i), i);

            for (uint i = 0; i < tileCount; i++)
            {
                ref var land = ref ClassicUO.IO.Resources.TileDataLoader.Instance.LandData[i];

                if (land.TexID > 0)
                {
                    Texture2D tex = UltimaArt.GetTexture(land.TexID);
                    AddTileTexture(tex, i);
                }
            }
        }

        bool Load(string path)
        {
            if (!File.Exists(path))
                return false;

            _uvs.Clear();

            using (FileStream stream = GetStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, false, false))
            using (BinaryReader r = new BinaryReader(stream))
            {
                Texture = new Texture2D(r.ReadInt32(), r.ReadInt32());

                _x = r.ReadInt32();
                _xWidth = r.ReadInt32();
                _y = r.ReadInt32();

                int uvCount = r.ReadInt32();
                for (int i = 0; i < uvCount; i++)
                {
                    uint id = r.ReadUInt32();
                    Vector2[] uvs = new Vector2[r.ReadInt32()];

                    for (int j = 0; j < uvs.Length; j++)
                        uvs[j] = r.ReadVector2();

                    _uvs[id] = uvs;
                }

                Color32[] colors = new Color32[r.ReadInt32()];
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = r.ReadColor32();

                Texture.SetPixels32(colors);
            }

            return true;
        }

        void Save(string path, bool backup)
        {
            using (FileStream stream = GetStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, true, backup))
            using (BinaryWriter w = new BinaryWriter(stream))
            {
                Color32[] imgData = Texture.GetPixels32();

                w.Write(Texture.width);
                w.Write(Texture.height);
                w.Write(_x);
                w.Write(_xWidth);
                w.Write(_y);
                w.Write(_uvs.Count);

                foreach (var pair in _uvs)
                {
                    w.Write(pair.Key);
                    w.Write(pair.Value.Length);

                    for (int i = 0; i < pair.Value.Length; i++)
                        w.Write(pair.Value[i]);
                }

                w.Write(imgData.Length);
                for (int i = 0; i < imgData.Length; i++)
                    w.Write(imgData[i]);

                stream.Flush();
            }

            using (var bmp = Texture.ToBitmap())
                bmp.Save($"{path}.bmp");
        }

        FileStream GetStream(string path, FileMode mode, FileAccess access, FileShare share, bool deleteOriginal, bool backupOriginal)
        {
            if (deleteOriginal)
            {
                if (backupOriginal && File.Exists(path))
                {
                    string backupPath = $"{path}.backup";

                    if (File.Exists(backupPath))
                        File.Delete(backupPath);

                    File.Move(path, backupPath);
                }
            }

            return new FileStream(path, mode, access, share);
        }
    }
}
