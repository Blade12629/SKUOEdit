using Assets.Source.IO;
using System;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    public class ItemMap : MonoBehaviour
    {
        int _size;
        Vector3 _position;
        ItemLookup _items;
        MapStatics _statics;

        public void Initialize(int size, Material material, MapStatics statics)
        {
            _size = size;
            _items = new ItemLookup();
            _items.Initialize(material);
            _statics = statics;
        }

        public void MoveToWorld(Vector3 pos)
        {
            _position = pos;
            RefreshItems();
        }

        public void SpawnTestItems()
        {
            for (int x = 5; x < 10; x++)
            {
                for (int y = 5; y < 10; y++)
                {
                    _items.SpawnItem(new Vector3(y + .5f, x + .5f, 0), 1982, 0);
                }
            }

            for (int x = 1; x < 5; x++)
            {
                for (int y = 1; y < 5; y++)
                {
                    _items.SpawnItem(new Vector3(y + .5f, x + .5f, 0), 1928, 0);
                }
            }

            for (int x = 0; x < 10; x++)
            {
                _items.SpawnItem(new Vector3(10.5f, x + .5f, 0), 2171, 0);
            }

            for (int x = 15; x < 18; x++)
            {
                for (int y = 15; y < 18; y++)
                {
                    _items.SpawnItem(new Vector3(y + .5f, x + .5f, 0), 7855, 0);
                }
            }
        }

        public void RefreshItems()
        {
            _items.Clear();

            int spawned = 0;
            int spawnedError = 0;

            int bs = (int)Math.Ceiling(_size / 8.0);
            int bx = (int)Math.Ceiling(_position.x / 8.0);
            int by = (int)Math.Ceiling(_position.y / 8.0);

            if (bx < 0)
                bx = 0;
            if (by < 0)
                by = 0;

            int bxe = Math.Min(bx + bs, _statics.BlockWidth);
            int bye = Math.Min(by + bs, _statics.BlockDepth);

            for (int blockX = bxe - 1; blockX >= bx; blockX--)
            //for (int blockX = bx; blockX < bxe; blockX++)
            {
                int wx = blockX * 8;

                for (int blockY = bye - 1; blockY >= by; blockY--)
                //for (int blockY = by; blockY < bye; blockY++)
                {
                    int wy = blockY * 8;

                    StaticBlock block = _statics.GetStaticBlock(blockX * 8, blockY * 8);

                    if (block == null || block.Statics.Length == 0)
                        continue;

                    Static[] statics = block.Statics;

                    for (int i = 0; i < statics.Length; i++)
                    {
                        ref Static st = ref statics[i];

                        Vector3 pos = new Vector3(wx + st.Y,
                                                  wy + st.X, 
                                                  -(st.Z * MapConstants.TILE_HEIGHT_MULTIPLIER + MapConstants.TILE_HEIGHT_OFFSET));

                        if (!_items.SpawnItem(pos, st.TileId, st.Z))
                            spawnedError++;
                        else
                            spawned++;
                    }
                }
            }
        }
    }
}
