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
            get => _position;
            set
            {
                Vector3 diff = value - _position;
                _position = value;

                OnMoved?.Invoke(new CameraMovedArgs(value, diff));
            }
        }

        [SerializeField] float _speedMultiplier;
        [SerializeField] Vector3 _position;

        void Update()
        {
            if (IsPaused)
                return;

            Vector3 dir = new Vector3();

            if (Input.GetKey(KeyCode.W))
                dir.y--;
            if (Input.GetKey(KeyCode.S))
                dir.y++;
            if (Input.GetKey(KeyCode.A))
                dir.x--;
            if (Input.GetKey(KeyCode.D))
                dir.x++;

            if (dir.x != 0 || dir.y != 0)
            {
                dir.x *= Time.deltaTime;

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    dir.x *= _speedMultiplier;

                _position += dir;
                OnMoved?.Invoke(new CameraMovedArgs(_position, dir));
            }
        }
<<<<<<< HEAD

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
=======
>>>>>>> parent of bf27347 (.)
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
