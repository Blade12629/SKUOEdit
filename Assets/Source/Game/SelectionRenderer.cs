using Assets.Source.Game.Map;
using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


namespace Assets.Source.Game
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class SelectionRenderer : MonoBehaviour
    {
        public static int MaxAreaSize => 20;
        public static SelectionRenderer Instance { get; private set; }

        public int AreaSize
        {
            get => _areaSize;
            set
            {
                if (value > MaxAreaSize || value <= 0)
                    return;

                _areaSize = value;

                if (!_lastPos.Equals(Vector3.zero))
                    SetPosition(_lastPos);
            }
        }

        [SerializeField] int _areaSize;
        LineRenderer _renderer;
        Vector3 _lastPos;

        readonly Vector2[] _defaultPointOffsets = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0),
        };

        public SelectionRenderer() : base()
        {
            Instance = this;
        }

        public void Refresh()
        {
            Vector3[] linePoints = new Vector3[_defaultPointOffsets.Length + 1];
            _renderer.GetPositions(linePoints);

            for (int i = 0; i < linePoints.Length; i++)
            {
                ref Vector3 point = ref linePoints[i];
                point.y = GameMap.Instance.GetTileCornerHeight((int)point.x, (int)point.z) * .1f + .1f;
            }

            _renderer.SetPositions(linePoints);
        }

        public void SetPosition(Vector3 pos)
        {
            pos.x = (int)pos.x;
            pos.z = (int)pos.z;

            if (pos.x == _lastPos.x && pos.z == _lastPos.z)
                return;

            _lastPos = pos;
            Vector2[] pointOffsets = _defaultPointOffsets;
            Vector3[] linePoints = new Vector3[pointOffsets.Length + 1];

            for (int i = 0; i < pointOffsets.Length; i++)
            {
                Vector2 offset = pointOffsets[i] * AreaSize;
                Vector3 curPos = new Vector3(pos.x + offset.x, 0, pos.z + offset.y);

                curPos.y = GameMap.Instance.GetTileCornerHeight((int)curPos.x, (int)curPos.z) * .1f + .1f;
                linePoints[i] = curPos;
            }

            linePoints[linePoints.Length - 1] = linePoints[0];

            _renderer.positionCount = linePoints.Length;
            _renderer.SetPositions(linePoints);
        }

        public void Clear()
        {
            _renderer.positionCount = 0;
            _renderer.SetPositions(Array.Empty<Vector3>());
        }

        void Start()
        {
            _renderer = GetComponent<LineRenderer>();
        }
    }
}
