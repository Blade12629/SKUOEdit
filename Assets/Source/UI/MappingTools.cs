using Assets.Source.Game;
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

        public void OnHeightValueChanged(string newValue)
        {
            if (!int.TryParse(_heightValueInput.text, out int h))
                return;

            EditorInput.Instance.CurrentValue = h;
        }

        public void SetTileIdInput(short tileid)
        {
            _tileIdInput.SetTextWithoutNotify(tileid.ToString());
        }

        public void SetHeightValueInput(int height)
        {
            _heightValueInput.SetTextWithoutNotify(height.ToString());
        }

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
