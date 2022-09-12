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

        public int Height
        {
            get => _height;
            set
            {
                _height = value;

                //Vector3 pos = transform.position;
                //pos.y = value * MapConstants.TILE_HEIGHT_MULTIPLIER + MapConstants.TILE_HEIGHT_OFFSET;

                //transform.position = pos;
            }
        }

        uint _itemId;
        int _height;
        
        MeshCollider _collider;
        MeshRenderer _renderer;
        MeshFilter _filter;
        Mesh _mesh;

        [SerializeField] Vertex[] _vertices;

#if UNITY_EDITOR
        [SerializeField] bool _editorUpdateVertices;
#endif

        void Update()
        {
#if UNITY_EDITOR
            if (_editorUpdateVertices)
            {
                _editorUpdateVertices = false;
                UpdateAndInvalidate(_vertices);
            }
#endif
        }

        public void Initialize(Material mat)
        {
            _collider = gameObject.AddComponent<MeshCollider>();
            _renderer = gameObject.AddComponent<MeshRenderer>();
            _renderer.material = new Material(mat);
            _filter = gameObject.AddComponent<MeshFilter>();
            _mesh = _filter.mesh = new Mesh();
            _mesh.SetVertexBufferParams(4, VertexLayout.Layout);

            _vertices = new Vertex[]
            {
                new Vertex(1, 0, 0, 0, 0),
                new Vertex(1, 1, 0, 0, 1),
                new Vertex(0, 1, 0, 1, 1),
                new Vertex(0, 0, 0, 1, 0),
            };

            UpdateVertices(_vertices);
            GenerateIndices();
            InvalidateChanges();
        }

        void SetItemId(uint id)
        {
            const float TILE_SIZE = 44f;

            if (_itemId == id)
                return;

            if (id == 0)
            {
                //transform.localScale = Vector3.zero;
                return;
            }

            Texture2D tex = Ultima.UltimaArt.GetStatic(id);
            _renderer.material.mainTexture = tex;

            float height = tex.height / TILE_SIZE;
            float width = tex.width / 44f;
            transform.localScale = new Vector3(width * 1.5f, height, 1f);

            _itemId = id;
        }

        void UpdateAndInvalidate(Vertex[] vertices)
        {
            UpdateVertices(vertices);
            InvalidateChanges();
        }

        void UpdateVertices(Vertex[] vertices)
        {
            using (NativeArray<Vertex> verts = new NativeArray<Vertex>(vertices, Allocator.Temp))
                _mesh.SetVertexBufferData(verts, 0, 0, vertices.Length, 0);
        }

        void InvalidateChanges()
        {
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            //_mesh.RecalculateTangents();
            _collider.sharedMesh = _mesh;
        }

        void GenerateIndices()
        {
            _mesh.SetIndices(new int[]
            {
                0, 1, 2,
                0, 2, 3
                //2, 1, 0,
                //3, 2, 0
            }, MeshTopology.Triangles, 0);
        }
    }
}
