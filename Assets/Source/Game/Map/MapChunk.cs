using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    public sealed class MapChunk : MonoBehaviour
    {
        public static int MeshSize = 32;
        public Rect RenderedArea => _renderedArea;

        Rect _renderedArea;
        Vertex[] _vertices;
        Mesh _mesh;
        MeshCollider _collider;
        MeshRenderer _renderer;

        public void BuildChunk(Rect areaToRender)
        {
            _renderedArea = areaToRender;
            _vertices = new Vertex[MeshSize * MeshSize * 4];
            _mesh = GetComponent<MeshFilter>().mesh = new Mesh();
            _collider = GetComponent<MeshCollider>();

            _renderer = GetComponent<MeshRenderer>();
            _renderer.material = new Material(Client.Instance.DefaultMaterial);
            _renderer.material.mainTexture = Textures.GameTextures.GameTexture;

            _mesh.SetVertexBufferParams(_vertices.Length, VertexLayout.Layout);
            RefreshChunk();

            int width = (int)areaToRender.width;
            int depth = (int)areaToRender.height;
            int[] indices = new int[width * depth * 6];

            int iv = 0;
            int i = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
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
            _collider.sharedMesh = _mesh;
        }

        public void DestroyChunk()
        {
            Debug.Log($"Destroying chunk {name}");
            GameObject.Destroy(gameObject);
        }

        public void MoveToWorld(Vector3 newPos)
        {
            _renderedArea = new Rect(newPos.x, newPos.z, _renderedArea.width, _renderedArea.height);
            RefreshChunk();
        }

        public void RefreshChunk()
        {
            GameMap.Instance.CopyAreaVertices(_vertices, (int)_renderedArea.x, (int)_renderedArea.y, (int)_renderedArea.width, (int)_renderedArea.height);

            using (NativeArray<Vertex> nvertices = new NativeArray<Vertex>(_vertices, Allocator.Temp))
            {
                _mesh.SetVertexBufferData(nvertices, 0, 0, nvertices.Length);
            }

            _mesh.RecalculateBounds();
            _collider.sharedMesh = _mesh;
        }

        public bool IsInBounds(int x, int z)
        {
            return _renderedArea.Contains(new Vector2(x, z), false);
        }

        public void EnableGrid()
        {
            _renderer.material.SetInt("_DrawGrid", 1);
        }

        public void DisableGrid()
        {
            _renderer.material.SetInt("_DrawGrid", 0);
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

        public void EnableSelectionRendering()
        {
            _renderer.material.SetInt("_EnableSelectedRendering", 1);
        }

        public void DisableSelectionRendering()
        {
            _renderer.material.SetInt("_EnableSelectedRendering", 0);
        }

        public bool IsGridEnabled()
        {
            return _renderer.material.GetInt("_DrawGrid") != 0;
        }

        public Color GetGridColor()
        {
            return _renderer.material.GetColor("_GridColor");
        }

        public float GetGridSize()
        {
            return _renderer.material.GetFloat("_GridSize");
        }
    }
}
