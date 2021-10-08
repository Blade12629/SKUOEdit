using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source.Textures
{
    public unsafe class TextureAtlas
    {
        public Texture2D Texture { get; private set; }

        Dictionary<int, Vector2[]> _uvs;

        Vector2 _position;
        Vector2 _size;
        Vector2 _singleStepSize;
        int _currentMaxWidth;

        public void Initialize()
        {
            Texture = new Texture2D(6000, 6000);
            _uvs = new Dictionary<int, Vector2[]>();

            Color[] colors = new Color[Texture.width * Texture.height];
            Texture.SetPixels(colors);
            Texture.Apply();
        }

        Color FromHue(short hue)
        {
            float r = ((hue & 0x7c00) >> 10) * (255 / 31);
            float g = ((hue & 0x3e0) >> 5) * (255 / 31);
            float b = (hue & 0x1f) * (255 / 31);
            float a = 1f;

            return new Color(r / 255f, g / 255f, b / 255f, a);
        }

        Vector2[] CalculateUVs(float x, float y, float bmpWidth, float bmpHeight, bool rotateIsometric)
        {
            float xStart = (float)x / Texture.width;
            float yStart = (float)y / Texture.height;

            float width = bmpWidth / Texture.width;
            float height = bmpHeight / Texture.height;

            if (!rotateIsometric)
            {
                return new Vector2[]
                {
                    new Vector2(xStart,         yStart),
                    new Vector2(xStart,         yStart + height),
                    new Vector2(xStart + width, yStart + height),
                    new Vector2(xStart + width, yStart),
                };
            }

            float halfX = bmpWidth / 2f / Texture.width;
            float halfY = bmpHeight / 2f / Texture.height;

            return new Vector2[]
            {
                new Vector2(xStart,                     yStart + halfY),
                new Vector2(xStart + halfX,             yStart + height),
                new Vector2(xStart + width,             yStart + height - halfY),
                new Vector2(xStart + width - halfX,     yStart),
            };
        }
    }
}
