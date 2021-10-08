using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Source.Game;
using Assets.Source.Textures;

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

        void Start()
        {
            GameFiles.LoadClientFiles();
            
            int mapIndex = 3;
            int mapWidth = 2560;
            int mapDepth = 2048;

            _map = new GameObject("Game Map", typeof(GameMap)).GetComponent<GameMap>();
            _map.Load(GameFiles.GetUOPath($"map{mapIndex}.mul"), mapWidth, mapDepth);

            CameraController.Instance.InitializePosition();
        }

        void OnApplicationQuit()
        {

        }
    }
}
