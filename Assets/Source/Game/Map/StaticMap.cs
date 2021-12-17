using Assets.Source.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    public class StaticMap
    {
        public int Width { get; private set; }
        public int Depth { get; private set; }
        public int BlockWidth { get; private set; }
        public int BlockDepth { get; private set; }

        Dictionary<Vector2, List<GameObject>> _statics;
        MapStatics _mapStatics;
        GameObject _lastSelectedStatic;

        public StaticMap()
        {
            _statics = new Dictionary<Vector2, List<GameObject>>();
        }

        public void LoadMap(string staticMulFile, string staticIdxFile, int width, int depth)
        {
            Width = width;
            Depth = depth;
            BlockWidth = width / 8;
            BlockDepth = depth / 8;

            _mapStatics = new MapStatics();
            _mapStatics.Load(staticMulFile, staticIdxFile, Width, Depth);
        }

        public void SetSelectedStatic(GameObject st)
        {
            if (_lastSelectedStatic != null)
            {
                if (st.Equals(_lastSelectedStatic))
                    return;

                _lastSelectedStatic.GetComponent<MeshRenderer>().material.SetInt("_EnableSelectedRendering", 0);
            }

            st.GetComponent<MeshRenderer>().material.SetInt("_EnableSelectedRendering", 1);
            _lastSelectedStatic = st;
        }

        public void UnsetSelectedStatic()
        {
            if (_lastSelectedStatic == null)
                return;

            _lastSelectedStatic.GetComponent<MeshRenderer>().material.SetInt("_EnableSelectedRendering", 0);
        }

        public void SpawnStatics()
        {
            foreach (var statics in _statics.Values)
            {
                StaticPool.ReturnRange(statics, true);
            }

            //for (int bx = 56; bx < 75; bx++)
            for (int bx = 0; bx < BlockWidth; bx++)
            {
                int wx = bx * 8;

                //for (int bz = 112; bz < 138; bz++)
                for (int bz = 0; bz < BlockDepth; bz++)
                {
                    int wz = bz * 8;
                    StaticBlock sb = _mapStatics.GetStaticBlock(wx, wz);

                    if (sb == null)
                        continue;

                    for (int i = 0; i < sb.Statics.Length; i++)
                    {
                        ref var st = ref sb.Statics[i];

                        uint id = st.TileId;
                        byte y = st.Y;
                        sbyte z = st.Z;
                        byte x = st.X;

                        Task.Run(() =>
                        {
                            SpawnStatic(id, new Vector3(wx + y, z * .1f, wz + x));
                        }).ConfigureAwait(false);
                    }
                }
            }
        }

        void SpawnStatic(uint staticId, Vector3 pos)
        {
            Texture2D staticTex = null;
            GameObject staticObj = null;
            SpriteRenderer renderer = null;

            Threading.UThreading.Invoke(() =>
            {
                staticTex = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(staticId);
                staticObj = StaticPool.Rent(true);
                renderer = staticObj.GetComponent<SpriteRenderer>();


                staticObj.name = staticId.ToString();

                Rect spriteRect = new Rect(0, 0, staticTex.width, staticTex.height);
                Vector2 spritePivot = new Vector2(0, 0);

                renderer.sprite = Sprite.Create(staticTex, spriteRect, spritePivot, staticTex.width);
                renderer.spriteSortPoint = SpriteSortPoint.Pivot;

                staticObj.transform.position = new Vector3(pos.x + 2f, pos.y + .48f, pos.z + .91f);
                staticObj.transform.rotation = Quaternion.Euler(new Vector3(90, 225, 0));
                staticObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            });

            Vector2 staticDictPos = new Vector2(pos.x, pos.z);

            if (!_statics.TryGetValue(staticDictPos, out List<GameObject> statics))
                _statics.Add(staticDictPos, statics = new List<GameObject>(100));

            statics.Add(staticObj);
        }
        //void SpawnStatic(uint staticId, Vector3 pos)
        //{
        //    Texture2D staticTex = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(staticId);

        //    GameObject staticObj = StaticPool.Rent(true);
        //    staticObj.name = staticId.ToString();

        //    Rect spriteRect = new Rect(0, 0, staticTex.width, staticTex.height);
        //    Vector2 spritePivot = new Vector2(0, 0);

        //    SpriteRenderer renderer = staticObj.GetComponent<SpriteRenderer>();
        //    renderer.sprite = Sprite.Create(staticTex, spriteRect, spritePivot, staticTex.width);
        //    renderer.spriteSortPoint = SpriteSortPoint.Pivot;

        //    staticObj.transform.position = new Vector3(pos.x + 2f, pos.y + .48f, pos.z + .91f);
        //    staticObj.transform.rotation = Quaternion.Euler(new Vector3(90, 225, 0));
        //    staticObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        //    Vector2 staticDictPos = new Vector2(pos.x, pos.z);

        //    if (!_statics.TryGetValue(staticDictPos, out List<GameObject> statics))
        //        _statics.Add(staticDictPos, statics = new List<GameObject>(100));

        //    statics.Add(staticObj);
        //}
        
        //void SpawnStatic(uint staticId, Vector3 pos)
        //{
        //    Texture2D staticTex = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(staticId);

        //    GameObject staticObj = StaticPool.Rent(true);
        //    staticObj.name = staticId.ToString();
        //    Mesh mesh = staticObj.GetComponent<MeshFilter>().mesh = new Mesh();

        //    mesh.SetVertexBufferParams(_imageVerts.Length, VertexLayout.Layout);
        //    mesh.SetVertexBufferData(_imageVerts, 0, 0, _imageVerts.Length);
        //    mesh.SetIndices(_imageInds, MeshTopology.Triangles, 0);
        //    //mesh.SetVertexBufferParams(entry.Vertices.Length, VertexLayout.Layout);
        //    //mesh.SetVertexBufferData(entry.Vertices, 0, 0, entry.Vertices.Length);
        //    //mesh.SetIndices(entry.Indices, MeshTopology.Triangles, 0);
        //    mesh.RecalculateBounds();

        //    MeshRenderer renderer = staticObj.GetComponent<MeshRenderer>();
        //    renderer.material = new Material(Client.Instance.DefaultStaticMaterial);

        //    renderer.material.mainTexture = staticTex;
        //    staticObj.transform.position = new Vector3(pos.x + 2f, pos.y + .49f, pos.z + .91f);
        //    staticObj.transform.rotation = Quaternion.Euler(new Vector3(0, -138, 0));
        //    staticObj.transform.localScale = new Vector3(staticTex.width / 29.333333333f, 0, staticTex.height / 32.5f); // TODO: fix these values someday

        //    MeshCollider collider = staticObj.GetComponent<MeshCollider>();
        //    // Force rebuild of collider
        //    collider.sharedMesh = mesh;

        //    Vector2 staticDictPos = new Vector2(pos.x, pos.z);

        //    if (!_statics.TryGetValue(staticDictPos, out List<GameObject> statics))
        //        _statics.Add(staticDictPos, statics = new List<GameObject>(100));

        //    statics.Add(staticObj);
        //}

        //void SpawnStatic(uint staticId, Vector3 pos)
        //{
        //    StaticCacheEntry entry = StaticCache.Get(staticId);

        //    if (entry == null)
        //        return;

        //    GameObject staticObj = StaticPool.Rent(true);
        //    staticObj.name = staticId.ToString();
        //    Mesh mesh = staticObj.GetComponent<MeshFilter>().mesh = new Mesh();

        //    mesh.SetVertexBufferParams(entry.Vertices.Length, VertexLayout.Layout);
        //    mesh.SetVertexBufferData(entry.Vertices, 0, 0, entry.Vertices.Length);
        //    mesh.SetIndices(entry.Indices, MeshTopology.Triangles, 0);
        //    mesh.RecalculateBounds();

        //    MeshRenderer renderer = staticObj.GetComponent<MeshRenderer>();
        //    renderer.material = new Material(Client.Instance.DefaultStaticMaterial);
        //    renderer.material.mainTexture = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(staticId);
        //    staticObj.transform.position = pos;

        //    MeshCollider collider = staticObj.GetComponent<MeshCollider>();
        //    // Force rebuild of collider
        //    collider.sharedMesh = mesh;

        //    Vector2 staticDictPos = new Vector2(pos.x, pos.z);

        //    if (!_statics.TryGetValue(staticDictPos, out List<GameObject> statics))
        //        _statics.Add(staticDictPos, statics = new List<GameObject>(100));

        //    statics.Add(staticObj);
        //}
    }
}
