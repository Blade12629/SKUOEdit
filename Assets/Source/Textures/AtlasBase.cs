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
            Texture = new Texture2D(width, height);
            Texture.SetPixelData(new Color[width * height], 0);
            Texture.Apply();

            _uvs = new Dictionary<uint, Vector2[]>();
        }

        public Vector2[] AddOrGetTexture(uint id, Texture2D texture)
        {
            if (_uvs.TryGetValue(id, out Vector2[] tempUVs))
                return tempUVs;

            if (_y + texture.height >= Height)
            {
                _y = 0;
                _x += _xWidth;
                _xWidth = 0;
            }

            int bx = _x / Texture.width;
            int by = _y / Texture.height;
            int bxEnd = (_x + texture.width - 1) / Texture.width;
            int byEnd = (_y + texture.height - 1) / Texture.height;

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(bx,     by),
                new Vector2(bx,     byEnd),
                new Vector2(bxEnd,  byEnd),
                new Vector2(bxEnd,  by),
            };

            _uvs.Add(id, uvs);

            Color[] pixels = texture.GetPixels();
            Texture.SetPixels(_x, _y, texture.width, texture.height, pixels);

            _y += texture.height;

            if (_xWidth < texture.width)
                _xWidth = texture.width;

            return uvs;
        }

        public void Apply()
        {
            Texture.Apply();
        }
    }
}
