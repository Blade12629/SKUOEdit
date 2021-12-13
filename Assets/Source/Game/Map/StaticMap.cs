﻿using Assets.Source.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    public class StaticMap
    {
        public int Width { get; private set; }
        public int Depth { get; private set; }
        public int BlockWidth { get; private set; }
        public int BlockDepth { get; private set; }

        Dictionary<Vector2, List<GameObject>> _statics;
        MapStatics _mapStatics;
        GameObject _lastSelectedStatic;

        public StaticMap()
        {
            _statics = new Dictionary<Vector2, List<GameObject>>();
        }

        public void LoadMap(string staticMulFile, string staticIdxFile, int width, int depth)
        {
            Width = width;
            Depth = depth;
            BlockWidth = width / 8;
            BlockDepth = depth / 8;

            _mapStatics.Load(staticMulFile, staticIdxFile, Width, Depth);
        }

        public void SetSelectedStatic(GameObject st)
        {
            if (_lastSelectedStatic != null)
            {
                if (st.Equals(_lastSelectedStatic))
                    return;

                _lastSelectedStatic.GetComponent<MeshRenderer>().material.SetInt("_EnableSelectedRendering", 0);
            }

            st.GetComponent<MeshRenderer>().material.SetInt("_EnableSelectedRendering", 1);
            _lastSelectedStatic = st;
        }

        public void UnsetSelectedStatic()
        {
            if (_lastSelectedStatic == null)
                return;

            _lastSelectedStatic.GetComponent<MeshRenderer>().material.SetInt("_EnableSelectedRendering", 0);
        }

        public void SpawnStatics()
        {
            foreach (var statics in _statics.Values)
            {
                StaticPool.ReturnRange(statics, true);
            }

            for (int bx = 0; bx < BlockWidth; bx++)
            {
                int wx = bx * 8;

                for (int bz = 0; bz < BlockDepth; bz++)
                {
                    int wz = bz * 8;
                    StaticBlock sb = _mapStatics.GetStaticBlock(wx, wz);

                    if (sb == null)
                        continue;

                    for (int i = 0; i < sb.Statics.Length; i++)
                    {
                        ref var st = ref sb.Statics[i];
                        SpawnStatic(st.TileId, new Vector3(wx + st.Y, st.Z * .1f, wz + st.X));
                    }
                }
            }
        }

        void SpawnStatic(uint staticId, Vector3 pos)
        {
            StaticCacheEntry entry = StaticCache.Get(staticId);

            if (entry == null)
                return;

            GameObject staticObj = StaticPool.Rent(true);
            staticObj.name = staticId.ToString();
            Mesh mesh = staticObj.GetComponent<MeshFilter>().mesh = new Mesh();

            mesh.SetVertexBufferParams(entry.Vertices.Length, VertexLayout.Layout);
            mesh.SetVertexBufferData(entry.Vertices, 0, 0, entry.Vertices.Length);
            mesh.SetIndices(entry.Indices, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();

            MeshRenderer renderer = staticObj.GetComponent<MeshRenderer>();
            renderer.material = new Material(Client.Instance.DefaultStaticMaterial);
            renderer.material.mainTexture = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(staticId);
            staticObj.transform.position = pos;

            MeshCollider collider = staticObj.GetComponent<MeshCollider>();
            // Force rebuild of collider
            collider.sharedMesh = mesh;

            Vector2 staticDictPos = new Vector2(pos.x, pos.z);

            if (!_statics.TryGetValue(staticDictPos, out List<GameObject> statics))
                _statics.Add(staticDictPos, statics = new List<GameObject>(100));

            statics.Add(staticObj);
        }
    }
}