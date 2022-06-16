using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Textures
{
    public abstract class AtlasBase
    {
        const uint _NODRAW_ID = uint.MaxValue;

        public int Width { get; }
        public int Height { get; }
        public Texture2D Texture { get; }
        public IReadOnlyDictionary<uint, Vector2[]> UVs => _uvs;

        Dictionary<uint, Vector2[]> _uvs;

        int _x;
        int _xWidth;
        int _y;

        public AtlasBase(int width, int height)
        {
            Width = width;
            Height = height;
            _uvs = new Dictionary<uint, Vector2[]>();
            Texture = new Texture2D(width, height);
            Texture.SetPixelData(new Color[width * height], 0);
            Texture.Apply();

            Texture2D nodraw = new Texture2D(44, 44);
            Color[] colors = new Color[44 * 44];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.black;

            nodraw.SetPixels(colors);
            nodraw.Apply();

            AddOrGetTexture(_NODRAW_ID, nodraw);
        }

        public Vector2[] GetTexture(uint id)
        {
            if (_uvs.TryGetValue(id, out Vector2[] uvs))
                return uvs;

            return null;
        }

        public Vector2[] AddOrGetTexture(uint id, Texture2D texture, object fixUVsParam = null)
        {
            Vector2[] tempUVs = GetTexture(id);

            if (tempUVs != null)
                return tempUVs;

            if (_y + texture.height >= Height)
            {
                _y = 0;
                _x += _xWidth;
                _xWidth = 0;
            }

            float bx = _x / (float)Texture.width;
            float by = _y / (float)Texture.height;
            float bxEnd = (_x + texture.width - 1f) / Texture.width;
            float byEnd = (_y + texture.height - 1f) / Texture.height;

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(bx,     by),
                new Vector2(bx,     byEnd),
                new Vector2(bxEnd,  byEnd),
                new Vector2(bxEnd,  by),
            };

            FixUVs(uvs, 1f / Texture.width, 1f / Texture.height, fixUVsParam);

            _uvs.Add(id, uvs);

            Color[] pixels = texture.GetPixels();
            Texture.SetPixels(_x, _y, texture.width, texture.height, pixels);

            _y += texture.height;

            if (_xWidth < texture.width)
                _xWidth = texture.width;

            return uvs;
        }

        public Vector2[] GetNoDraw()
        {
            return GetTexture(_NODRAW_ID);
        }

        public void Apply()
        {
            Texture.Apply();
        }

        protected virtual void FixUVs(Vector2[] uvs, float widthPerPixel, float heightPerPixel, object param)
        {

        }
    }
}
