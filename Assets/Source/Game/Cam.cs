using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game
{
    public class Cam : MonoBehaviour
    {
        public event Action<CameraMovedArgs> OnMoved;

        public bool IsPaused { get; set; }
        public Vector3 Position
        {
            get => GetPosition();
            set => MoveToPosition(value, true);
        }

        [SerializeField] float _speedMultiplier;
        [SerializeField] Vector2 _offset;

        /// <summary>
        /// Sets the camera position without invoking <see cref="OnMoved"/>
        /// </summary>
        public void MoveToPosition(Vector3 position, bool invokeCameraMoved)
        {
            Vector3 srcPos = GetPosition();

            Vector3 destWOffset = position;
            ApplyOffset(ref destWOffset);

            position.z = srcPos.z;
            destWOffset.z = srcPos.z;

            transform.position = destWOffset;

            if (invokeCameraMoved)
                OnMoved?.Invoke(new CameraMovedArgs(position, position - srcPos));
        }

        public Vector3 GetPosition()
        {
            Vector3 pos = transform.position;
            RemoveOffset(ref pos);

            return pos;
        }

        void Update()
        {
            if (IsPaused)
                return;

            Vector3 dir = new Vector3();

            if (Input.GetKey(KeyCode.W))
            {
                dir.y++;
                dir.x--;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir.y--;
                dir.x++;
            }
            if (Input.GetKey(KeyCode.A))
            {
                dir.x--;
                dir.y--;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir.x++;
                dir.y++;
            }

            if (dir.x != 0 || dir.y != 0)
            {
                //dir.x *= Time.deltaTime;

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    dir.x *= _speedMultiplier;
                    dir.y *= _speedMultiplier;
                }

                MoveToPosition(GetPosition() + dir, true);
            }
        }

        void ApplyOffset(ref Vector3 v)
        {
            v.x += _offset.x;
            v.y -= _offset.y;
        }

        void RemoveOffset(ref Vector3 v)
        {
            v.x -= _offset.x;
            v.y += _offset.y;
        }
    }

    public class CameraMovedArgs
    {
        public Vector3 NewPosition { get; }
        public Vector3 Direction { get; }

        public CameraMovedArgs(Vector3 newPosition, Vector3 direction)
        {
            NewPosition = newPosition;
            Direction = direction;
        }
    }
}
