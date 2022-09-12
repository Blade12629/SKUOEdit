using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Tests
{
    public class Texture2DPlane : MonoBehaviour
    {
        public static Texture2D BlankTexture
        {
            get
            {
                if (_blankTexture == null)
                {
                    _blankTexture = new Texture2D(44, 44);
                    Color32[] pixels = new Color32[44 * 44];

                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i] = new Color32(0, 0, 0, 1);

                    _blankTexture.SetPixels32(pixels);
                    _blankTexture.Apply();
                }

                return _blankTexture;
            }
        }

        static Texture2D _blankTexture;

        [SerializeField] MeshRenderer _renderer;
        [SerializeField] MeshFilter _filter;
        [SerializeField] Mesh _mesh;

        [SerializeField] Texture2D _texture;
        [SerializeField] Material _material;
        [SerializeField] Sprite _sprite;

        void Start()
        {
            _renderer = gameObject.AddComponent<MeshRenderer>();
            _filter = gameObject.AddComponent<MeshFilter>();
            _mesh = _filter.mesh = new Mesh();
            _mesh.SetVertexBufferParams(4, VertexLayout.Layout);

            _renderer.material = new Material(Resources.Load<Material>("ItemMaterial"));

            SetVertices(new Vertex[4]
            {
                new Vertex(0, 0, 0, 0, 0),
                new Vertex(0, 1, 0, 0, 1),
                new Vertex(1, 1, 0, 1, 1),
                new Vertex(1, 0, 0, 1, 0)
            });

            SetIndices(0, 1, 2,
                       0, 2, 3);
        }

        void Update()
        {
            if (_material != null)
            {
                _renderer.material = new Material(_material);
                _material = null;
            }

            if (_sprite != null)
            {
                _texture = _sprite.texture;
                _sprite = null;
            }

            if (_texture != null)
            {
                SetTexture(_texture);
                _texture = null;
            }
        }

        void FixedUpdate()
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        void SetTexture(Texture2D tex)
        {
            _renderer.material.mainTexture = tex;

            SetVertices(tex);
            _mesh.RecalculateBounds();
        }

        void SetVertices(Texture2D tex)
        {
            const float scale = 0.022727272727272727272727272727273f;

            float swidth = tex.width * scale;
            float sheight = tex.height * scale;

            SetVertices(new Vertex[4]
            {
                new Vertex(0,       0,       0, 0, 0),
                new Vertex(0,       sheight, 0, 0, 1),
                new Vertex(swidth,  sheight, 0, 1, 1),
                new Vertex(swidth,  0,       0, 1, 0)
            });

            SetVertices(new Vertex[4]
            {
                new Vertex(0,       0,       0, 1, 1),
                new Vertex(0,       sheight, 0, 1, 0),
                new Vertex(swidth,  sheight, 0, 0, 0),
                new Vertex(swidth,  0,       0, 0, 1)
            });
        }

        void SetVertices(params Vertex[] vertices)
        {
            if (vertices == null || vertices.Length == 0)
                return;

            using (NativeArray<Vertex> nvertices = new NativeArray<Vertex>(vertices, Allocator.Temp))
                _mesh.SetVertexBufferData(nvertices, 0, 0, 4);
        }

        void SetIndices(params int[] indices)
        {
            if (indices == null || indices.Length == 0)
                return;

            _mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }
    }
}
