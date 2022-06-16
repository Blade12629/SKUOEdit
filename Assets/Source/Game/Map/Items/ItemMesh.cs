using Assets.Source.Ultima;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    [RequireComponent(typeof(MeshCollider), typeof(MeshRenderer), typeof(MeshFilter))]
    public class ItemMesh : MonoBehaviour
    {
        const int _CACHE_SIZE = 128 * 128;

        public int AvailableSlots => _freeVertexIndices.Count;
        public int UsedSlots => _lookup.Count;

        Dictionary<Vector3, ItemLookup> _lookup;

        Vertex[] _vertices;
        int[] _indices;
        Queue<int> _freeVertexIndices;

        MeshCollider _collider;
        MeshFilter _filter;
        MeshRenderer _renderer;
        Mesh _mesh;

        public bool AddItem(Vector3 pos, uint itemId)
        {
            if (AvailableSlots == 0 || Exists(pos))
                return false;

            int sv = _freeVertexIndices.Dequeue();
            ItemLookup entry = new ItemLookup(pos, itemId, sv, 4);

            Vector2[] uvs = Art.GetStaticUVs(itemId);
            Vector2 size = Art.GetStaticSize(itemId);
            float height = size.y / 44f;

            _vertices[sv++] = new Vertex(pos.x,         pos.y,          pos.z, uvs[0].x, uvs[0].y);
            _vertices[sv++] = new Vertex(pos.x,         pos.y + height, pos.z, uvs[1].x, uvs[1].y);
            _vertices[sv++] = new Vertex(pos.x + 1f,    pos.y + height, pos.z, uvs[2].x, uvs[2].y);
            _vertices[sv]   = new Vertex(pos.x + 1f,    pos.y,          pos.z, uvs[3].x, uvs[3].y);

            _lookup[pos] = entry;
            return true;
        }

        public bool RemoveItem(Vector3 pos)
        {
            if (_lookup.TryGetValue(pos, out ItemLookup lookup))
            {
                RemoveEntry(lookup);
                return true;
            }

            return false;
        }

        public uint GetItemId(Vector3 pos)
        {
            if (_lookup.TryGetValue(pos, out ItemLookup lookup))
                return lookup.ItemId;

            return 0;
        }

        public bool SetItemId(Vector3 pos, uint newItemId)
        {
            RemoveItem(pos);
            return AddItem(pos, newItemId);
        }

        public bool Exists(Vector3 pos)
        {
            return _lookup.ContainsKey(pos);
        }

        public bool Contains(Vector3 pos)
        {
            return Exists(pos);
        }

        public void Invalidate()
        {
            using (NativeArray<Vertex> vertices = new NativeArray<Vertex>(_vertices, Allocator.Temp))
                _mesh.SetVertexBufferData(vertices, 0, 0, _vertices.Length);

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _collider.sharedMesh = _mesh;
        }

        void Start()
        {
            _lookup = new Dictionary<Vector3, ItemLookup>();
            _vertices = new Vertex[_CACHE_SIZE * 4];
            _indices = new int[_CACHE_SIZE * 6];
            _freeVertexIndices = new Queue<int>(_CACHE_SIZE);

            for (int i = 0; i < _CACHE_SIZE; i++)
            {
                int sv = i * 4;
                int si = i * 6;
                _freeVertexIndices.Enqueue(sv);

                _indices[si++] = sv + 0;
                _indices[si++] = sv + 1;
                _indices[si++] = sv + 2;

                _indices[si++] = sv + 0;
                _indices[si++] = sv + 2;
                _indices[si++] = sv + 3;
            }

            _collider = GetComponent<MeshCollider>();
            _renderer = GetComponent<MeshRenderer>();
            _filter = GetComponent<MeshFilter>();
            _mesh = _filter.mesh = new Mesh();

            _renderer.material = new Material(Core.StaticMaterial);
            _renderer.material.mainTexture = Art.AtlasTextureItems;

            _mesh.SetVertexBufferParams(_vertices.Length, VertexLayout.Layout);

            Invalidate();

            _mesh.SetIndices(_indices, MeshTopology.Triangles, 0);

            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _collider.sharedMesh = _mesh;
        }

        void RemoveEntry(ItemLookup entry)
        {
            int j = entry.Index;
            _freeVertexIndices.Enqueue(j);

            _vertices[j++] = new Vertex();
            _vertices[j++] = new Vertex();
            _vertices[j++] = new Vertex();
            _vertices[j] = new Vertex();

            _lookup.Remove(entry.Position);
        }
    }
}
