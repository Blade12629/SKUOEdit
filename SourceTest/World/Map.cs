using Assets.Source.Game.Map;
using Assets.Source.Game.Map.Statics;
using Assets.Source.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.SourceTest.World
{
    public class Map : MonoBehaviour
    {
        [SerializeField] TerrainMesh _terrain;
        [SerializeField] StaticMesh _statics;
        //[SerializeField] StaticTilemap _statics;
        //[SerializeField] StaticTileMap _actualStatics;

        int _renderedSize;

        MapStatics _mapStatics;

        [SerializeField] int _defaultOffsetX;
        [SerializeField] int _defaultOffsetY;
        [SerializeField] bool _defaultOffsetApply;


        void Update()
        {
            if (!_defaultOffsetApply)
                return;

            _defaultOffsetApply = false;
            _terrain.MoveToOffset(_defaultOffsetX, _defaultOffsetY);
        }

        /// <summary>
        /// Initializes the map
        /// </summary>
        /// <param name="textures">Game textures</param>
        public void Initialize(int renderedSize)
        {
            _renderedSize = renderedSize;
        }

        /// <summary>
        /// Loads a map, can be uop or mul
        /// </summary>
        /// <param name="path">Map path</param>
        /// <param name="width">Map width</param>
        /// <param name="height">Map height</param>
        public void LoadMap(string path, int width, int height)
        {
            Clear();

            Source.IO.MapTiles mapTiles = new Source.IO.MapTiles();
            
            if (path.EndsWith("uop", StringComparison.CurrentCultureIgnoreCase))
            {
                mapTiles.Load(path, true, width, height);
            }
            else
            {
                mapTiles.Load(path, false, width, height);
            }

            int index = GetMapIndex(path);
            string dir = new System.IO.FileInfo(path).Directory.FullName;

            _mapStatics = new MapStatics();
            _mapStatics.Load(System.IO.Path.Combine(dir, $"statics{index}.mul"), System.IO.Path.Combine(dir, $"staidx{index}.mul"), width, height);

            //_statics.RenderArea(new Rect(1150, 500, 200, 200), _mapStatics);

            // TODO: rework statics, use mesh instead of 2d tilemap so we achieve the full 3d render effect
            // 0.44 on isometric tilemap equals roughly 0.31 on a rectangle tilemap (non isometric)
            int blockWidth = width / 8;
            int blockHeight = height / 8;

            for (int bx = 0; bx < blockWidth; bx++)
            {
                int wx = bx * 8;

                for (int by = 0; by < blockHeight; by++)
                {
                    int wy = by * 8;
                    StaticBlock sb = _mapStatics.GetStaticBlock(wy, wx);

                    if (sb == null)
                        continue;

                    for (int i = 0; i < sb.Statics.Length; i++)
                    {
                        ref var st = ref sb.Statics[i];

                        uint id = st.TileId;
                        byte y = st.Y;
                        sbyte z = st.Z;
                        byte x = st.X;

                        StaticTileMap.Instance.SetStatic(wx + x, wy + y + 1, -z, id);
                    }
                }
            }

            _terrain.BuildCache(mapTiles);
            _terrain.BuildMesh(_renderedSize, _renderedSize, 0, 0);

            WorldCamera.OnCameraMoved += e =>
            {
                _terrain.MoveToOffset((int)e.NewPosition.x, (int)e.NewPosition.y);
            };
        }

        /// <summary>
        /// Saves the map, currently only possible as .mul
        /// </summary>
        /// <param name="path">Map path</param>
        public void SaveMap(string path)
        {

        }

        /// <summary>
        /// Clears all tiles and statics from the map
        /// </summary>
        public void Clear()
        {

        }

        int GetMapIndex(string fileName)
        {
            StringBuilder sb = new StringBuilder(2);

            bool wasNumber = false;
            for (int i = fileName.Length - 1; i >= 0; i--)
            {
                char c = fileName[i];

                if (!char.IsNumber(c))
                {
                    if (wasNumber)
                        break;
                }
                else
                {
                    wasNumber = true;

                    if (sb.Length == 0)
                        sb.Append(c);
                    else
                        sb.Insert(0, c);
                }
            }

            return int.Parse(sb.ToString());
        }
    }
}
