using Assets.Source.IO;
using Assets.Source.Ultima;
using Assets.Source.Game.Map.Items;
using UnityEngine;
using Assets.Source.Textures;

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

        ItemMap _items;
        MapStatics _statics;

        public void Load(string path, string staticPath, string staticIdxPath, bool isUop, int width, int height, int renderSize)
        {
            GameObject gterrain = new GameObject("terrain");
            _terrain = gterrain.AddComponent<UltimaTerrain>();
            _terrain.transform.SetParent(transform);

            _tiles = new MapTiles();
            _tiles.Load(path, isUop, width, height);

            _terrain.Initialize(renderSize, _tiles, Core.TerrainMaterial, TileAtlas.Instance.Texture);
            _terrain.SetVertices(Vector2.zero, true);

            _statics = new MapStatics();
            _statics.Load(staticPath, staticIdxPath, width, height);

            GameObject gitems = new GameObject("items");
            _items = gitems.AddComponent<ItemMap>();
            _items.transform.SetParent(transform);
            _items.Initialize(renderSize, Core.StaticMaterial, _statics);

            MoveToPosition(new Vector3(0, 0), true);
        }

        public void MoveToPosition(Vector3 position, bool repositionCamera)
        {
            Debug.Log(position);
            System.Console.WriteLine(position);

            _terrain.SetVertices(position, true);
            _items.SetPosition(position, true);

            if (repositionCamera)
            {
                Vector3 cameraPos = position;
                cameraPos.y = Height - cameraPos.y - 1;

                Core.Camera.MoveToPosition(cameraPos, false);
            }
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
