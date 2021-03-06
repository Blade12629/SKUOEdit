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

        public void RefreshItems()
        {
            _items.Clear();

            int bs = (int)Math.Ceiling(_size / 8.0);
            int bx = (int)Math.Ceiling(_position.x / 8.0);
            int by = (int)Math.Ceiling(_position.y / 8.0);

            if (bx < 0)
                bx = 0;
            if (by < 0)
                by = 0;

            int bxe = Math.Min(bx + bs, _statics.BlockWidth);
            int bye = Math.Min(by + bs, _statics.BlockDepth);

            for (int blockX = bx; blockX < bxe; blockX++)
            {
                int wx = blockX * 8;

                for (int blockY = by; blockY < bye; blockY++)
                {
                    int wy = blockY * 8;

                    StaticBlock block = _statics.GetStaticBlock(blockX * 8, blockY * 8);

                    if (block == null || block.Statics.Length == 0)
                        continue;

                    Static[] statics = block.Statics;

                    for (int i = 0; i < statics.Length; i++)
                    {
                        ref Static st = ref statics[i];

                        Vector3 pos = new Vector3(wx + st.X, st.Z * 0.039f + 0.0001f, wy + st.Y);

                        _items.SpawnItem(pos, st.TileId);
                    }
                }
            }
        }
    }
}
