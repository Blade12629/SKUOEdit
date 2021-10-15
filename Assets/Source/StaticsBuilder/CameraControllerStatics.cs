using Assets.Source.Game.Map;
using Assets.Source.UI;
using System;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Source.StaticsBuilder
{
    public sealed class CameraControllerStatics : MonoBehaviour
    {
        [SerializeField] Camera _camera;

        void Update()
        {
            Vector3 dir = Vector3.zero;
            float shiftScale = 1f;

            if (Input.GetKey(KeyCode.W))
            {
                dir += _camera.transform.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir -= _camera.transform.forward;
            }

            if (Input.GetKey(KeyCode.A))
            {
                dir -= _camera.transform.right;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir += _camera.transform.right;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                shiftScale = 2f;
            }

            if (!dir.Equals(Vector3.zero))
            {
                _camera.transform.position = _camera.transform.position + (dir * shiftScale * Time.deltaTime);
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
    }
}
