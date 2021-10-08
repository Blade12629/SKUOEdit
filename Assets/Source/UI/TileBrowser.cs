using Assets.Source.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI
{
    public class TileBrowser : MonoBehaviour
    {
        public static TileBrowser Instance { get; private set; }

        Dictionary<short, RawImage> _tiles;

        [SerializeField] GameObject _contentObj;
        [SerializeField] GameObject _selectionCell;

        [SerializeField] bool _testClick;

        public TileBrowser()
        {
            Instance = this;
            _tiles = new Dictionary<short, RawImage>();
        }

        void Update()
        {
            if (_testClick)
            {
                _testClick = false;

                RawImage img = _tiles[3];
                Button button = img.GetComponent<Button>();
                button.onClick.Invoke();
            }
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

            AddButtonClick(imgObj, tileId);

            RawImage img = imgObj.GetComponent<RawImage>();
            img.texture = imgTex;

            _tiles.Add(tileId, img);
        }

        void AddButtonClick(GameObject obj, short tileId)
        {
            Button button = obj.GetComponent<Button>();
            button.onClick.AddListener(() => OnTileClick(tileId));
        }

        public void SelectDefault()
        {
            if (_tiles.TryGetValue(0, out RawImage img))
                OnTileClick(0);
            else
                OnTileClick(1);
        }

        public void OnTileClick(short id)
        {
            if (_tiles.TryGetValue(id, out RawImage newImg))
                MoveSelectionCell(newImg);

            EditorInput.Instance.CurrentTileId = id;
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
    }
}
