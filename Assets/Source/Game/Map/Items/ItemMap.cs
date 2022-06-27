using Assets.Source.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    public class ItemMap : MonoBehaviour
    {
        int _size;
        Vector3 _position;
        ItemLookup _lookup;
        MapStatics _statics;

        public void Initialize(int size, Material material, MapStatics statics)
        {
            _size = size;
            _lookup = new ItemLookup();
            _lookup.Initialize(material);
            _statics = statics;
        }

        public void RefreshItems()
        {
            List<Item> items = _lookup.ToList();

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

                        Vector3 pos = new Vector3(wx + st.X, wy + st.Y, -(st.Z * 0.039f));

                        // currently we do not support multiple items on the same position
                        // this will change at a later time
                        // TODO: allow multiple items on the same position
                        if (_lookup.TryGetItem(pos, out Item origItem))
                        {
                            System.IO.File.AppendAllText("Test/1111_itemSlotDuplicates.txt", $"Orig id: {origItem.ItemId}, New id: {st.TileId}, Pos: {pos}");
                            continue;
                        }

                        try
                        {
                            if (items.Count > 0)
                            {
                                Item item = items[0];
                                items.RemoveAt(0);

                                item.ItemId = st.TileId;
                                _lookup.Update(item.transform.position, pos);
                            }
                            else
                            {
                                _lookup.AddItem(pos, st.TileId);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex);
                            return;
                        }
                    }
                }
            }

            // remove unused items
            for (int i = 0; i < items.Count; i++)
                _lookup.RemoveItem(items[i].transform.position);
        }
    }
}
