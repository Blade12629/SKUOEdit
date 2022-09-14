using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Source.Geometry;

namespace Assets.Source.Game.Map.Items
{
    public class ItemLookup
    {
        Dictionary<Point2, List<StaticMesh>> _items;
        ItemPool _itemsPool;

        public void Initialize(Material material)
        {
            _items = new Dictionary<Point2, List<StaticMesh>>();
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

            StaticMesh item = _itemsPool.Rent();

            PrepareItem(item, id, height, position);
            AddItem(position, item);

            return true;
        }

        public bool TryGetItems(Vector3 position, out List<StaticMesh> items)
        {
            items = new List<StaticMesh>();

            if (_items.TryGetValue(ToVector2(position), out List<StaticMesh> tempItems))
            {
                items.AddRange(tempItems);
                return true;
            }

            items = null;
            return false;
        }

        public bool ContainsItem(Vector3 position, uint id)
        {
            return ContainsItem(position, i => i.Id == id && i.transform.position.Equals(position));
        }

        public bool ContainsItem(Vector3 position, uint id, int height)
        {
            return ContainsItem(position, i => i.Id == id && i.Height == height);
        }

        public bool ContainsItem(Vector3 position, Func<StaticMesh, bool> predicate)
        {
            if (TryGetItems(position, out List<StaticMesh> items))
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

            foreach (KeyValuePair<Point2, List<StaticMesh>> pair in _items)
            {
                foreach (StaticMesh item in pair.Value)
                {
                    _itemsPool.Release(item);
                    cleared++;
                }
            }

            _items.Clear();
        }

        void PrepareItem(StaticMesh item, uint id, int height, Vector3 pos)
        {
            item.Id = id;
            item.Height = height;
            item.transform.position = pos;
        }

        void AddItem(Vector3 position, StaticMesh item)
        {
            List<StaticMesh> items = AddOrGet(ToVector2(position));
            items.Add(item);
        }

        List<StaticMesh> AddOrGet(Point2 position)
        {
            List<StaticMesh> items;

            if (_items.TryGetValue(position, out items))
                return items;

            items = new List<StaticMesh>();
            _items.Add(position, items);

            return items;
        }

        Vector2 ToVector2(Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        Vector3 ToVector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0f);
        }
    }
}
