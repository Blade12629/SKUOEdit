using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    public class Item : MonoBehaviour
    {
        public uint ItemId
        {
            get => _itemId;
            set => SetItemId(value);
        }

        static readonly int[] _indices = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        static readonly Vertex[] _vertices = new Vertex[]
        {
            new Vertex(0, 0, 0, 0, 0),
            new Vertex(0, 1, 0, 0, 1),
            new Vertex(1, 1, 0, 1, 1),
            new Vertex(1, 0, 0, 1, 0)
        };

        uint _itemId;
        
        MeshCollider _collider;
        MeshRenderer _renderer;
        MeshFilter _filter;
        Mesh _mesh;

        public void Initialize(Material mat)
        {
            _collider = gameObject.AddComponent<MeshCollider>();
            _renderer = gameObject.AddComponent<MeshRenderer>();
            _renderer.material = new Material(mat);
            _filter = gameObject.AddComponent<MeshFilter>();
            _mesh = _filter.mesh = new Mesh();
            _mesh.SetVertexBufferParams(4, VertexLayout.Layout);

            using (NativeArray<Vertex> vertices = new NativeArray<Vertex>(_vertices, Allocator.Temp))
                _mesh.SetVertexBufferData(vertices, 0, 0, _vertices.Length, 0);

            _mesh.SetIndices(_indices, MeshTopology.Triangles, 0);
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            _collider.sharedMesh = _mesh;
        }

        void SetItemId(uint id)
        {
            if (_itemId == id)
                return;

            if (id == 0)
            {
                transform.localScale = Vector3.zero;
                return;
            }

            Texture2D tex = Ultima.UltimaArt.GetStatic(id);
            _renderer.material.mainTexture = tex;
            transform.localScale = new Vector3(1, tex.height / (float)Constants.TileSize, 1);

            _itemId = id;
        }
    }
}
