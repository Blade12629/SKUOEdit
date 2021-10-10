using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Source.UI
{
    public sealed class DragDropTransform : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool DisallowDragging { get => _disallowDragging; set => _disallowDragging = value; }

        [SerializeField] Transform _parentToMove;
        [SerializeField] bool _disallowDragging;

        bool _isMouseInside;
        Vector3 _lastMousePos;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isMouseInside = false;
        }

        void Update()
        {
            if (!DisallowDragging && _isMouseInside)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _lastMousePos = Input.mousePosition;
                }
                else if (Input.GetMouseButton(0))
                {
                    Vector3 curMousePos = Input.mousePosition;

                    if (!_lastMousePos.Equals(curMousePos))
                    {
                        Vector3 diff = curMousePos - _lastMousePos;
                        _parentToMove.position += diff;
                        _lastMousePos = curMousePos;
                    }
                }
            }
        }
    }
}
