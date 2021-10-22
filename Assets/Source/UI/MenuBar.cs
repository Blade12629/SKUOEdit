using Assets.Source.Game.Map;
using Assets.Source.IO;
using UnityEngine;


namespace Assets.Source.UI
{
    public sealed class MenuBar : MonoBehaviour
    {
        [SerializeField] GameObject _settingsPanel;
        [SerializeField] LoadMapPanel _loadMapPanel;
        [SerializeField] GenerateFlatlandMapPanel _genMapPanel;

        [SerializeField] GameObject _settingsBtn;
        [SerializeField] GameObject _saveMapBtn;

        MenuBar() : base()
        {
            GameMap.OnMapDestroyed += () =>
            {
                _settingsBtn.SetActive(false);
                _saveMapBtn.SetActive(false);
            };

            GameMap.OnMapFinishLoading += () =>
            {
                _settingsBtn.SetActive(true);
                _saveMapBtn.SetActive(true);
            };
        }

        public void ToggleVisSettingsPanel()
        {
            _settingsPanel.SetActive(!_settingsPanel.activeInHierarchy);
        }

        public void LoadMap()
        {
            _loadMapPanel.gameObject.SetActive(!_loadMapPanel.gameObject.activeInHierarchy);
        }

        public void GenerateMap()
        {
            _genMapPanel.gameObject.SetActive(!_genMapPanel.gameObject.activeInHierarchy);
        }

        public void SaveMap()
        {
            FileBrowser.SaveFile("Save Map", s =>
            {
                Client.Instance.SaveMap(s);
            }, null);
        }

        public void ConvertMap()
        {
            FileBrowser.OpenFile("Select Heightmap", s =>
            {
                // Select heightmap

                FileBrowser.OpenFile("Select Tilemap", s2 =>
                {
                    // Select tilemap
                    ConvertMap(s, s2);
                }, () =>
                {
                    // Cancel tilemap
                    ConvertMap(s, null);
                });
            }, () =>
            {
                // Cancel heightmap
                FileBrowser.OpenFile("Select Tilemap", s =>
                {
                    // Select tilemap
                    ConvertMap(null, s);
                }, null);
            });
        }

        void ConvertMap(string heightMap, string tileMap)
        {
            if (heightMap == null && tileMap == null)
                return;

            int width = 0;
            int depth = 0;

            int[] heights = ConvertImage(heightMap);
            int[] tiles = ConvertImage(tileMap);

            Client.Instance.LoadMap(null, width, depth, GenerationOption.Converted, heights, tiles);

            int[] ConvertImage(string path)
            {
                if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                    return null;

                ColorStore cs = new ColorStore(0);
                cs.Load($"{path}.cs");

                using (System.Drawing.Bitmap mapImg = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(heightMap))
                {
                    width = mapImg.Width;
                    depth = mapImg.Height;

                    int[] result = new int[mapImg.Width * mapImg.Height];
                    int index = 0;

                    for (int x = 0; x < mapImg.Width; x++)
                    {
                        for (int z = 0; z < mapImg.Height; z++)
                        {
                            result[index++] = cs[mapImg.GetPixel(x, z)];
                        }
                    }

                    return result;
                }
            }
        }

        public void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
