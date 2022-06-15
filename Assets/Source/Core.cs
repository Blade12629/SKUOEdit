using UnityEngine;
using Assets.Source.Ultima;
using Assets.Source.Game.Map;
using Assets.Source.Game;

namespace Assets.Source
{
    public class Core : MonoBehaviour
    {
        public static Core Instance { get; private set; }
        public static Map Map { get; private set; }
        public static Cam Camera { get; private set; }

        [SerializeField] Material _terrainMaterial;
        [SerializeField] Material _staticMaterial;
        [SerializeField] Cam _camera;

        public Core()
        {
            Instance = this;
        }

        void Start()
        {
            GameFiles.LoadClientFiles();

            Camera = _camera;
            Map = new GameObject("map").AddComponent<Map>();
            Map.Load(@"D:\reposSSD\SKUOEdit\Test\map3LegacyMUL.uop", true, 2560, 2048, 95, _terrainMaterial, _staticMaterial);

            Camera.OnMoved += e => Map.MoveToPosition(e.NewPosition);
        }
    }
}
