//using Assets.Source.Terrain;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Assets.Source.Rendering
//{
//    [RequireComponent(typeof(LineRenderer))]
//    public class SelectionRenderer : MonoBehaviour
//    {
//        public static int MaxAreaSize => 20;
//        public static SelectionRenderer Instance { get; private set; }

//        public int AreaSize
//        {
//            get => _areaSize;
//            set
//            {
//                if (value > MaxAreaSize || value <= 0)
//                    return;

//                _areaSize = value;

//                if (!_lastPos.Equals(Vector3.zero))
//                    SetPosition(_lastPos);
//            }
//        }

//        [SerializeField] int _areaSize;
//        LineRenderer _renderer;
//        Vector3 _lastPos;
//        HeightTable _table;

//        public SelectionRenderer() : base()
//        {
//            Instance = this;
//        }

//        public void Init(HeightTable table)
//        {
//            _table = table;
//        }

//        public void Refresh()
//        {
//            SetPosition(_lastPos);
//        }

//        public void SetPosition(Vector3 pos)
//        {
//            _lastPos = pos;

//            if (_table.TryGetHeight(pos.x, pos.z, out int hstart))
//                pos.y = hstart * .1f;

//            List<Vector3> points = new List<Vector3>()
//            {
//                new Vector3((int)pos.x, pos.y + .1f, (int)pos.z)
//            };

//            AddDirections(points, points[0], AreaSize, new Vector3(0, 0, 1));
//            AddDirections(points, points[points.Count - 1], AreaSize, new Vector3(1, 0, 0));
//            AddDirections(points, points[points.Count - 1], AreaSize, new Vector3(0, 0, -1));
//            AddDirections(points, points[points.Count - 1], AreaSize, new Vector3(-1, 0, 0));

//            points.Add(points[0]);

//            _renderer.positionCount = points.Count;
//            _renderer.SetPositions(points.ToArray());
//        }

//        void AddDirections(List<Vector3> points, Vector3 pos, int length, Vector3 direction)
//        {
//            if (_table.TryGetHeight(pos.x, pos.z, out int _h))
//                pos.y = _h * .1f;

//            float lastHeight = pos.y;
//            pos.y += .1f;
//            points.Add(pos);
//            pos.y -= .1f;

//            for (int i = 0; i < length; i++)
//            {
//                pos += direction;

//                if (_table.TryGetHeight(pos.x, pos.z, out int height))
//                {
//                    pos.y = height * .1f;
//                }
//                else if (pos.y == lastHeight && i != length - 1)
//                    continue;

//                lastHeight = pos.y;
//                pos.y += .1f;
//                points.Add(pos);
//                pos.y -= .1f;
//            }
//        }

//        public void Clear()
//        {
//            _renderer.positionCount = 0;
//            _renderer.SetPositions(Array.Empty<Vector3>());
//        }

//        void Start()
//        {
//            _renderer = GetComponent<LineRenderer>();
//        }
//    }
//}
