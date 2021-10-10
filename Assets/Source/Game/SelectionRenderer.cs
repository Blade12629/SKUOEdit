using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

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

        public SelectionRenderer() : base()
        {
            Instance = this;
        }

        public void Refresh()
        {
            SetPosition(_lastPos);
        }

        public void SetPosition(Vector3 pos)
        {
            if (pos.x == _lastPos.x && pos.z == _lastPos.z)
                return;

            _lastPos = pos;
            int areaWidth = AreaSize;
            int areaDepth = AreaSize;

            if (pos.x < 0)
            {
                areaWidth -= (int)pos.x;

                if (areaWidth <= 0)
                    return;
            }
            else if (pos.x >= GameMap.Instance.Width)
            {
                areaWidth -= (int)(pos.x - GameMap.Instance.Width);

                if (areaWidth <= 0)
                    return;
            }

            if (pos.z < 0)
            {
                areaDepth -= (int)pos.z;

                if (areaDepth <= 0)
                    return;
            }
            else if (pos.z >= GameMap.Instance.Depth)
            {
                areaDepth -= (int)(pos.z - GameMap.Instance.Depth);

                if (areaDepth <= 0)
                    return;
            }

            int length = areaWidth * 2 + areaDepth * 2 + 1; 

            using (NativeArray<Vector3> tempPoints = new NativeArray<Vector3>(length, Allocator.TempJob))
            {
                CalculateLinePointsJob job = new CalculateLinePointsJob()
                {
                    StartPosition = new Vector3((int)pos.x, pos.y, (int)pos.z),
                    AreaWidth = areaWidth,
                    AreaDepth = areaDepth,
                    Result = tempPoints
                };

                JobHandle handle = job.Schedule();
                handle.Complete();

                _renderer.positionCount = length;
                _renderer.SetPositions(tempPoints.ToArray());
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

        struct CalculateLinePointsJob : IJob
        {
            public int AreaWidth;
            public int AreaDepth;
            public Vector3 StartPosition;

            public NativeArray<Vector3> Result;

            public void Execute()
            {
                int index = 0;
                int x = (int)StartPosition.x;
                int z = (int)StartPosition.z;

                AddDirection(ref index, ref x, ref z,  0,  1, AreaDepth);
                AddDirection(ref index, ref x, ref z,  1,  0, AreaWidth);
                AddDirection(ref index, ref x, ref z,  0, -1, AreaDepth);
                AddDirection(ref index, ref x, ref z, -1,  0, AreaWidth);

                Result[Result.Length - 1] = Result[0];
            }

            void AddDirection(ref int index, ref int x, ref int z, int xDir, int zDir, int length)
            {
                for (int i = 0; i < length; i++)
                {
                    AddPoint(ref index, ref x, ref z);

                    x += xDir;
                    z += zDir;
                }
            }

            void AddPoint(ref int index, ref int x, ref int z)
            {
                float height = GameMap.Instance.GetTileCornerHeight(x, z) * .1f + .1f;
                Result[index++] = new Vector3(x, height, z);
            }
        }
    }
}
