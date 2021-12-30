using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Source.Game.Map.Statics
{
    public class StaticTileMap : MonoBehaviour
    {
        public static StaticTileMap Instance { get; private set; }

        [SerializeField] Tilemap _tilemap;


        [SerializeField] Vector3Int _1Position;
        [SerializeField] uint _1Id;
        [SerializeField] bool _1Apply;

        Dictionary<uint, TileBase> _statics;

        public StaticTileMap()
        {
            Instance = this;
            _statics = new Dictionary<uint, TileBase>();
        }

        public void SetStatic(int x, int y, int z, uint staticId)
        {
            if (!_statics.TryGetValue(staticId, out TileBase st))
                AddStatic(staticId, out st);

            if (st == null)
                return;

            Vector3Int pos = new Vector3Int(y, x, z * 10);
            // try to fit tile into another tile, for this we have to create the illusion that the 2 tiles are the same
            // our height is height * 10, so we 9 height levels to fit other stuff in
            // might need to increase this in the future
            for (int i = 0; i < 9; i++)
            {
                TileBase tb = _tilemap.GetTile(pos);

                if (tb == null)
                    break;

                Tile tile = tb as Tile;

                if (tile == null || tile.sprite == null)
                    break;

                pos.z++;
            }

            _tilemap.SetTile(pos, st);
        }

        void AddStatic(uint staticId, out TileBase st)
        {
            Texture2D stTex = ClassicUO.IO.Resources.ArtLoader.Instance.GetTexture(staticId);
           
            if (stTex == null)
            {
                st = null;
                return;
            }   
            
            Sprite stSprite = Sprite.Create(stTex, new Rect(0, 0, stTex.width, stTex.height), Vector2.zero);
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = stSprite;

            _statics[staticId] = st = tile;
        }

        void Update()
        {
            if (!_1Apply)
                return;

            _1Apply = false;

            SetStatic(_1Position.x, _1Position.y, _1Position.z, _1Id);
        }
    }
}
