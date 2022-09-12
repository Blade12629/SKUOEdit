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

        public string TotalString()
        {
            return $"Total: {_itemsPool.Total}, Available: {_itemsPool.Available}, Rented {_itemsPool.Rented}, ";
        }

        public bool SpawnItem(Vector3 position, uint id, int height)
        {
            if (ContainsItem(position, id, height))
                return false;

            Item item = _itemsPool.Rent();

            PrepareItem(item, id, height, position);
            AddItem(position, item);

            return true;
        }

        public bool TryGetItems(Vector3 position, out List<Item> items)
        {
            items = new List<Item>();

            if (_items.TryGetValue(ToVector2(position), out List<Item> tempItems))
            {
                items.AddRange(tempItems);
                return true;
            }

            items = null;
            return false;
        }

        public bool ContainsItem(Vector3 position, uint id)
        {
            return ContainsItem(position, i => i.ItemId == id && i.transform.position.Equals(position));
        }

        public bool ContainsItem(Vector3 position, uint id, int height)
        {
            return ContainsItem(position, i => i.ItemId == id && i.Height == height);
        }

        public bool ContainsItem(Vector3 position, Func<Item, bool> predicate)
        {
            if (TryGetItems(position, out List<Item> items))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (!predicate(items[i]))
                        continue;

                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            int cleared = 0;

            foreach (KeyValuePair<Point2, List<Item>> pair in _items)
            {
                foreach (Item item in pair.Value)
                {
                    _itemsPool.Release(item);
                    cleared++;
                }
            }

            _items.Clear();
        }

        void PrepareItem(Item item, uint id, int height, Vector3 pos)
        {
            item.ItemId = id;
            item.Height = height;
            item.transform.position = pos;
        }

        void AddItem(Vector3 position, Item item)
        {
            List<Item> items = AddOrGet(ToVector2(position));
            items.Add(item);
        }

        List<Item> AddOrGet(Point2 position)
        {
            List<Item> items;

            if (_items.TryGetValue(position, out items))
                return items;

            items = new List<Item>();
            _items.Add(position, items);

            return items;
        }

        Vector2 ToVector2(Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        Vector3 ToVector3(Vector2 v)
        {
            return new Vector3(v.x, 0f, v.y);
        }
    }
}
