using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using ClassicUO.IO.Resources;
using UnityEngine.EventSystems;
using Assets.Source.Game;

namespace Assets.Source.UI
{
    public unsafe class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        /// <summary>
        /// Vector3 contains map position
        /// </summary>
        public static event Action<Vector3> OnMinimapPositionChange;
        public static Minimap Instance { get; private set; }

        [SerializeField] RawImage _mapImg;
        [SerializeField] RawImage _markerImg;

        Texture2D _markerTexture;
        Texture2D _mapTexture;
        Vector2 _mapImgSize;
        Vector2 _mapImgSizeHalf;

        static readonly Color _transparentColor = new Color(0, 0, 0, 0);
        static readonly Color _markerColor = new Color(1, 192 / 255, 203 / 255, 1);
        static readonly int _markerSize = 15;

        public Minimap()
        {
            Instance = this;
        }

        public void Initialize(int width, int depth)
        {
            _mapImg.texture = _mapTexture = new Texture2D(width, depth);
            _mapTexture.wrapMode = TextureWrapMode.Clamp;
            _mapImgSize = _mapImg.rectTransform.sizeDelta;
            _mapImgSizeHalf = _mapImgSize / 2f;

            CreatePositionMarker(_markerSize);
            SetMarkerPosition(Vector2.zero);

            CameraController.OnCameraMoved += args =>
            {
                Vector3 v = args.NewPosition;
                Vector3 cameraOffset = CameraController.Instance.CameraOffset;
                Vector2 markerPos = new Vector2(v.z + cameraOffset.z, v.x + cameraOffset.x);

                SetMarkerPosition(markerPos, false);
            };
        }

        public void SetMapTile(Vector2 pos, int tileId, bool applyTexture = true)
        {
            SetMapTile(pos, HuesLoader.Instance.GetRadarColorData(tileId).ToColor(), applyTexture);
        }

        public void SetMapTile(Vector2 pos, Color color, bool applyTexture = true)
        {
            _mapTexture.SetPixel((int)pos.y, _mapTexture.height - 1 - (int)pos.x, color);

            if (applyTexture)
                _mapTexture.Apply();
        }

        public void SetAllMapTiles(Color[] colors)
        {
            _mapTexture.SetPixels(colors);
            _mapTexture.Apply();
        }

        public void SetMarkerPosition(Vector2 pos, bool signalEvent = true)
        {
            MapToUIPos(&pos);
            Vector2 uiPos = pos;

            pos.x -= _mapImgSizeHalf.x;
            pos.y -= _mapImgSizeHalf.y;
            pos.y *= -1;

            pos.x = Math.Max(-_mapImgSizeHalf.x, Math.Min(pos.x, _mapImgSizeHalf.x));
            pos.y = Math.Max(-_mapImgSizeHalf.y, Math.Min(pos.y, _mapImgSizeHalf.y));

            _markerImg.rectTransform.localPosition = pos;
            UIToMapPos(&uiPos);

            if (signalEvent)
            {
                OnMinimapPositionChange?.Invoke(new Vector3(uiPos.y, 0, uiPos.x));
            }
        }

        public void ApplyMapChanges()
        {
            _mapTexture.Apply();
        }

        void MapToUIPos(Vector2* pos)
        {
            pos->x = pos->x / _mapTexture.width * _mapImgSize.x;
            pos->y = pos->y / _mapTexture.height * _mapImgSize.y;
        }

        void UIToMapPos(Vector2* pos)
        {
            pos->x = pos->x / _mapImgSize.x * _mapTexture.width;
            pos->y = (pos->y / _mapImgSize.y * _mapTexture.height)/* * -1*/;
        }

        void CreatePositionMarker(int size)
        {
            if (_markerTexture == null || _markerTexture.width != size)
            {
                _markerTexture = new Texture2D(size, size);
                _markerImg.texture = _markerTexture;
            }

            int endPos = size - 2;
            int endPos2 = size - 1;

            for (int i = 1; i < size - 1; i++)
            {
                _markerTexture.SetPixel(i, 0, _transparentColor);
                _markerTexture.SetPixel(i, 1, _markerColor);
                _markerTexture.SetPixel(i, endPos, _markerColor);
                _markerTexture.SetPixel(i, endPos2, _transparentColor);

                _markerTexture.SetPixel(0, i, _transparentColor);
                _markerTexture.SetPixel(1, i, _markerColor);
                _markerTexture.SetPixel(endPos, i, _markerColor);
                _markerTexture.SetPixel(endPos2, i, _transparentColor);
            }

            _markerTexture.SetPixel(0, 0, _transparentColor);
            _markerTexture.SetPixel(endPos2, 0, _transparentColor);
            _markerTexture.SetPixel(0, endPos2, _transparentColor);
            _markerTexture.SetPixel(endPos2, endPos2, _transparentColor);

            for (int x = 2; x < size - 2; x++)
            {
                for (int y = 2; y < size - 2; y++)
                {
                    _markerTexture.SetPixel(x, y, _transparentColor);
                }
            }

            _markerTexture.Apply();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnMinimapClick(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnMinimapClick(eventData.position);
        }

        void OnMinimapClick(Vector2 worldPos)
        {
            worldPos.x = (worldPos.x - transform.position.x + _mapImgSize.x / 2) /*- _markerSize * .5f*/;
            worldPos.y = ((worldPos.y - transform.position.y - _mapImgSize.y / 2) * -1) /*- _markerSize * .5f*/;

            UIToMapPos(&worldPos);

            Vector3 cameraOffset = CameraController.Instance.CameraOffset;
            worldPos.x -= cameraOffset.x;
            worldPos.y -= cameraOffset.z;

            SetMarkerPosition(worldPos);
        }
    }
}
