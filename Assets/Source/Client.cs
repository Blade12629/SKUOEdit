using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Source.Game;
using Assets.Source.Textures;
using Assets.Source.UI;

namespace Assets.Source
{
    public class Client : MonoBehaviour
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

            for (short i = 0; i < ClassicUO.Game.Constants.MAX_LAND_DATA_INDEX_COUNT; i++)
            {
                TileBrowser.Instance.AddTile(i);
            }

            GameMap.OnMapFinishLoading += () =>
            {
                TileBrowser.Instance.SelectDefault();
                CameraController.Instance.InitializePosition();
            };
        }

        void OnApplicationQuit()
        {

        }
    }
}
