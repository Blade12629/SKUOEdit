using Assets.Source.Ultima;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Textures
{
    public class TerrainAtlas : AtlasBase
    {
        const uint _TEXTURE_OFFSET = int.MaxValue;

        public static TerrainAtlas Instance { get; private set; }

        object _paramObj = new object();

        public TerrainAtlas(int width, int height) : base(width, height)
        {

        }

        static TerrainAtlas()
        {
            Instance = new TerrainAtlas(6000, 6000); // TODO: terrain atlas default width + height
        }

        public Vector2[] AddOrGetTile(uint id)
        {
            Vector2[] uvs = GetTexture(id);

            if (uvs != null)
                return uvs;

            return AddOrGetTexture(id, Art.GetTile(id), _paramObj);
        }

        public Vector2[] AddOrGetTileTexture(uint id)
        {
            ref var landData = ref ClassicUO.IO.Resources.TileDataLoader.Instance.LandData[id];

            if (landData.TexID > 0)
            {
                Vector2[] uvs = GetTexture(landData.TexID + _TEXTURE_OFFSET);

                if (uvs == null)
                    uvs = AddOrGetTexture(landData.TexID + _TEXTURE_OFFSET, Art.GetTile(landData.TexID));

                return uvs;
            }

            return GetNoDraw();
        }

        protected override void FixUVs(Vector2[] uvs, float widthPerPixel, float heightPerPixel, object param)
        {
            if (param == null)
            {
                // adding texture

                // rotate
                Vector2 r = uvs[3];
                Array.Copy(uvs, 0, uvs, 1, 3);
                uvs[0] = r;
            }
            else
            {
                // adding tile

                // rotate isometric
                float w = widthPerPixel * 22f;
                float h = heightPerPixel * 22f;

                uvs[0].y += h;
                uvs[1].x += w;
                uvs[2].y -= h;
                uvs[3].x -= w;
            }

            // mirror
            uvs.Switch(0, 3);
            uvs.Switch(1, 2);
        }
    }
}
