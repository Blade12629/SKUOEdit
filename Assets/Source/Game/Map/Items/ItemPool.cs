using System;
using UnityEngine;
using Assets.Source.Utility;

namespace Assets.Source.Game.Map.Items
{
    public class ItemPool : SimplePool<Item>
    {
        static readonly Quaternion _defaultRotation = Quaternion.Euler(-65f, 45.5f, 0.5f);
        static readonly Vector3 _placeholderPos = new Vector3(-10000f, -10000f, -10000f);

        Material _material;

        public ItemPool(Material itemMaterial) : base()
        {
            _material = itemMaterial;
        }

        protected override Item CreateObject()
        {
            GameObject gobj = new GameObject();
            Item item = gobj.AddComponent<Item>();
            item.Initialize(_material);

            gobj.transform.position = _placeholderPos;
            gobj.transform.rotation = _defaultRotation;

            return item;
        }

        protected override void OnReleased(Item obj)
        {
            obj.gameObject.transform.position = _placeholderPos;
        }

        protected override void OnRented(Item obj)
        {

        }

        protected override void DestroyObject(Item obj)
        {
            GameObject.Destroy(obj.gameObject);
        }
    }
}
