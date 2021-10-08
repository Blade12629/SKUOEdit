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
//    public class DragDropTransform : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//    {
//        public bool DisallowDragging { get => _disallowDragging; set => _disallowDragging = value; }

//        [SerializeField] Transform _parentToMove;
//        [SerializeField] bool _disallowDragging;
        
//        bool _isMouseInside;
//        Vector3 _lastMousePos;

//        public void OnPointerEnter(PointerEventData eventData)
//        {
//            _isMouseInside = true;
//        }

//        public void OnPointerExit(PointerEventData eventData)
//        {
//            _isMouseInside = false;
//        }

//        void Update()
//        {
//            if (!DisallowDragging && _isMouseInside)
//            {
//                if (Input.GetMouseButtonDown(0))
//                {
//                    _lastMousePos = Input.mousePosition;
//                }
//                else if (Input.GetMouseButton(0))
//                {
//                    Vector3 curMousePos = Input.mousePosition;

//                    if (!_lastMousePos.Equals(curMousePos))
//                    {
//                        Vector3 diff = curMousePos - _lastMousePos;
//                        _parentToMove.position += diff;
//                        _lastMousePos = curMousePos;
//                    }
//                }
//            }
//        }
//    }
//}
