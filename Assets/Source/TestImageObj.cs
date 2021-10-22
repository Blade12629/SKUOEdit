using Assets.Source.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class TestImageObj : MonoBehaviour
    {
        MeshRenderer _renderer;
        MeshFilter _filter;

        [SerializeField] uint _staticId;
        [SerializeField] bool _updateTex;

        [SerializeField] bool _followCamera;
        [SerializeField] Camera _camera;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _filter = GetComponent<MeshFilter>();
        }

        private void Update()
        {
            if (_updateTex)
            {
                _updateTex = false;
                Texture2D tex = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(_staticId);

                if (tex != null)
                    SetMesh(tex);
            }

            if (_followCamera)
            {
                transform.LookAt(_camera.transform);
            }
        }

        void SetMesh(Texture2D tex)
        {
            Mesh mesh;

            if (_filter.mesh == null)
                mesh = _filter.mesh = new Mesh();
            else
                mesh = _filter.mesh;

            Vertex[] vertices = new Vertex[]
            {
                new Vertex(0, 0, 0, 0, 0),
                new Vertex(0, 1, 0, 0, 1),
                new Vertex(1, 1, 0, 1, 1),
                new Vertex(1, 0, 0, 1, 0)
            };

            int[] indices = new int[]
            {
                0, 1, 2,
                0, 2, 3
            };

            mesh.SetVertexBufferParams(vertices.Length, VertexLayout.Layout);
            mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();

            _renderer.material.mainTexture = tex;
        }
    }
}
