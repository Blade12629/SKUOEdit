using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Assets.Source.Game.Map
{
    public class ItemMap
    {
        public int Width { get; }
        public int Height { get; }

        Dictionary<Vector2, ItemBlock> _items;

        public ItemMap(int width, int height)
        {
            Width = width;
            Height = height;
            _items = new Dictionary<Vector2, ItemBlock>();
        }

        public ItemBlock this[Vector2 pos]
        {
            get
            {
                ItemBlock block;
                _items.TryGetValue(pos, out block);

                return block;
            }
        }

        public ItemBlock AddOrGet(Vector2 pos)
        {
            ItemBlock block = this[pos];

            if (block != null)
                return block;

            _items[pos] = block = new ItemBlock(pos);
            return block;
        }
    }

    public class ItemBlock
    {
        public Vector2 Position { get; }
        public List<ItemInfo> Items { get; }
        public int Count => Items.Count;

        public ItemBlock(Vector2 pos)
        {
            Position = pos;
            Items = new List<ItemInfo>(32);
        }

        public ItemInfo this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        /// <summary>
        /// Searches for the index of a specific <see cref="ItemInfo"/>
        /// </summary>
        /// <returns>Index or -1 if not found</returns>
        public int Find(Func<ItemInfo, bool> predicate)
        {
            for (int i = 0; i < Items.Count; i++)
                if (predicate(Items[i]))
                    return i;

            return -1;
        }

        public bool Contains(uint itemId)
        {
            return Find(i => i.Id == itemId) != -1;
        }

        public bool Contains(ItemInfo info)
        {
            return Contains(info.Id, info.Z);
        }

        public bool Contains(uint itemId, float z)
        {
            return Find(i => i.Id == itemId && i.Z == z) != -1;
        }

        /// <summary>
        /// Adds a new <see cref="ItemInfo"/> and checks for duplicates
        /// </summary>
        /// <returns>Item was added</returns>
        public bool Add(ItemInfo info)
        {
            return Add(info, true);
        }

        public bool Add(ItemInfo info, bool checkForDuplicate)
        {
            if (checkForDuplicate && Contains(info))
                return false;

            Items.Add(info);
            return true;
        }

        public void AddOrUpdate(ItemInfo info)
        {
            int index = Find(i => i.Equals(info));

            if (index == -1)
            {
                Add(info, false);
                return;
            }

            Items[index] = info;
        }

        public bool Remove(int index)
        {
            if (index >= Items.Count || index < 0)
                return false;

            Items.RemoveAt(index);
            return true;
        }

        public bool Remove(uint itemId, float z)
        {
            int index = Find(i => i.Id == itemId && i.Z == z);
            return Remove(index);
        }

        public bool Remove(ItemInfo info)
        {
            return Remove(info.Id, info.Z);
        }

        public void RemoveAll(float z)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Z == z)
                {
                    Items.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public struct ItemInfo
    {
        public uint Id;
        public float Z;
        public int VertexStart;

        public override bool Equals(object obj)
        {
            return obj is ItemInfo info &&
                   Id == info.Id &&
                   Z == info.Z &&
                   VertexStart == info.VertexStart;
        }

        public override int GetHashCode()
        {
            int hashCode = 1556005453;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            hashCode = hashCode * -1521134295 + VertexStart.GetHashCode();
            return hashCode;
        }
    }
}
