using Assets.Source.Game;
using Assets.Source.Game.Map;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Source.UI
{
    public sealed class TileBrowser : MonoBehaviour
    {
        public static TileBrowser Instance { get; private set; }

        Dictionary<short, RawImage> _tiles;

        [SerializeField] GameObject _contentObj;
        [SerializeField] GameObject _selectionCell;

        [SerializeField] ScrollRect _scrollRect;
        [SerializeField] RectTransform _scrollTransform;
        [SerializeField] RectTransform _contentTransform;

        TileBrowser() : base()
        {
            Instance = this;
            _tiles = new Dictionary<short, RawImage>();

            GameMap.OnMapFinishLoading += SelectDefault;
        }

        public void AddTile(short tileId)
        {
            Texture2D imgTex = ClassicUO.IO.Resources.ArtLoader.Instance.GetLandTexture((uint)tileId);

            if (imgTex == null)
            {
                _tiles.Add(tileId, null);
                return;
            }

            GameObject imgObj = new GameObject($"Tile {tileId}", typeof(Button), typeof(RawImage));
            imgObj.transform.SetParent(_contentObj.transform);
            imgObj.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            imgObj.GetComponent<Button>().onClick.AddListener(() => OnTileClick(tileId));

            RawImage img = imgObj.GetComponent<RawImage>();
            img.texture = imgTex;

            _tiles.Add(tileId, img);
        }

        public void SelectDefault()
        {
            if (_tiles.TryGetValue(0, out RawImage img))
                OnTileClick(0);
            else
                OnTileClick(1);
        }

        public void OnTileClick(short id, bool snapTo = false)
        {
            if (_tiles.TryGetValue(id, out RawImage newImg))
            {
                if (snapTo)
                    SnapTo(newImg.rectTransform);

                MoveSelectionCell(newImg);
            }

            EditorInput.CurrentTileId = id;

            if (MappingTools.Instance != null)
                MappingTools.Instance.SetTileIdInput(id);

        }

        public void LoadTiles()
        {
            for (short i = 0; i < ClassicUO.Game.Constants.MAX_LAND_DATA_INDEX_COUNT; i++)
            {
                AddTile(i);
            }
        }

        void DisableSelectionCell()
        {
            if (_selectionCell.activeInHierarchy)
                _selectionCell.SetActive(false);
        }

        void MoveSelectionCell(RawImage image)
        {
            if (image == null)
            {
                DisableSelectionCell();
                return;
            }

            _selectionCell.transform.position = image.transform.position;
        }

        void SnapTo(RectTransform target)
        {
            // Item is here
            var itemCenterPositionInScroll = GetWorldPointInWidget(_scrollTransform, GetWidgetWorldPoint(target));
            // But must be here
            var targetPositionInScroll = GetWorldPointInWidget(_scrollTransform, GetWidgetWorldPoint(GetComponentInChildren<Mask>(true).rectTransform));
            // So it has to move this distance
            var difference = targetPositionInScroll - itemCenterPositionInScroll;
            difference.z = 0f;
            //clear axis data that is not enabled in the scrollrect
            if (!_scrollRect.horizontal)
            {
                difference.x = 0f;
            }
            if (!_scrollRect.vertical)
            {
                difference.y = 0f;
            }

            var normalizedDifference = new Vector2(
                difference.x / (_contentTransform.rect.size.x - _scrollTransform.rect.size.x),
                difference.y / (_contentTransform.rect.size.y - _scrollTransform.rect.size.y));

            var newNormalizedPosition = _scrollRect.normalizedPosition - normalizedDifference;
            if (_scrollRect.movementType != ScrollRect.MovementType.Unrestricted)
            {
                newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
                newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
            }

            _scrollRect.normalizedPosition = newNormalizedPosition;
        }

        Vector3 GetWidgetWorldPoint(RectTransform target)
        {
            //pivot position + item size has to be included
            var pivotOffset = new Vector3(
                (0.5f - target.pivot.x) * target.rect.size.x,
                (0.5f - target.pivot.y) * target.rect.size.y,
                0f);
            var localPosition = target.localPosition + pivotOffset;
            return target.parent.TransformPoint(localPosition);
        }

        Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
        {
            return target.InverseTransformPoint(worldPoint);
        }
    }
}
