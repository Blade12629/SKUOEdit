using Assets.Source.Game.Map;
using Assets.Source.UI;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Source.Game
{
    public sealed class CameraController : MonoBehaviour
    {
        public static event Action<CameraMovedEventArgs> OnCameraMoved;
        public static CameraController Instance { get; private set; }

        public Vector3 CameraOffset
        {
            get => _cameraOffset;
            set
            {
                _cameraOffset = value;
                MoveToWorld(_camera.transform.position);
            }
        }

        [SerializeField] Text _positionText;
        [SerializeField] Vector3 _cameraOffset;
        [SerializeField] Camera _camera;

        // TODO: allow switching between perspective (3d) and ortographic (2d)
        /*
         * Perspective:
         *      Rot: 60, 235, 0
         *      
         *      
         * Ortographic:
         *      Rot: 38, 225, 0
         *      Size == view height -> 10-15 is a good value
         */

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

            _camera.transform.position = new Vector3(pos.x + _cameraOffset.x, _camera.transform.transform.position.y, pos.z + _cameraOffset.z);

            Minimap.OnMinimapPositionChange += MoveToWorld;
        }

        /// <summary>
        /// Sets the current camera position (does not set the height, <see cref="SetCameraHeight(float)"/>)
        /// </summary>
        /// <param name="pos">y coordinate is ignored, <see cref="SetCameraHeight(float)"/></param>
        public void MoveToWorld(Vector3 pos)
        {
            Vector3 oldPos = _camera.transform.position;
            Vector3 newPos = new Vector3(pos.x, oldPos.y, pos.z);
            Vector3 diff = newPos - oldPos;

            _camera.transform.position = new Vector3(newPos.x + _cameraOffset.x, _camera.transform.position.y, newPos.z + _cameraOffset.z);
            _positionText.text = $"X: {(int)_camera.transform.position.z}\nY: {(int)_camera.transform.position.x}";

            OnCameraMoved?.Invoke(new CameraMovedEventArgs(oldPos, newPos, diff));
        }

        /// <summary>
        /// Sets the current camera height
        /// </summary>
        /// <param name="height"></param>
        public void SetCameraHeight(float height)
        {
            Vector3 oldPos = _camera.transform.position;
            oldPos.y = height;

            _camera.transform.position = oldPos;
        }

        void Update()
        {
            if (GameMap.Instance == null)
                return;

            Vector3 dir = Vector3.zero;
            float shiftScale = 1f;

            if (Input.GetKey(KeyCode.W))
            {
                dir += _camera.transform.forward;
                //dir.x--;
                //dir.z--;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir -= _camera.transform.forward;
                //dir.x++;
                //dir.z++;
            }

            if (Input.GetKey(KeyCode.A))
            {
                dir -= _camera.transform.right;
                //dir.x++;
                //dir.z--;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir += _camera.transform.right;
                //dir.x--;
                //dir.z++;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                shiftScale = 2f;
            }

            if (!dir.Equals(Vector3.zero))
            {
                Vector3 pos = _camera.transform.position + dir * shiftScale;
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

                Vector3 pos = _camera.transform.position;
                pos.y += mouseScroll * shiftScale;

                _camera.transform.position = pos;
            }

            if (Input.GetMouseButton(1) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                _camera.transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
                _camera.transform.Rotate(-Input.GetAxis("Mouse Y"), 0, 0);

                Vector3 eulerAngles = _camera.transform.eulerAngles;
                eulerAngles.z = 0;

                _camera.transform.eulerAngles = eulerAngles;
                //transform.Rotate(0, 0, -Input.GetAxis("QandE") * 90 * Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100000f))
            {
                GameMap.Instance.SetSelectedTile((int)hit.point.x, (int)hit.point.z);

                if (hit.collider.gameObject.GetComponent<MapChunk>() != null)
                {
                    //GameMap.Instance.UnsetSelectedStatic();
                    GameMap.Instance.EnableSelectedGrid();
                    GameMap.Instance.SetSelectedTile((int)hit.point.x, (int)hit.point.z);
                }
                else
                {
                    GameMap.Instance.DisableSelectedGrid();
                    //GameMap.Instance.SetSelectedStatic(hit.collider.gameObject);
                }
                //if (hit.collider.gameObject.GetComponent<MapChunk>() != null)
                //    SelectionRenderer.Instance.SetPosition(hit.point);
                //else
                //    SelectionRenderer.Instance.SetStatic(hit.collider.gameObject);
            }
        }
    }

    public class CameraMovedEventArgs : EventArgs
    {
        public Vector3 OldPosition { get; }
        public Vector3 NewPosition { get; }
        public Vector3 Diffrence { get; }

        public CameraMovedEventArgs(Vector3 oldPos, Vector3 newPos, Vector3 diff)
        {
            OldPosition = oldPos;
            NewPosition = newPos;
            Diffrence = diff;
        }
    }
}
