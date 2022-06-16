using Assets.Source.IO;
using Assets.Source.Ultima;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    public class Map : MonoBehaviour
    {
        public int RenderSize
        {
            get => _terrain.Size;
            set => _terrain.Size = value;
        }
        public int Width => _tiles.Width;
        public int Height => _tiles.Depth;

        UltimaTerrain _terrain;
        MapTiles _tiles;

        public void Load(string path, bool isUop, int width, int height, int renderSize)
        {
            // TODO: statics: texture.height / (float)texture.width

            GameObject gterrain = new GameObject("terrain");
            _terrain = gterrain.AddComponent<UltimaTerrain>();
            _terrain.transform.SetParent(transform);

            _tiles = new MapTiles();
            _tiles.Load(path, isUop, width, height);

            _terrain.Initialize(renderSize, _tiles, Core.TerrainMaterial, Art.AtlasTexture);
            _terrain.SetVertices(Vector2.zero, true);
        }

        public void MoveToPosition(Vector3 position)
        {
            _terrain.SetVertices(position, false);
        }

        public void Clear()
        {
            _terrain.Clear();
        }

        public void Delete()
        {
            _terrain.Delete();

            Destroy(gameObject);
        }
    }
}
