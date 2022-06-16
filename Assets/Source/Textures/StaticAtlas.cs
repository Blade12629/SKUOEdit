using Assets.Source.Ultima;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Textures
{
    public class StaticAtlas : AtlasBase
    {
        public static StaticAtlas Instance { get; private set; }

        public StaticAtlas(int width, int height) : base(width, height)
        {

        }

        static StaticAtlas()
        {
            Instance = new StaticAtlas(8000, 8000); // TODO: find optimal width + height
        }

        public Vector2[] AddOrGetStatic(uint id)
        {
            Vector2[] uvs = GetTexture(id);

            if (uvs != null)
                return uvs;

            return AddOrGetTexture(id, Art.GetStatic(id));
        }
    }
}
