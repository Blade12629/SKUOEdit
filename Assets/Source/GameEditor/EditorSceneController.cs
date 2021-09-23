using Assets.Source.Rendering;
using Assets.Source.Terrain;
//using SKMapGenerator.Ultima;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.GameEditor
{
    public class EditorSceneController : MonoBehaviour
    {
        public static EditorSceneController Instance { get; private set; }

        public CameraController Camera { get; private set; }

        public GameFiles GameFiles { get; private set; }
        public HeightTable Heights { get; private set; }
        public MapController MapController { get; private set; }
        public EditorInput EditorInput { get; private set; }

        [SerializeField] TerrainMesh _mesh;
        [SerializeField] SelectionRenderer _selectionRenderer;
        [SerializeField] Text _cameraOffsetText;

        [SerializeField] RawImage _minimapImg;
        [SerializeField] RawImage _minimapMarkerImg;

        [SerializeField] GameObject _staticsHolder;

        [SerializeField] StaticMesh _staticMesh;
        [SerializeField] StaticMeshOriginal _staticMeshOrig;

#if UNITY_EDITOR
        [SerializeField] bool _saveTexAtlas;
        [SerializeField] string _saveTexAtlasPath;
#endif

        void Awake()
        {
            Instance = this;

            StartCoroutine(InitializeScene());
        }

        IEnumerator InitializeScene()
        {
            MapController = new MapController(_mesh, _staticsHolder);
            Camera = new CameraController(MapController, _selectionRenderer, _cameraOffsetText);
            GameFiles = new GameFiles();
            EditorInput = new EditorInput(Camera.TerrainLayerMask, MapController);

            GameFiles.LoadClientFiles();

            yield break;
            yield return new WaitForEndOfFrame();

            MapController.SetupMapCamera(Camera);
            
            yield return new WaitForEndOfFrame();
            
            LoadMap();

            while (!MapController.IsMapBuilt)
                yield return new WaitForEndOfFrame();

            MapController.MoveToPosition(535, 1020, true);
            MapController.SpawnArea(890, 440, 200, 200, GameFiles.StaticTileMatrix);
        }

        void LoadMap()
        {
            int index = 3;
            int width = 2560;
            int depth = 2048;

            GameFiles.LoadMap(index, width, depth);
            Heights = new HeightTable(width, depth);
            _selectionRenderer.Init(Heights);

            Debug.Log("--- Building map ---");

            MapController.BuildMap(Camera, GameFiles.TileMatrix, GameFiles.StaticTileMatrix, Heights, _minimapImg, _minimapMarkerImg, width, depth, this);
        }

        void Update()
        {
            Camera.Update();
            EditorInput.Update();
            MapController.Update();

#if UNITY_EDITOR
            if (_saveTexAtlas)
            {
                _saveTexAtlas = false;
                Textures.GameTextures.Export(new System.IO.FileInfo(GamePaths.TileAtlasTexFile).Directory.FullName);
            }
#endif
        }
    }
}
