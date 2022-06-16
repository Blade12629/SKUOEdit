using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Source.Game.Map.Items
{
    public class ItemLookup : IEquatable<ItemLookup>
    {
        public Vector3 Position { get; set; }
        public uint ItemId { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }

        public ItemLookup(Vector3 position, uint itemId, int index, int length)
        {
            Position = position;
            ItemId = itemId;
            Index = index;
            Length = length;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ItemLookup);
        }

        public bool Equals(ItemLookup other)
        {
            return other != null &&
                   Position.Equals(other.Position) &&
                   ItemId == other.ItemId;
        }

        public override int GetHashCode()
        {
            int hashCode = 1117739637;
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + ItemId.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ItemLookup left, ItemLookup right)
        {
            return EqualityComparer<ItemLookup>.Default.Equals(left, right);
        }

        public static bool operator !=(ItemLookup left, ItemLookup right)
        {
            return !(left == right);
        }
    }
}
