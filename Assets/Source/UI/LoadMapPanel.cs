using Assets.Source.Game.Map;
using Assets.Source.IO;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Source.UI
{
    public sealed class LoadMapPanel : MonoBehaviour
    {
        [SerializeField] InputField _widthInput;
        [SerializeField] InputField _depthInput;

        LoadMapPanel() : base()
        {

        }

        public void SelectAndLoadMap()
        {
            if (!int.TryParse(_widthInput.text, out int w) || !int.TryParse(_depthInput.text, out int d))
                return;

            FileBrowser.OpenFile("Load Map", s =>
            {
                Client.Instance.LoadMap(s, w, d, GenerationOption.Default, null, null);
            }, null);

            gameObject.SetActive(false);
        }
    }
}
