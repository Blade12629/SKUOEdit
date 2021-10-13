using Assets.Source.Game;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI
{
    public sealed class MappingTools : MonoBehaviour
    {
        public static MappingTools Instance { get; private set; }

        [SerializeField] Outline _vertUpLine;
        [SerializeField] Outline _vertDownLine;
        [SerializeField] Outline _vertLine;

        [SerializeField] Outline _tileUpLine;
        [SerializeField] Outline _tileDownLine;
        [SerializeField] Outline _tileLine;

        [SerializeField] Outline _tileIdLine;

        [SerializeField] InputField _heightValueInput;
        [SerializeField] InputField _tileIdInput;
        [SerializeField] TileBrowser _tileBrowser;
        [SerializeField] InputField _selectionAreaSize;

        MappingTools() : base()
        {
            Instance = this;
        }

        public void OnTileIdChanged(string newValue)
        {
            if (!int.TryParse(_tileIdInput.text, out int tileId))
                return;

            _tileBrowser.OnTileClick((short)tileId, true);
        }

        public void OnSelectionAreaSizeChanged(string newValue)
        {
            if (!int.TryParse(_selectionAreaSize.text, out int selectionAreaSize))
                return;

            selectionAreaSize = Math.Max(1, Math.Min(selectionAreaSize, SelectionRenderer.MaxAreaSize));

            EditorInput.CurrentSize = selectionAreaSize;
            SelectionRenderer.Instance.AreaSize = selectionAreaSize;
            _selectionAreaSize.SetTextWithoutNotify(selectionAreaSize.ToString());
        }

        public void OnHeightValueChanged(string newValue)
        {
            if (!int.TryParse(_heightValueInput.text, out int h))
                return;

            EditorInput.CurrentHeightValue = h;
        }

        public void SetTileIdInput(short tileid)
        {
            _tileIdInput.SetTextWithoutNotify(tileid.ToString());
        }

        public void SetHeightValueInput(int height)
        {
            _heightValueInput.SetTextWithoutNotify(height.ToString());
        }

        public void SetSelectionSizeValue(int size)
        {
            _selectionAreaSize.SetTextWithoutNotify(size.ToString());
        }

        public void SetToolVertUp()
        {
            EditorInput.CurrentAction = EditorAction.IncreaseVerticeHeight;
            DisableOutlines();
            _vertUpLine.enabled = true;
        }

        public void SetToolVertDown()
        {
            EditorInput.CurrentAction = EditorAction.DecreaseVerticeHeight;
            DisableOutlines();
            _vertDownLine.enabled = true;
        }

        public void SetToolVert()
        {
            EditorInput.CurrentAction = EditorAction.SetVerticeHeight;
            DisableOutlines();
            _vertLine.enabled = true;
        }

        public void SetToolTileUp()
        {
            EditorInput.CurrentAction = EditorAction.IncreaseTileHeight;
            DisableOutlines();
            _tileUpLine.enabled = true;
        }

        public void SetToolTileDown()
        {
            EditorInput.CurrentAction = EditorAction.DecreaseTileHeight;
            DisableOutlines();
            _tileDownLine.enabled = true;
        }

        public void SetToolTile()
        {
            EditorInput.CurrentAction = EditorAction.SetTileHeight;
            DisableOutlines();
            _tileLine.enabled = true;
        }

        public void SetToolTileId()
        {
            EditorInput.CurrentAction = EditorAction.SetTileId;
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
