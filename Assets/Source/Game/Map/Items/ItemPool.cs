using System;
using UnityEngine;
using Assets.Source.Utility;

namespace Assets.Source.Game.Map.Items
{
    public class ItemPool : SimplePool<Item>
    {
        Material _material;

        public ItemPool(Material itemMaterial, int startCapacity = 16) : base(startCapacity)
        {
            _material = itemMaterial;
        }

        protected override Item CreateObject()
        {
            GameObject gobj = new GameObject();
            Item item = gobj.AddComponent<Item>();
            item.Initialize(_material);

            gobj.transform.position = new Vector3(-10000f, -10000f, -10000f);
            gobj.transform.rotation = Quaternion.Euler(-45f, -45f, 60f);

            return item;
        }

        protected override void OnReleased(Item obj)
        {
            obj.transform.position = new Vector3(-10000f, -10000f, -10000f);
        }

        protected override void OnRented(Item obj)
        {
            obj.transform.position = Vector3.zero;
        }
    }
}
