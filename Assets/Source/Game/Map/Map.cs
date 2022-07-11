using Assets.Source.IO;
using Assets.Source.Ultima;
using Assets.Source.Game.Map.Items;
using Assets.Source.Game.Map.Terrain;
using UnityEngine;
using Assets.Source.Textures;

namespace Assets.Source.Game.Map
{
    public class Map : MonoBehaviour
    {
        public int RenderSize
        {
            get => _terrain.RenderSize;
        }

        public int Width => _tiles.Width;
        public int Height => _tiles.Depth;

        UOTerrain _terrain;
        MapTiles _tiles;

        ItemMap _items;
        MapStatics _statics;

        public void Load(string path, string staticPath, string staticIdxPath, bool isUop, int width, int height, int renderSize)
        {
            _tiles = new MapTiles();
            _tiles.Load(path, isUop, width, height);

            GameObject gterrain = new GameObject("terrain");
            _terrain = gterrain.AddComponent<UOTerrain>();
            _terrain.transform.SetParent(transform);
            _terrain.Initialize(renderSize, _tiles, Core.TerrainMaterial, TileAtlas.Instance.Texture);

            _statics = new MapStatics();
            _statics.Load(staticPath, staticIdxPath, width, height);

            GameObject gitems = new GameObject("items");
            _items = gitems.AddComponent<ItemMap>();
            _items.transform.SetParent(transform);
            _items.Initialize(renderSize, Core.StaticMaterial, _statics);
        }

        /// <summary>
        /// Do not call this, instead use <see cref="Cam.MoveToPosition(Vector3, bool)"/>
        /// </summary>
        public void MoveToPosition(Vector3 position)
        {
            Debug.Log(position);
            System.Console.WriteLine(position);

            _terrain.MoveToWorld(position);
            _items.SetPosition(position, true);
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
