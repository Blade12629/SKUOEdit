using Assets.Source.Game.Map;
using Assets.Source.UI;
using UnityEngine;

namespace Assets.Source
{
    public sealed class Client : MonoBehaviour
    {
        public static Client Instance { get; private set; }

        public Material DefaultMaterial => _defaultMaterial;
        [SerializeField] Material _defaultMaterial;

        GameMap _map;

        public Client()
        {
            Instance = this;
        }

        public void LoadMap(string path, int width, int depth, GameMap.GenerationOption option, int[] tileHeights, int[] tileIds)
        {
            if (_map != null)
            {
                _map.Destroy();
                _map = null;
            }

            _map = new GameObject("Game Map", typeof(GameMap)).GetComponent<GameMap>();
            _map.Load(path, width, depth, option, tileHeights, tileIds);
        }

        public void SaveMap(string path)
        {
            if (_map == null)
                return;

            _map.Save(path);
        }

        void Start()
        {
            GameFiles.LoadClientFiles();
            TileBrowser.Instance.LoadTiles();
        }
    }
}
