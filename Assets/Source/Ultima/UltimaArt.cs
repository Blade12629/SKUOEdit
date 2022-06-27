//using Assets.Source.Textures;
using Assets.Source.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Ultima
{
    public static class UltimaArt
    {
        public static Texture2D GetTile(uint id)
        {
            return ClassicUO.IO.Resources.ArtLoader.Instance.GetLandTexture(id);
        }

        public static Texture2D GetTexture(uint id)
        {
            return ClassicUO.IO.Resources.TexmapsLoader.Instance.GetTexture(id);
        }

        public static Texture2D GetStatic(uint id)
        {
            return ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(id);
        }
    }
}
