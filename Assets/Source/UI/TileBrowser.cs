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
        [SerializeField] Outline _outlinePrefab;

        public TileBrowser()
        {
            Instance = this;
            _tiles = new Dictionary<short, RawImage>();
        }

        public void AddTile(short tileId)
        {
            GameObject imgObj = new GameObject($"Tile {tileId}", typeof(Button), typeof(RawImage));
            RawImage img = imgObj.GetComponent<RawImage>();
            Button button = imgObj.GetComponent<Button>();

            Outline outline = imgObj.AddComponent(_outlinePrefab);

            button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => OnTileClick(tileId)));
            imgObj.transform.SetParent(_contentObj.transform);

            if (_tiles.Count != 0)
                outline.enabled = false;

            _tiles.Add(tileId, img);
        }

        public void OnTileClick(short id)
        {
            _tiles[(short)EditorInput.Instance.CurrentTileId].GetComponent<Outline>().enabled = false;
            _tiles[id].GetComponent<Outline>().enabled = false;

            EditorInput.Instance.CurrentTileId = id;
        }
    }
}
