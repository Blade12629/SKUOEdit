using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Source.Geometry;

namespace Assets.Source.Game.Map.Items
{
    public class ItemLookup
    {
        Dictionary<Point2, List<Item>> _items;
        ItemPool _itemsPool;

        public void Initialize(Material material)
        {
            _items = new Dictionary<Point2, List<Item>>();
            _itemsPool = new ItemPool(material);
        }

        public bool SpawnItem(Vector3 position, uint id)
        {
            if (ContainsItem(position, id))
                return false;

            Item item = _itemsPool.Rent();
            PrepareItem(item, id, position);

            if (!TryGetItems(position, out List<Item> items))
                _items[position] = items = new List<Item>();

            items.Add(item);
            return true;
        }

        public bool DeleteItem(Vector3 position, uint id)
        {
            if (TryGetItems(position, out List<Item> items))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!items[i].transform.position.Equals(position) || items[i].ItemId != id)
                        continue;

                    _itemsPool.Release(items[i]);
                    items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool DeleteItems(Vector2 position, Func<Item, bool> predicate)
        {
            bool anyDeleted = false;

            if (TryGetItems(position, out List<Item> items))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!predicate(items[i]))
                        continue;

                    _itemsPool.Release(items[i]);
                    items.RemoveAt(i);
                    i--;
                    anyDeleted = true;
                }
            }

            return anyDeleted;
        }

        public bool TryGetItems(Vector2 position, out List<Item> items)
        {
            items = new List<Item>();

            if (_items.TryGetValue(position, out List<Item> tempItems))
            {
                items.AddRange(tempItems);
                return true;
            }

            items = null;
            return false;
        }

        public List<Item> FindItems(Vector3 position, Func<Item, bool> predicate)
        {
            List<Item> result = new List<Item>();

            if (TryGetItems(position, out List<Item> items))
                for (int i = 0; i < items.Count; i++)
                    if (predicate(items[i]))
                        result.Add(items[i]);

            return result;
        }

        public Item FindItem(Vector3 position, Func<Item, bool> predicate)
        {
            if (TryGetItems(position, out List<Item> items))
                for (int i = 0; i < items.Count; i++)
                    if (predicate(items[i]))
                        return items[i];

            return null;
        }

        public bool ContainsItem(Vector3 position, uint id)
        {
            if (TryGetItems(position, out List<Item> items))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].ItemId != id)
                        continue;

                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            foreach (List<Item> items in _items.Values)
                for (int i = 0; i < items.Count; i++)
                    _itemsPool.Release(items[i]);

            _items.Clear();
        }

        void PrepareItem(Item item, uint id, Vector3 pos)
        {
            item.ItemId = id;
            item.transform.position = pos;
        }
    }
}
