using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Terrain
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class TerrainMesh : MonoBehaviour
    {
        /// <summary>
        /// UO Map Offset
        /// </summary>
        public int MapOffsetX
        {
            get => _mapOffsetX;
            set => _mapOffsetX = value;
        }

        /// <summary>
        /// UO Map Offset
        /// </summary>
        public int MapOffsetZ
        {
            get => _mapOffsetZ;
            set => _mapOffsetZ = value;
        }

        /// <summary>
        /// Mesh Width
        /// </summary>
        public int Width => _meshWidth;

        /// <summary>
        /// Mesh Depth
        /// </summary>
        public int Depth => _meshDepth;

        VertexMap _map;
        Mesh _mesh;
        Vertex[] _vertices;

        int _meshWidth;
        int _meshDepth;

        int _mapOffsetX;
        int _mapOffsetZ;

        MeshCollider _collider;

        /// <summary>
        /// Initializes the mesh after it has been created
        /// </summary>
        /// <param name="atlasTexture"></param>
        /// <param name="map"></param>
        /// <param name="meshWidth"></param>
        /// <param name="meshDepth"></param>
        public void SetBase(Texture2D atlasTexture, VertexMap map, int meshWidth, int meshDepth)
        {
            GetComponent<MeshRenderer>().material.mainTexture = atlasTexture;
            _mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            _map = map;

            _collider = GetComponent<MeshCollider>();

            Resize(meshWidth, meshDepth);
            _collider.sharedMesh = _mesh;
        }

        /// <summary>
        /// Resizes the mesh
        /// </summary>
        /// <param name="meshWidth"></param>
        /// <param name="meshDepth"></param>
        public void Resize(int meshWidth, int meshDepth)
        {
            _meshWidth = meshWidth;
            _meshDepth = meshDepth;
            _vertices = new Vertex[meshWidth * meshDepth * 4];

            _mesh.SetVertexBufferParams(_vertices.Length, VertexLayout.Layout);

            ApplyNew(true);

            _mesh.SetIndices(VertexMap.GetIndices(meshWidth, meshDepth), MeshTopology.Triangles, 0);
        }

        /// <summary>
        /// Apply the current mesh data
        /// </summary>
        /// <param name="copy">Copy mesh data from <see cref="VertexMap"/> before applying it</param>
        public void ApplyNew(bool copy = false)
        {
            if (copy)
            {
                _map.Copy(_vertices, _mapOffsetX, _mapOffsetZ, _meshWidth, _meshDepth);
            }

            NativeArray<Vertex> vertices = new NativeArray<Vertex>(_vertices, Allocator.Temp);
            _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
            _mesh.RecalculateBounds();
            _collider.sharedMesh = _mesh;
        }
    }
}
