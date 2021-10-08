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
            _vertUpLine.enabled = true;
        }

        public void SetToolVertDown()
        {
            EditorInput.Instance.CurrentAction = EditorAction.DecreaseVerticeHeight;
            DisableOutlines();
            _vertDownLine.enabled = true;
        }

        public void SetToolVert()
        {
            EditorInput.Instance.CurrentAction = EditorAction.SetVerticeHeight;
            DisableOutlines();
            _vertLine.enabled = true;
        }

        public void SetToolTileUp()
        {
            EditorInput.Instance.CurrentAction = EditorAction.IncreaseTileHeight;
            DisableOutlines();
            _tileUpLine.enabled = true;
        }

        public void SetToolTileDown()
        {
            EditorInput.Instance.CurrentAction = EditorAction.DecreaseTileHeight;
            DisableOutlines();
            _tileDownLine.enabled = true;
        }

        public void SetToolTile()
        {
            EditorInput.Instance.CurrentAction = EditorAction.SetTileHeight;
            DisableOutlines();
            _tileLine.enabled = true;
        }

        public void SetToolTileId()
        {
            EditorInput.Instance.CurrentAction = EditorAction.SetTileId;
            DisableOutlines();
            _tileIdLine.enabled = true;
        }

        void DisableOutlines()
        {
            _vertUpLine.enabled = false;
            _vertDownLine.enabled = false;
            _vertLine.enabled = false;

            _tileUpLine.enabled = false;
            _tileDownLine.enabled = false;
            _tileLine.enabled = false;

            _tileIdLine.enabled = false;
        }
    }
}
