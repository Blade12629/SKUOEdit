using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Statics
{
    public class Item : MonoBehaviour
    {
        public StaticInfo StaticInfo { get; private set; }
        public int Id => StaticInfo.Id;
        public Vertex[] Vertices => StaticInfo.Vertices;

        public void GenerateItemMesh(StaticInfo stInfo)
        {
            StaticInfo = stInfo;

            Mesh mesh = GetComponent<MeshFilter>().mesh = new Mesh();

            Material material = new Material(Client.Instance.DefaultMaterial);
            material.mainTexture = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture((uint)stInfo.Id);

            GetComponent<MeshRenderer>().material = material;

            using (NativeArray<Vertex> vertices = new NativeArray<Vertex>(Vertices, Allocator.Temp))
            {
                mesh.SetVertexBufferParams(vertices.Length, VertexLayout.Layout);
                mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
            }

            mesh.SetIndices(GenerateIndices(), MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();

            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        int[] GenerateIndices()
        {
            int totalTriangles = Vertices.Length / 4;
            int[] indices = new int[totalTriangles * 6];

            int indiceIndex = 0;
            int verticeIndex = 0;
            for (int i = 0; i < totalTriangles; i++)
            {
                indices[indiceIndex++] = verticeIndex + 0;
                indices[indiceIndex++] = verticeIndex + 1;
                indices[indiceIndex++] = verticeIndex + 2;

                indices[indiceIndex++] = verticeIndex + 0;
                indices[indiceIndex++] = verticeIndex + 2;
                indices[indiceIndex++] = verticeIndex + 3;

                verticeIndex += 4;
            }

            return indices;
        }
    }
}
