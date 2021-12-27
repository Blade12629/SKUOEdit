using Assets.Source.Game.Map;
using Assets.Source.Threading;
using Assets.Source.UI;
using System.Collections;
using UnityEngine;

namespace Assets.Source
{
    public sealed class Client : MonoBehaviour
    {
        public static Client Instance { get; private set; }

        public Material DefaultMaterial => _defaultMaterial;
        public Material DefaultStaticMaterial => _defaultStaticMaterial;

        [SerializeField] Material _defaultMaterial;
        [SerializeField] Material _defaultStaticMaterial;
        [SerializeField] LoadingBar _loadingbar;

        GameMap _map;

        public Client()
        {
            Instance = this;
        }

        public void GenerateMap(GenerationOption option, int width, int depth, int[] tileHeights, int[] tileIds)
        {
            _loadingbar.gameObject.SetActive(true);
            _loadingbar.Text = "Generating Map";

            PrepareMap();

            _map.Generate(option, width, depth, tileHeights, tileIds, _loadingbar);
        }

        public void LoadMap(string path, int width, int depth)
        {
            _loadingbar.gameObject.SetActive(true);
            _loadingbar.Text = "Loading Map";

            PrepareMap();

            _map.Load(path, width, depth, _loadingbar);
        }

        public void SaveMap(string path)
        {
            if (_map == null)
                return;

            _map.Save(path);
        }

        void PrepareMap()
        {
            if (_map != null)
            {
                _map.Destroy();
                _map = null;
            }

            _map = new GameObject("Game Map", typeof(GameMap)).GetComponent<GameMap>();
        }

        void Start()
        {
            if (GameConfig.ConfigNotFound)
            {
                Debug.LogError("Config not found, saving config and exiting client");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif

                return;
            }

            UThreading.Init();

            GameFiles.LoadClientFiles();
            TileBrowser.Instance.LoadTiles();
        }

        private void OnApplicationQuit()
        {
            GameConfig.Save();
        }
    }
}
