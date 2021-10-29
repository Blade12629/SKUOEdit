using Assets.Source.Game.Map;
using System;
using System.Collections.Generic;
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

        static readonly Quaternion _staticRotation = Quaternion.Euler(0, -90, 0);

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

        public SelectionRenderer() : base()
        {
            Instance = this;
            _areaSize = 1;
        }

        public void Refresh()
        {
            Vector3[] linePoints = new Vector3[_renderer.positionCount];
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
            int x;
            int z;
            pos.x = x = (int)pos.x;
            pos.z = z = (int)pos.z;

            if (pos.x == _lastPos.x && pos.z == _lastPos.z)
                return;

            _lastPos = pos;
            List<Vector3> linePoints = new List<Vector3>(4);

            int xEnd = (int)pos.x + AreaSize;
            int zEnd = (int)pos.z + AreaSize;

            AddDirection(linePoints, x, z, AreaSize, 0, 1);
            AddDirection(linePoints, x, zEnd, AreaSize, 1, 0);
            AddDirection(linePoints, xEnd, zEnd, AreaSize, 0, -1);
            AddDirection(linePoints, xEnd, z, AreaSize + 1, -1, 0);

            _renderer.positionCount = linePoints.Count;
            _renderer.SetPositions(linePoints.ToArray());
        }

        public void SetStatic(GameObject st)
        {
            if (st.transform.position.Equals(_lastPos))
                return;

            Mesh m = st.GetComponent<MeshFilter>()?.mesh;

            if (m == null)
                return;

            _lastPos = st.transform.position;

            List<Vector3> verts = new List<Vector3>();
            m.GetVertices(verts);

            Vector3[] renderVerts = new Vector3[verts.Count / 4];
            Vector3 center = st.transform.position + new Vector3(.5f, .5f, .5f);

            for (int i = 0; i < renderVerts.Length; i++)
            {
                Vector3 v = _staticRotation * ((verts[i * 4] + st.transform.position) - center) + center;
                v.x--;

                renderVerts[i] = v;
            }

            _renderer.positionCount = renderVerts.Length;
            _renderer.SetPositions(renderVerts);
        }

        void AddDirection(List<Vector3> linePoints, int x, int z, int count, int xStep, int zStep)
        {
            for (int i = 0; i < count; i++, x += xStep, z += zStep)
            {
                linePoints.Add(new Vector3(x, GameMap.Instance.GetTileCornerHeight(x, z) * .1f + .1f, z));
            }
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
