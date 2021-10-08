using Assets.Source.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI
{
    public class MappingTools : MonoBehaviour
    {
        [SerializeField] Outline _vertUpLine;
        [SerializeField] Outline _vertDownLine;
        [SerializeField] Outline _vertLine;

        [SerializeField] Outline _tileUpLine;
        [SerializeField] Outline _tileDownLine;
        [SerializeField] Outline _tileLine;

        [SerializeField] Outline _tileIdLine;


        public void SetToolVertUp()
        {
            EditorInput.Instance.CurrentAction = EditorAction.IncreaseVerticeHeight;
            DisableOutlines();
            _vertUpLine.gameObject.SetActive(false);
        }

        public void SetToolVertDown()
        {
            EditorInput.Instance.CurrentAction = EditorAction.DecreaseVerticeHeight;
            DisableOutlines();
            _vertDownLine.gameObject.SetActive(false);
        }

        public void SetToolVert()
        {
            EditorInput.Instance.CurrentAction = EditorAction.SetVerticeHeight;
            DisableOutlines();
            _vertLine.gameObject.SetActive(false);
        }

        public void SetToolTileUp()
        {
            EditorInput.Instance.CurrentAction = EditorAction.IncreaseTileHeight;
            DisableOutlines();
            _tileUpLine.gameObject.SetActive(false);
        }

        public void SetToolTileDown()
        {
            EditorInput.Instance.CurrentAction = EditorAction.DecreaseTileHeight;
            DisableOutlines();
            _tileDownLine.gameObject.SetActive(false);
        }

        public void SetToolTile()
        {
            EditorInput.Instance.CurrentAction = EditorAction.SetTileHeight;
            DisableOutlines();
            _tileLine.gameObject.SetActive(false);
        }

        public void SetToolTileId()
        {
            EditorInput.Instance.CurrentAction = EditorAction.SetTileId;
            DisableOutlines();
            _tileIdLine.gameObject.SetActive(false);
        }

        void DisableOutlines()
        {
            _vertUpLine.gameObject.SetActive(false);
            _vertDownLine.gameObject.SetActive(false);
            _vertLine.gameObject.SetActive(false);

            _tileUpLine.gameObject.SetActive(false);
            _tileDownLine.gameObject.SetActive(false);
            _tileLine.gameObject.SetActive(false);

            _tileIdLine.gameObject.SetActive(false);
        }
    }
}
