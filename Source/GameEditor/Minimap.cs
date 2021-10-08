//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Assets.Source.GameEditor
//{
//    public class Minimap
//    {
//        RawImage _mapImage;
//        Texture2D _mapTexture;

//        RawImage _markerImage;
//        Texture2D _positionMarker;

//        static readonly Color _transparentColor = new Color(0, 0, 0, 0);
//        static readonly Color _invalidColor = new Color(1, 1, 1, 1);
//        static readonly Color _markerColor = new Color(1, 192 / 255, 203 / 255, 1);

//        public Minimap(RawImage mapImage, RawImage markerImage, Texture2D mapTexture)
//        {
//            _mapImage = mapImage;
//            _markerImage = markerImage;
//            _mapTexture = mapTexture;
//            _mapImage.texture = _mapTexture;
//        }

//        public Minimap(RawImage mapImage, RawImage markerImage, int width, int height) : this(mapImage, markerImage, new Texture2D(width, height))
//        {
//            CreatePositionMarker(15);
//        }

//        public void SetTile(int x, int z, Color c)
//        {
//            _mapTexture.SetPixel(z, _mapTexture.height - x, c);
//        }

//        public void SetMap(Color32[] colors)
//        {
//            _mapTexture.SetPixels32(colors);
//            Apply();
//        }

//        public void Clear(int x, int z)
//        {
//            SetTile(x, z, _invalidColor);
//        }

//        public void OnMapClick(Vector3 mapPos)
//        {
//            EditorSceneController.Instance.Camera.InvalidatePosition(new Vector3(mapPos.x, 0, mapPos.z));
//        }

//        public void SetMarkerPosition(float x, float z)
//        {
//            x += EditorSceneController.Instance.Camera.CameraOffsetX / _mapImage.rectTransform.sizeDelta.x;
//            z += EditorSceneController.Instance.Camera.CameraOffsetZ / _mapImage.rectTransform.sizeDelta.y;

//            x -= _mapImage.rectTransform.sizeDelta.x / 2f;
//            z -= _mapImage.rectTransform.sizeDelta.y / 2f;

//            _markerImage.rectTransform.localPosition = new Vector3(x, z * -1);
//        }

//        public void Apply()
//        {
//            _mapTexture.Apply();
//        }

//        void CreatePositionMarker(int size)
//        {
//            if (_positionMarker == null || _positionMarker.width != size)
//            {
//                _positionMarker = new Texture2D(size, size);
//                _markerImage.texture = _positionMarker;
//            }

//            int endPos = size - 2;
//            int endPos2 = size - 1;

//            for (int i = 1; i < size - 1; i++)
//            {
//                _positionMarker.SetPixel(i, 0, _transparentColor);
//                _positionMarker.SetPixel(i, 1, _markerColor);
//                _positionMarker.SetPixel(i, endPos, _markerColor);
//                _positionMarker.SetPixel(i, endPos2, _transparentColor);

//                _positionMarker.SetPixel(0, i, _transparentColor);
//                _positionMarker.SetPixel(1, i, _markerColor);
//                _positionMarker.SetPixel(endPos, i, _markerColor);
//                _positionMarker.SetPixel(endPos2, i, _transparentColor);
//            }

//            _positionMarker.SetPixel(0,         0, _transparentColor);
//            _positionMarker.SetPixel(endPos2,   0, _transparentColor);
//            _positionMarker.SetPixel(0,         endPos2, _transparentColor);
//            _positionMarker.SetPixel(endPos2,   endPos2, _transparentColor);

//            for (int x = 2; x < size - 2; x++)
//            {
//                for (int y = 2; y < size - 2; y++)
//                {
//                    _positionMarker.SetPixel(x, y, _transparentColor);
//                }
//            }

//            _positionMarker.Apply();
//        }
//    }
//}
