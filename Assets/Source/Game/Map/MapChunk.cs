using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    [RequireComponent(typeof(MeshCollider), typeof(MeshRenderer), typeof(MeshFilter))]
    public class MapChunk : MonoBehaviour
    {
        public static readonly int DefaultMeshSize = 64;

        public int Size { get; private set; }
        public Point Offset { get; private set; }
        public bool IsRenderingGrid
        {
            get => _renderer.material.GetInt("_DrawGrid") == 1;
            set => _renderer.material.SetInt("_DrawGrid", value ? 1 : 0);
        }
        public bool IsRenderingSelection
        {
            get => _renderer.material.GetInt("_EnableSelectedRendering") == 1;
            set => _renderer.material.SetInt("_EnableSelectedRendering", value ? 1 : 0);
        }
        public Color GridColor
        {
            get => _renderer.material.GetColor("_GridColor");
            set => _renderer.material.SetColor("_GridColor", value);
        }
        public float GridSize
        {
            get => _renderer.material.GetFloat("_GridSize");
            set
            {
                if (value < 0.0005f)
                    throw new InvalidOperationException("Gridsize cannot be below 0.0005");

                _renderer.material.SetFloat("_GridSize", value);
            }
        }
        public Point SelectedTile
        {
            get
            {
                Vector4 pos = _renderer.material.GetVector("_SelectedPos");
                return new Point(pos.x + Offset.X, pos.z + Offset.Z);
            }
            set
            {
                _renderer.material.SetVector("_SelectedPos", new Vector4(value.X - Offset.X, value.Z - Offset.Z));
            }
        }

        MeshCollider _collider;
        MeshRenderer _renderer;
        MeshFilter _filter;
        Mesh _mesh;

        Vertex[] _vertexCache;

        /// <summary>
        /// Initializes the mesh, this method has to be called before anything else
        /// </summary>
        /// <param name="material">Mesh material</param>
        /// <param name="atlasTexture">Mesh texture</param>
        public void Initialize(Material material, Texture2D atlasTexture)
        {
            _collider = GetComponent<MeshCollider>();
            _renderer = GetComponent<MeshRenderer>();
            _filter = GetComponent<MeshFilter>();

            _mesh = _filter.mesh = new Mesh();
            _renderer.material = new Material(material);
            _renderer.material.mainTexture = atlasTexture;

            Size = DefaultMeshSize;
            _vertexCache = new Vertex[Size * Size * 4];
            _mesh.SetVertexBufferParams(_vertexCache.Length, VertexLayout.Layout);
        }

        /// <summary>
        /// Offsets the mesh to a specific point
        /// </summary>
        public void MoveToWorld(Point offset)
        {
            Offset = offset;
            Build();
        }

        /// <summary>
        /// Resizes the mesh
        /// </summary>
        /// <param name="size"></param>
        public void Resize(int size)
        {
            if (size <= 1)
                return;

            Size = size;
            Array.Resize(ref _vertexCache, Size * Size * 4);
            Build();
        }

        /// <summary>
        /// Clears and then builds the mesh
        /// </summary>
        public void Build()
        {
            try
            {
                Clear();

                _mesh.SetVertexBufferParams(_vertexCache.Length, VertexLayout.Layout);

                ApplyVertices();
                ApplyIndices();
                _mesh.RecalculateBounds();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// Rebuilds the mesh without clearing it
        /// </summary>
        public void RebuildVertices()
        {
            try
            {
                ApplyVertices();
                _mesh.RecalculateBounds();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                throw;
            }
        }

        /// <summary>
        /// Clears the mesh
        /// </summary>
        public void Clear()
        {
            _mesh.Clear();
        }

        public bool IsInBounds(Point p)
        {
            return p.X >= Offset.X && p.X < Offset.X + Size &&
                   p.Z >= Offset.Z && p.Z < Offset.Z + Size;
        }

        public void SetGridColor(Color color)
        {
            _renderer.material.SetColor("_GridColor", color);
        }

        public void SetGridSize(float size)
        {
            _renderer.material.SetFloat("_GridSize", size);
        }

        public void SetSelectedTile(int x, int z)
        {
            _renderer.material.SetVector("_SelectedPos", new Vector4(x, 0, z, 1));
        }

        public void SetSelectionSize(int size)
        {
            _renderer.material.SetInt("_SelectedAreaSize", size);
        }

        void ApplyVertices()
        {
            GameMap.Instance.CopyAreaVertices(_vertexCache, Offset.X, Offset.Z, Size, Size);

            using (NativeArray<Vertex> nvertices = new NativeArray<Vertex>(_vertexCache, Allocator.Temp))
            {
                _mesh.SetVertexBufferData(nvertices, 0, 0, nvertices.Length);
            }
        }

        void ApplyIndices()
        {
            int[] indices = new int[Size * Size * 6];

            int iv = 0;
            int i = 0;
            for (int x = 0; x < Size; x++)
            {
                for (int z = 0; z < Size; z++)
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

            _mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }
    }
}
