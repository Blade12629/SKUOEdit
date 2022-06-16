using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Ultima
{
    public static class Art
    {
        public static Texture2D AtlasTexture => GameTextures.GameTexture;
        public static Texture2D AtlasTextureItems => null; // TODO: texture atlas for items

        public static Texture2D GetTile(uint id)
        {
            return ClassicUO.IO.Resources.ArtLoader.Instance.GetLandTexture(id);
        }

        public static Texture2D GetStatic(uint id)
        {
            return ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(id);
        }

        public static Vector2[] GetStaticUVs(uint itemId)
        {
            throw new NotImplementedException();
            //return ItemTextures.GetUVs(itemId); // TODO: build texture atlas
        }

        public static Vector2 GetStaticSize(uint itemId)
        {
            Texture2D item = GetStatic(itemId);
            return new Vector2(item.width, item.height);
        }

        public static Vector2[] GetTileUVs(int id, bool isTexture)
        {
            Vector2[] uvs = null;

            if (isTexture)
            {
                ref var landData = ref ClassicUO.IO.Resources.TileDataLoader.Instance.LandData[id];

                if (landData.TexID > 0)
                    uvs = GameTextures.GetUVsTexture(landData.TexID);
                else
                    uvs = GameTextures.AddNoDraw();
            }
            else
            {
                uvs = GameTextures.GetUVsTile(id);
            }

            return uvs;
        }
    }
}
