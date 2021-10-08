//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//namespace Assets.Source.GameEditor.UI
//{
//    public class MinimapUI : MonoBehaviour, IPointerClickHandler, IDragHandler
//    {
//        Vector3 _mapTopLeft;
//        float _mapHeight;

//        void Awake()
//        {
//            RectTransform rect = GetComponent<RectTransform>();
//            _mapTopLeft = new Vector3(rect.position.x - rect.sizeDelta.x / 2, rect.position.y - rect.sizeDelta.y / 2);
//            _mapHeight = rect.sizeDelta.y;
//        }

//        public void OnPointerClick(PointerEventData eventData)
//        {
//            SetMapPosition(eventData);
//        }

//        public void OnDrag(PointerEventData eventData)
//        {
//            SetMapPosition(eventData);
//        }

//        void SetMapPosition(PointerEventData eventData)
//        {
//            Vector3 mapPos = new Vector3(eventData.position.x - _mapTopLeft.x, 0,
//                                         _mapHeight - (eventData.position.y - _mapTopLeft.y));

//            EditorSceneController.Instance.MapController.Minimap.OnMapClick(mapPos);
//        }
//    }
//}
