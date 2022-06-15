using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    public class ItemLookup
    {
        Dictionary<Vector3, List<ItemLookupEntry>> _entries;

        public ItemLookup()
        {
            _entries = new Dictionary<Vector3, List<ItemLookupEntry>>();
        }

        public List<ItemLookupEntry> this[Vector3 position]
        {
            get => GetEntries(position);
        }

        public List<ItemLookupEntry> GetEntries(Vector3 position)
        {
            List<ItemLookupEntry> res;
            _entries.TryGetValue(position, out res);

            return res;
        }

        /// <summary>
        /// Adds a new entry
        /// </summary>
        /// <returns>False if entry with <see cref="ItemLookupEntry.ItemId"/> already exists at <paramref name="position"/></returns>
        public bool AddEntry(Vector3 position, ItemLookupEntry entry)
        {
            List<ItemLookupEntry> entries = GetEntries(position);

            if (entries == null)
                entries = _entries[position] = new List<ItemLookupEntry>();

            if (Find(entry.ItemId, entries) != null)
                return false;

            entries.Add(entry);
            return true;
        }

        /// <summary>
        /// Equivalent to calling <see cref="FirstOrNull(uint)"/>
        /// </summary>
        public ItemLookupEntry Find(uint itemId)
        {
            return FirstOrNull(itemId);
        }

        /// <summary>
        /// Searches for the first entry with the specified <paramref name="itemId"/>
        /// </summary>
        /// <returns>First entry or null if not found</returns>
        public ItemLookupEntry FirstOrNull(uint itemId)
        {
            foreach (var entries in _entries.Values)
            {
                ItemLookupEntry entry = Find(itemId, entries);

                if (entry != null)
                    return entry;
            }

            return null;
        }

        static ItemLookupEntry Find(uint itemId, List<ItemLookupEntry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                ItemLookupEntry entry = entries[i];

                if (entry.ItemId == itemId)
                    return entry;
            }

            return null;
        }
    }
}
