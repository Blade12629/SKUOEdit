using Assets.Source.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.Game
{
    public class CameraController : MonoBehaviour
    {
        public static event Action<CameraMovedEventArgs> OnCameraMoved;
        public static CameraController Instance { get; private set; }

        public Vector3 CameraOffset
        {
            get => _cameraOffset;
            set
            {
                _cameraOffset = value;
                MoveToWorld(transform.position);
            }
        }

        [SerializeField] Text _positionText;
        [SerializeField] Vector3 _cameraOffset;

        int _terrainLayerMask;

        public CameraController()
        {
            Instance = this;
            _terrainLayerMask = 1 << 0;
        }

        public void InitializePosition(Vector3? startPos = null)
        {
            Vector3 pos;

            if (startPos.HasValue)
                pos = startPos.Value;
            else
                pos = Vector3.zero;

            transform.position = new Vector3(pos.x + _cameraOffset.x, transform.position.y, pos.z + _cameraOffset.z);

            Minimap.OnMinimapPositionChange += MoveToWorld;
        }

        /// <summary>
        /// Sets the current camera position (does not set the height, <see cref="SetCameraHeight(float)"/>)
        /// </summary>
        /// <param name="pos">y coordinate is ignored, <see cref="SetCameraHeight(float)"/></param>
        public void MoveToWorld(Vector3 pos)
        {
            Vector3 oldPos = transform.position;
            Vector3 newPos = new Vector3(pos.x, oldPos.y, pos.z);
            Vector3 diff = newPos - oldPos;

            transform.position = new Vector3(newPos.x + _cameraOffset.x, transform.position.y, newPos.z + _cameraOffset.z);
            _positionText.text = $"X: {(int)transform.position.z}\nY: {(int)transform.position.x}";

            OnCameraMoved?.Invoke(new CameraMovedEventArgs(oldPos, newPos, diff));
        }

        /// <summary>
        /// Sets the current camera height
        /// </summary>
        /// <param name="height"></param>
        public void SetCameraHeight(float height)
        {
            Vector3 oldPos = transform.position;
            oldPos.y = height;

            transform.position = oldPos;
        }

        void Update()
        {
            if (GameMap.Instance == null)
                return;

            Vector3 dir = Vector3.zero;
            float shiftScale = 1f;

            if (Input.GetKey(KeyCode.W))
            {
                dir += transform.forward;
                //dir.x--;
                //dir.z--;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir -= transform.forward;
                //dir.x++;
                //dir.z++;
            }

            if (Input.GetKey(KeyCode.A))
            {
                dir -= transform.right;
                //dir.x++;
                //dir.z--;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir += transform.right;
                //dir.x--;
                //dir.z++;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                shiftScale = 2f;
            }

            if (!dir.Equals(Vector3.zero))
            {
                Vector3 pos = transform.position + dir * shiftScale;
                pos.x -= _cameraOffset.x;
                pos.z -= _cameraOffset.z;

                MoveToWorld(pos);
            }

            float mouseScroll = Input.mouseScrollDelta.y;

            if (mouseScroll != 0f && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                if (mouseScroll < 0)
                    mouseScroll = 1f;
                else
                    mouseScroll = -1f;

                Vector3 pos = transform.position;
                pos.y += mouseScroll * shiftScale;

                transform.position = pos;
            }

            if (Input.GetMouseButton(1) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
                transform.Rotate(-Input.GetAxis("Mouse Y"), 0, 0);

                Vector3 eulerAngles = transform.eulerAngles;
                eulerAngles.z = 0;

                transform.eulerAngles = eulerAngles;
                //transform.Rotate(0, 0, -Input.GetAxis("QandE") * 90 * Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100000f, _terrainLayerMask))
            {
                SelectionRenderer.Instance.SetPosition(hit.point);
            }
        }
    }

    public class CameraMovedEventArgs : EventArgs
    {
        public Vector3 OldPosition { get; }
        public Vector3 NewPosition { get; }
        public Vector3 Diffrence { get; }

        public CameraMovedEventArgs(Vector3 oldPos, Vector3 newPos, Vector3 diff) : base()
        {
            OldPosition = oldPos;
            NewPosition = newPos;
            Diffrence = diff;
        }
    }
}
