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
            get => RemoveOffset(transform.position);
            set
            {
                Vector3 old = Position;
                Vector3 diff = value - old;

                transform.position = ApplyOffset(value);
                OnMoved?.Invoke(new CameraMovedArgs(value, diff));
            }
        }

        [SerializeField] float _speedMultiplier;
        [SerializeField] Vector3 _offset;

        void Update()
        {
            if (IsPaused)
                return;

            Vector3 dir = new Vector3();

            if (Input.GetKey(KeyCode.W))
            {
                dir.x--;
                dir.y--;
            }
            if (Input.GetKey(KeyCode.S))
            {
                dir.x++;
                dir.y++;
            }
            if (Input.GetKey(KeyCode.A))
            {
                dir.x++;
                dir.y--;
            }
            if (Input.GetKey(KeyCode.D))
            {
                dir.x--;
                dir.y++;
            }

            if (dir.x != 0 || dir.y != 0)
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    dir.x *= _speedMultiplier;
                    dir.y *= _speedMultiplier;
                }

                Position += dir;
            }
        }

        Vector3 ApplyOffset(Vector3 v)
        {
            return new Vector3(v.x + _offset.x, 
                               v.y + _offset.y, 
                               v.z + _offset.z);
        }

        Vector3 RemoveOffset(Vector3 v)
        {
            return new Vector3(v.x - _offset.x,
                               v.y - _offset.y,
                               v.z - _offset.z);
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
