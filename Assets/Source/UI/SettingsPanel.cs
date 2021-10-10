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
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] InputField _renderWidthInput;
        [SerializeField] InputField _renderDepthInput;
        [SerializeField] InputField _gridSizeInput;

        [SerializeField] InputField _gridColorRInput;
        [SerializeField] InputField _gridColorGInput;
        [SerializeField] InputField _gridColorBInput;

        public SettingsPanel() : base()
        {
            GameMap.OnMapFinishLoading += () =>
            {
                SetGridColorValue(GameMap.Instance.GetGridColor());
                SetGridSizeValue(GameMap.Instance.GetGridSize());
                SetRenderSizesValues(MapChunk.MeshSize * 3, MapChunk.MeshSize * 3);
            };
        }

        public void ApplyRenderSizes()
        {

        }

        public void ToggleMapGrid()
        {
            bool status = GameMap.Instance.IsGridEnabled();

            if (status)
                GameMap.Instance.DisableGrid();
            else
                GameMap.Instance.EnableGrid();
        }

        public void OnGridSizeChanged(string _)
        {
            if (float.TryParse(_gridSizeInput.text, out float gridSize))
                GameMap.Instance.SetGridSize(gridSize);
        }

        public void OnGridColorChanged()
        {
            Color oldColor = GameMap.Instance.GetGridColor();

            if (byte.TryParse(_gridColorRInput.text, out byte r))
                oldColor.r = r;
            if (byte.TryParse(_gridColorGInput.text, out byte g))
                oldColor.g = g;
            if (byte.TryParse(_gridColorBInput.text, out byte b))
                oldColor.b = b;

            GameMap.Instance.SetGridColor(oldColor);
        }

        public void SetGridColorValue(Color c)
        {
            _gridColorRInput.SetTextWithoutNotify(c.r.ToString());
            _gridColorGInput.SetTextWithoutNotify(c.g.ToString());
            _gridColorBInput.SetTextWithoutNotify(c.b.ToString());
        }

        public void SetGridSizeValue(float v)
        {
            _gridSizeInput.SetTextWithoutNotify(v.ToString());
        }

        public void SetRenderSizesValues(int? width, int? depth)
        {
            if (width.HasValue)
                _renderWidthInput.SetTextWithoutNotify(width.ToString());
            if (depth.HasValue)
                _renderDepthInput.SetTextWithoutNotify(depth.ToString());
        }
    }
}
