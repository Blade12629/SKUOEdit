using Assets.Source.IO;
using Assets.Source.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Terrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class StaticMesh : MonoBehaviour
    {
        Mesh _mesh;

        [SerializeField] UnityEngine.UI.RawImage _testImg;

        public void Init(/*Art art*/)
        {
            _mesh = GetComponent<MeshFilter>().mesh = new Mesh();

            Vertex[] vertices = new Vertex[]
            {
                new Vertex(0, 0, 0,     0.0f,   0.0f), // front, bl 0
                new Vertex(0, 1, 0,     0.0f,   1.0f), // tl 1
                new Vertex(1, 1, 0,     1.0f,   1.0f), // tr 2
                new Vertex(1, 0, 0,     1.0f,   0.0f), // br 3
                
                //new Vertex(0, 1, 1,     0.0f,   1.0f), // top, tl 4 
                //new Vertex(1, 1, 1,     1.0f,   0.5f), // tr 5

                //new Vertex(1, 0, 1,     1.0f,   0.0f), // right, br 6
            };

            int[] indices = new int[]
            {
                0, 1, 2,
                0, 2, 3,

                //1, 4, 5,
                //1, 5, 2,

                //3, 2, 5,
                //3, 5, 6

                //4, 5, 1,
                //4, 1, 0,

                //3, 2, 6,
                //3, 6, 7,

                //8, 9, 10,
                //8, 10, 11
            };

            _mesh.SetVertexBufferParams(vertices.Length, VertexLayout.Layout);

            NativeArray<Vertex> nvertices = new NativeArray<Vertex>(vertices, Allocator.Temp);
            _mesh.SetVertexBufferData(nvertices, 0, 0, nvertices.Length);

            _mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            _mesh.RecalculateBounds();

            Texture2D tex = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(128);
            GetComponent<MeshRenderer>().material.mainTexture = tex;

            transform.localScale = new Vector3(1f, 2f, 1f);
        }
    }
}
