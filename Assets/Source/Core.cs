using UnityEngine;
using Assets.Source.Ultima;
using Assets.Source.Game.Map;
using Assets.Source.Game;
using Assets.Source.IO;

namespace Assets.Source
{
    public class Core : MonoBehaviour
    {
        public static Core Instance { get; private set; }
        public static Map Map { get; private set; }
        public static Cam Camera { get; private set; }
        public static Material TerrainMaterial { get; private set; }
        public static Material StaticMaterial { get; private set; }

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

            TerrainMaterial = _terrainMaterial;
            StaticMaterial = _staticMaterial;

            Camera = _camera;
            Map = new GameObject("map").AddComponent<Map>();
            Map.Load(@"D:\reposSSD\SKUOEdit\Test\map3LegacyMUL.uop",
                     @"D:\reposSSD\SKUOEdit\Test\statics3.mul",
                     @"D:\reposSSD\SKUOEdit\Test\staidx3.mul",
                     true, 2560, 2048, 95);

            Camera.OnMoved += e => Map.MoveToPosition(e.NewPosition, false);
        }
    }
}
