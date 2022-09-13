using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    /// <summary>
    /// Mesh to display Statics as 3d objects
    /// </summary>
    public class StaticMesh : MonoBehaviour
    {
        // UO tiles are 44x44, the number is 44 scaled down to 1
        const float _TEXTURE_SCALE = 0.022727272727272727272727272727273f;

        /// <summary>
        /// The static id
        /// </summary>
        public uint Id 
        {
            get => _id;
            set => Update(value);
        }

        // TODO: remove or make it change actual height
        public int Height
        {
            get => _height;
            set => _height = value;
        }

        MeshRenderer _renderer;
        MeshCollider _collider;
        MeshFilter _filter;
        Mesh _mesh;

        [SerializeField] uint _id;
        [SerializeField] int _height;

        /// <summary>
        /// Creates a static object with the id 0
        /// </summary>
        public static StaticMesh CreateStatic(Material mat)
        {
            return CreateStatic(0, mat);
        }

        /// <summary>
        /// Creates a static object
        /// </summary>
        /// <param name="id">Static id</param>
        public static StaticMesh CreateStatic(uint id, Material mat)
        {
            GameObject gobj = new GameObject(id.ToString(), typeof(MeshRenderer), typeof(MeshFilter),
                                                            typeof(MeshCollider), typeof(StaticMesh));
            StaticMesh smesh = gobj.GetComponent<StaticMesh>();
            smesh.Create(id, mat);

            return smesh;
        }

        // Required since unity otherwise will complain due to us having another Update method with parameters
        void Update()
        {
            
        }

        /// <summary>
        /// Creates the static, should only be called once when the static gets instantiated
        /// </summary>
        /// <param name="id"></param>
        void Create(uint id, Material material)
        {
            // TODO: probably will need to rotate this

            _renderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<MeshCollider>();
            _filter = GetComponent<MeshFilter>();
            _mesh = _filter.mesh = new Mesh();

            _renderer.material = new Material(material);
            _mesh.SetVertexBufferParams(4, VertexLayout.Layout);

            Update(id, false);

            _mesh.SetIndices(new int[]
            {
                2, 1, 0,
                3, 2, 0
                //0, 1, 2,
                //0, 2, 3
            }, MeshTopology.Triangles, 0);
            _mesh.RecalculateBounds();
        }

        /// <summary>
        /// Updates the static with the specified <paramref name="id"/>
        /// </summary>
        /// <param name="id">Static id</param>
        /// <param name="recalculateBounds">Recalculate mesh bounds</param>
        void Update(uint id, bool recalculateBounds = true)
        {
            Texture2D texture = Ultima.UltimaArt.GetStatic(id);
            float scaledHeight = texture.height * _TEXTURE_SCALE;
            float scaledWidth = texture.width * _TEXTURE_SCALE;

            //ApplyVertices(new Vertex(0, 0, 0, 0, 0),
            //              new Vertex(0, scaledHeight, 0, 0, 1),
            //              new Vertex(scaledWidth, scaledHeight, 0, 1, 1),
            //              new Vertex(scaledWidth, 0, 0, 1, 0));

            ApplyVertices(new Vertex(0, 0, 0,                       1, 0),
                          new Vertex(0, scaledHeight, 0,            1, 1),
                          new Vertex(scaledWidth, scaledHeight, 0,  0, 1),
                          new Vertex(scaledWidth, 0, 0,             0, 0));

            _renderer.material.mainTexture = texture;

            if (recalculateBounds)
                _mesh.RecalculateBounds();

            // we need to set this again, otherwise the collider won't notice the changes
            _collider.sharedMesh = _mesh;
            _id = id;
        }

        /// <summary>
        /// Sets the current mesh vertices
        /// </summary>
        void ApplyVertices(params Vertex[] vertices)
        {
            using (NativeArray<Vertex> nvertices = new NativeArray<Vertex>(vertices, Allocator.Temp))
                _mesh.SetVertexBufferData(nvertices, 0, 0, vertices.Length);
        }
    }
}
