using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI
{
    public class GenerateFlatlandMapPanel : MonoBehaviour
    {
        [SerializeField] InputField _widthInput;
        [SerializeField] InputField _depthInput;

        public void GenerateFlatlandMap()
        {
            if (!int.TryParse(_widthInput.text, out int w) || !int.TryParse(_depthInput.text, out int d))
                return;

            gameObject.SetActive(false);
            Client.Instance.LoadMap(null, w, d, Game.GameMap.GenerationOption.Flatland, null, null);
        }
    }
}
