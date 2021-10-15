using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Source.StaticsBuilder
{
    public class TexImage : MonoBehaviour, IPointerClickHandler, IDragHandler
    {
        Texture2D _texture => (Texture2D)_rawImg.texture;
        
        [SerializeField] RawImage _rawImg;
        [SerializeField] StaticBuilder _builder;
        [SerializeField] RawImage _markerImg;

        Texture2D _markerTexture;
        Vector2 _texImgSize;
        Vector2 _texImgSizeHalf;

        readonly Color _transparentColor = new Color(0, 0, 0, 0);
        readonly Color _markerColor = new Color(1, 0, 0, 1);

        void Start()
        {
            _texImgSize = ((RectTransform)transform).sizeDelta;
            _texImgSizeHalf = _texImgSize / 2f;
            CreatePositionMarker(11);

            _markerImg.rectTransform.sizeDelta = new Vector2(_markerTexture.width, _markerTexture.height);
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveUVs(eventData.position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            MoveUVs(eventData.position);
        }

        unsafe void MoveUVs(Vector2 worldPos)
        {
            worldPos.x = (worldPos.x - transform.position.x + _texImgSize.x / 2);
            worldPos.y = ((worldPos.y - transform.position.y - _texImgSize.y / 2) * -1);

            worldPos.x = Mathf.Max(0, Mathf.Min(worldPos.x, _texImgSize.x));
            worldPos.y = Mathf.Max(0, Mathf.Min(worldPos.y, _texImgSize.y));

            //UIToTexPos(&worldPos);

            _builder.OnImageClick(worldPos);
        }

        public unsafe void SetMarkerPosition(Vector2 pos, bool convert = false)
        {
            if (convert)
                TexToUIPos(&pos);
            Vector2 uiPos = pos;

            pos.x -= _texImgSizeHalf.x;
            pos.y -= _texImgSizeHalf.y;
            pos.y *= -1;

            pos.x = Math.Max(-_texImgSizeHalf.x, Math.Min(pos.x, _texImgSizeHalf.x));
            pos.y = Math.Max(-_texImgSizeHalf.y, Math.Min(pos.y, _texImgSizeHalf.y));

            _markerImg.rectTransform.localPosition = pos;
            UIToTexPos(&uiPos);
        }

        unsafe void UIToTexPos(Vector2* pos)
        {
            pos->x = pos->x / _texImgSize.x * _texture.width;
            pos->y = (pos->y / _texImgSize.y * _texture.height);
        }

        unsafe void TexToUIPos(Vector2* pos)
        {
            pos->x = pos->x / _texture.width * _texImgSize.x;
            pos->y = pos->y / _texture.height * _texImgSize.y;
        }

        void CreatePositionMarker(int size)
        {
            if (_markerTexture == null || _markerTexture.width != size)
            {
                _markerTexture = new Texture2D(size, size);
                _markerImg.texture = _markerTexture;
            }

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    _markerTexture.SetPixel(x, y, _transparentColor);

            int half = size / 2;

            for (int x = 0; x < size; x++)
            {
                _markerTexture.SetPixel(x, half, _markerColor);
            }

            for (int y = 0; y < size; y++)
            {
                _markerTexture.SetPixel(half, y, _markerColor);
            }

            _markerTexture.Apply();
        }
    }
}
