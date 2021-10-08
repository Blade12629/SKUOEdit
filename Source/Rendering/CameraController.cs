//using Assets.Source.GameEditor;
//using Assets.Source.Terrain;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Assets.Source.Rendering
//{
//    public class CameraController
//    {
//        /// <summary>
//        /// Called when the camera has been moved
//        /// </summary>
//        public event Action<Vector3> OnCameraPositionChanged;
//        public int TerrainLayerMask => _terrainLayerMask;

//        public int CameraOffsetX;
//        public int CameraOffsetZ;

//        MapController _mapController;
//        SelectionRenderer _selectionRenderer;

//        int _terrainLayerMask;

//        Text _cameraOffsetText;

//        public CameraController(MapController mapController, SelectionRenderer selectionRenderer, Text cameraOffsetText)
//        {
//            _mapController = mapController;
//            _selectionRenderer = selectionRenderer;
//            _cameraOffsetText = cameraOffsetText;

//            _terrainLayerMask = 1 << 8;
//        }

//        /// <summary>
//        /// Positions the camera at the correct position offset
//        /// </summary>
//        /// <param name="pos">Position to take into consideration when applying the offsets, y is ignored</param>
//        public void InvalidatePosition(Vector3 pos)
//        {
//            Camera.main.transform.position = new Vector3(CameraOffsetX + pos.x,
//                                                         Camera.main.transform.position.y,
//                                                         CameraOffsetZ + pos.z);

//            _cameraOffsetText.text = $"X: {(int)pos.z}\nZ: {(int)pos.x}";

//            _mapController.Minimap?.SetMarkerPosition(pos.x, pos.z);
//            OnCameraPositionChanged?.Invoke(pos);
//        }

//        public void Update()
//        {
//            Vector3 dir = Vector3.zero;

//            if (Input.GetKey(KeyCode.W))
//            {
//                dir.x--;
//                dir.z--;
//            }
//            if (Input.GetKey(KeyCode.S))
//            {
//                dir.x++;
//                dir.z++;
//            }

//            if (Input.GetKey(KeyCode.A))
//            {
//                dir.x++;
//                dir.z--;
//            }
//            if (Input.GetKey(KeyCode.D))
//            {
//                dir.x--;
//                dir.z++;
//            }

//            if (!dir.Equals(Vector3.zero))
//            {
//                InvalidatePosition(new Vector3(_mapController.XOffset + dir.x, 0f, _mapController.ZOffset + dir.z));
//            }

//            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//            if (Physics.Raycast(ray, out RaycastHit hit, 100000f, _terrainLayerMask))
//            {
//                _selectionRenderer.SetPosition(hit.point);
//            }
//        }
//    }
//}
