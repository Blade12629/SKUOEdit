using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Assets.Source.Game.Statics
{
    public static class StaticBuilder
    {
        static Dictionary<int, StaticInfo> _statics = new Dictionary<int, StaticInfo>();

        public static StaticInfo AddOrGetStatic(int id)
        {
            if (_statics.TryGetValue(id, out StaticInfo info))
                return info;

            return AddStatic(id);
        }

        public static StaticInfo AddStatic(int id)
        {
            return null;
        }

        public static Item BuildGameStatic(int id)
        {
            StaticInfo staticInfo = AddOrGetStatic(id);

            if (staticInfo == null)
                return null;

            GameObject itemObj = new GameObject($"Static {id}", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(Item));
            Item item = itemObj.GetComponent<Item>();

            item.GenerateItemMesh(staticInfo);
            return item;
        }
    }
}
