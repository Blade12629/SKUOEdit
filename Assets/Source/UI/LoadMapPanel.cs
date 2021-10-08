using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI
{
    public class LoadMapPanel : MonoBehaviour
    {
        [SerializeField] InputField _widthInput;
        [SerializeField] InputField _depthInput;

        public void SelectAndLoadMap()
        {
            if (!int.TryParse(_widthInput.text, out int w) || !int.TryParse(_depthInput.text, out int d))
                return;

            FileBrowser.OpenFile("Load Map", s =>
            {
                Client.Instance.LoadMap(s, w, d, Game.GameMap.GenerationOption.Default, null, null);
            }, null);

            gameObject.SetActive(false);
        }
    }
}
