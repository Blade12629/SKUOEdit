using UnityEngine;
using Unity.Collections;
using Assets.Source.IO;

namespace Assets.Source.Game.Map.Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class UOTerrain : MonoBehaviour
    {
        public int RenderSize
        {
            get => _meshSize;
        }

        MeshCollider _collider;

        Mesh _mesh;
        Vertex[] _meshVertices;
        int _meshSize;

        UOTerrainCache _mapCache;

        Vector3 _position;

        public void Initialize(int meshSize, MapTiles tiles, Material material, Texture2D atlasTexture)
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            _mesh = filter.mesh = new Mesh();
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            _meshSize = meshSize;
            _meshVertices = new Vertex[meshSize * meshSize * 4];

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.material = new Material(material);
            renderer.material.mainTexture = atlasTexture;

            _mapCache = new UOTerrainCache(tiles);
        }

        public void MoveToWorld(Vector3 pos)
        {
            if (_position.Equals(pos))
                return;

            _position = pos;
            UpdateVertices(false);
        }

        public void Delete()
        {
            Destroy(gameObject);
        }

        public void Clear()
        {
            _mesh.Clear();
        }

        void UpdateVertices(bool recalculateIndices)
        {
            _mapCache.CopyTo(_meshVertices, (int)_position.x, (int)_position.z, _meshSize);

            if (recalculateIndices)
                _mesh.SetVertexBufferParams(_meshVertices.Length, VertexLayout.Layout);

            using (NativeArray<Vertex> nvertices = new NativeArray<Vertex>(_meshVertices, Allocator.Temp))
                _mesh.SetVertexBufferData(_meshVertices, 0, 0, _meshVertices.Length);

            if (recalculateIndices)
                _mesh.SetIndices(GenerateIndices(), MeshTopology.Triangles, 0);

            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            _collider.sharedMesh = _mesh;
        }

        int[] GenerateIndices()
        {
            int[] indices = new int[_meshSize * _meshSize * 6];

            int iv = 0;
            int i = 0;
            for (int x = 0; x < _meshSize; x++)
            {
                for (int z = 0; z < _meshSize; z++)
                {
                    indices[i++] = iv + 0;
                    indices[i++] = iv + 1;
                    indices[i++] = iv + 2;

                    indices[i++] = iv + 0;
                    indices[i++] = iv + 2;
                    indices[i++] = iv + 3;

                    iv += 4;
                }
            }

            return indices;
        }
    }
}
