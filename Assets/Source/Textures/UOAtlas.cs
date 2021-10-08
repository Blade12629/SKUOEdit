using UnityEngine;

namespace Assets.Source.Textures
{
    public static class UOAtlas
    {
        public static Texture2D AtlasTexture => GameTextures.GameTexture;

        static int _textureIdOffset = 100000;

        public static void Initialize(bool drawTransparent)
        {
            GameTextures.Initialize(drawTransparent);
        }

        public static Vector2[] AddNoDraw()
        {
            return GameTextures.AddNoDraw();
        }

        public static Vector2[] GetUVsTile(int id)
        {
            if (GameTextures.TryGetUVs(id, out Vector2[] uvs))
                return uvs;

            Debug.Log($"Tile art not found {id}, returning NoDraw");

            return AddNoDraw();
        }

        public static Vector2[] GetUVsTexture(int id)
        {
            if (GameTextures.TryGetUVs(id + _textureIdOffset, out Vector2[] uvs))
                return uvs;

            Debug.Log($"Texture art not found {id}, returning NoDraw");

            return AddNoDraw();
        }

        public static Vector2[] AddTile(int id)
        {
            if (id < 0)
                return null;

            var img = ClassicUO.IO.Resources.ArtLoader.Instance.GetLandTexture((uint)id);

            return GameTextures.AddTexture(id, img, false, 0);
        }

        public static Vector2[] AddTexture(int id)
        {
            if (id <= 0)
                return null;

            var img = ClassicUO.IO.Resources.ArtLoader.Instance.GetLandTexture((uint)id);

            if (img == null)
                return null;

            id += _textureIdOffset;
            return GameTextures.AddTexture(id, img, false);
        }

        public static void ApplyChanges()
        {
            GameTextures.ApplyChanges();
        }
    }
}
