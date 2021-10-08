//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Assets.Source.GameEditor.UI
//{
//    public class MenuBar : MonoBehaviour
//    {
//        [SerializeField] GameObject _settingsPanel;

//        public void ToggleVisSettingsPanel()
//        {
//            _settingsPanel.SetActive(!_settingsPanel.activeInHierarchy);
//        }

//        public void LoadMap()
//        {

//        }

//        public void SaveMap()
//        {

//        }

//        public void GenerateMap()
//        {

//        }

//        public void Exit()
//        {
//#if UNITY_EDITOR
//            UnityEditor.EditorApplication.isPlaying = false;
//#else
//            Application.Quit();
//#endif
//        }
//    }
//}
