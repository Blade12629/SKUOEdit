using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.SourceTest
{
    public class WorldCamera : MonoBehaviour
    {
        public static event Action<CameraMovedEventArgs> OnCameraMoved;
        public static WorldCamera Instance { get; private set; }

        public Vector3 CameraOffset
        {
            get => _cameraOffset;
            set
            {
                _cameraOffset = value;
                MoveToWorld(_terrainCamera.transform.position);
            }
        }

        [SerializeField] Vector3 _cameraOffset;
        [SerializeField] Camera _terrainCamera;
        [SerializeField] Camera _staticCamera;
        public WorldCamera()
        {
            Instance = this;
        }

        /// <summary>
        /// Sets the current camera position (does not set the height, <see cref="SetCameraHeight(float)"/>)
        /// </summary>
        /// <param name="pos">y coordinate is ignored, <see cref="SetCameraHeight(float)"/></param>
        public void MoveToWorld(Vector3 pos)
        {
            Vector3 oldPos = _terrainCamera.transform.position;
            Vector3 newPos = new Vector3(Mathf.Ceil(pos.x), Mathf.Ceil(pos.y), oldPos.z);
            Vector3 diff = newPos - oldPos;

            _terrainCamera.transform.position = new Vector3(newPos.x + _cameraOffset.x, newPos.y + _cameraOffset.y, _terrainCamera.transform.position.z);
            _staticCamera.transform.position = _terrainCamera.transform.position;
            //_positionText.text = $"X: {(int)_camera.transform.position.z}\nY: {(int)_camera.transform.position.x}";

            OnCameraMoved?.Invoke(new CameraMovedEventArgs(oldPos, newPos, diff));
        }

        void Update()
        {
            Vector3 dir = Vector3.zero;
            float shiftScale = 1f;

            if (Input.GetKey(KeyCode.W))
            {
                dir += _terrainCamera.transform.up;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir -= _terrainCamera.transform.up;
            }

            if (Input.GetKey(KeyCode.A))
            {
                dir -= _terrainCamera.transform.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir += _terrainCamera.transform.right;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                shiftScale = 2f;
            }

            if (!dir.Equals(Vector3.zero))
            {
                Vector3 pos = _terrainCamera.transform.position + dir * shiftScale;
                pos.x -= _cameraOffset.x;
                pos.y -= _cameraOffset.y;

                MoveToWorld(pos);
            }

            float mouseScroll = Input.mouseScrollDelta.y;

            if (mouseScroll != 0f && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                if (mouseScroll < 0)
                    mouseScroll = 1f;
                else
                    mouseScroll = -1f;

                Vector3 pos = _terrainCamera.transform.position;
                pos.y += mouseScroll * shiftScale;

                _terrainCamera.transform.position = pos;
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
