using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ImageMesh : MonoBehaviour
    {
        [SerializeField] Texture2D _imgTex;
        [SerializeField] Material _imgMat;
        [SerializeField] GameObject _lookAt;

        MeshFilter _filter;
        MeshRenderer _renderer;

        int _oldTexId = -1;

        void Awake()
        {
            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();

            _filter.mesh = new Mesh();
        }

        void Update()
        {
            //if (_lookAt != null)
            //    transform.rotation = _lookAt.transform.rotation;

            if (_imgTex == null || _oldTexId == _imgTex.GetInstanceID())
                return;

            _filter.mesh.SetVertices(new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 1),
                new Vector3(1, 0, 0),
            });

            _filter.mesh.SetIndices(new int[]
            {
                0, 1, 2,
                0, 2, 3
            }, MeshTopology.Triangles, 0);

            _filter.mesh.SetUVs(0, new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
            });

            _renderer.material = new Material(_imgMat);
            _renderer.material.mainTexture = _imgTex;
        }
    }
}
