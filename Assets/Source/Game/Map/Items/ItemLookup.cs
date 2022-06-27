using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    public class ItemLookup
    {
        Dictionary<Vector3, Item> _items;
        ItemPool _itemPool;

        public Item this[Vector3 pos]
        {
            get => GetItem(pos);
        }

        public void Initialize(Material itemMaterial)
        {
            _items = new Dictionary<Vector3, Item>(64);
            _itemPool = new ItemPool(itemMaterial);
        }

        /// <summary>
        /// Searches the item map, this uses O(n) time complexity
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public List<Item> Search(Func<Item, bool> predicate)
        {
            List<Item> items = new List<Item>();

            foreach (Item item in _items.Values)
                if (predicate(item))
                    items.Add(item);

            return items;
        }

        public List<Item> ToList()
        {
            return _items.Values.ToList();
        }

        public bool Update(Vector3 old, Vector3 @new)
        {
            if (TryGetItem(old, out Item item) && !ContainsItem(@new))
            {
                _items.Remove(old);
                _items[@new] = item;

                item.transform.position = @new;

                return true;
            }

            return false;
        }

        public bool AddItem(Vector3 pos, uint itemId)
        {
            if (_items.ContainsKey(pos))
                return false;

            Item item = _itemPool.Rent();
            item.ItemId = itemId;
            item.transform.position = pos;
            _items[pos] = item;

            return true;
        }

        public bool TryGetItem(Vector3 pos, out Item item)
        {
            item = GetItem(pos);
            return item != null;
        }

        public Item GetItem(Vector3 pos)
        {
            if (_items.TryGetValue(pos, out Item item))
                return item;

            return null;
        }

        public bool ContainsItem(Vector3 pos)
        {
            return _items.ContainsKey(pos);
        }

        public bool RemoveItem(Vector3 pos)
        {
            if (TryGetItem(pos, out Item item))
            {
                _itemPool.Release(item);
                _items.Remove(pos);
                return true;
            }

            return false;
        }
    }
}
