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
    public class Item : MonoBehaviour, IEquatable<Item>
    {
        public Guid Id { get; private set; }
        public bool IsInitialized { get; private set; }
        public uint ItemId
        {
            get => _itemId;
            set
            {
                if (_itemId == value)
                    return;

                _itemId = value;
                ApplyItemTexture();
            }
        }

        uint _itemId;
        uint _lastItemId;

        MeshCollider _collider;
        MeshRenderer _renderer;
        MeshFilter _filter;
        Mesh _mesh;

        Vertex[] _vertices;

        public void Initialize(Material material)
        {
            if (IsInitialized)
                return;

            Id = Guid.NewGuid();
            _collider = gameObject.AddComponent<MeshCollider>();
            _renderer = gameObject.AddComponent<MeshRenderer>();
            _renderer.material = new Material(material);
            _filter = gameObject.AddComponent<MeshFilter>();
            _mesh = _filter.mesh = new Mesh();
            _vertices = new Vertex[4]
            {
                new Vertex(0, 0, 0, 0, 0),
                new Vertex(0, 1, 0, 0, 1),
                new Vertex(1, 1, 0, 1, 1),
                new Vertex(1, 0, 0, 1, 0),
            };

            _mesh.SetVertexBufferParams(4, VertexLayout.Layout);

            Invalidate(false);

            _mesh.SetIndices(new int[]
            {
                0, 1, 2,
                0, 2, 3
            }, MeshTopology.Triangles, 0);

            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            _collider.sharedMesh = _mesh;
            IsInitialized = true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Item);
        }

        public bool Equals(Item other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            int hashCode = 1545243542;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Item left, Item right)
        {
            return EqualityComparer<Item>.Default.Equals(left, right);
        }

        public static bool operator !=(Item left, Item right)
        {
            return !(left == right);
        }

        void Invalidate(bool recalculateMesh = true)
        {
            if (_lastItemId == _itemId)
                return;

            _lastItemId = _itemId;

            using (NativeArray<Vertex> vertices = new NativeArray<Vertex>(_vertices, Allocator.Temp))
                _mesh.SetVertexBufferData(vertices, 0, 0, _vertices.Length, 0);

            if (recalculateMesh)
            {
                _mesh.RecalculateBounds();
                _mesh.RecalculateNormals();
                _collider.sharedMesh = _mesh;
            }
        }

        void ApplyItemTexture()
        {
            const float TILE_SIZE = 44f;

            Texture2D texture = UltimaArt.GetStatic(ItemId);
            _renderer.material.mainTexture = texture;

            float height = texture.height / TILE_SIZE;

            ref Vertex topLeft = ref _vertices[1];
            ref Vertex topRight = ref _vertices[2];

            if (topLeft.Y != height || topRight.Y != height)
            {
                topLeft.Y = height;
                topRight.Y = height;

                Invalidate();
            }
        }
    }
}
