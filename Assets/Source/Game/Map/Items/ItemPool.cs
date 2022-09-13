using System;
using UnityEngine;
using Assets.Source.Utility;

namespace Assets.Source.Game.Map.Items
{
    public class ItemPool : SimplePool<StaticMesh>
    {
        static readonly Vector3 _placeholderPos = new Vector3(-10000f, -10000f, -10000f);
        static readonly Quaternion _placeholderRot = Quaternion.Euler(0f, 45f, 0f);
        static readonly Vector3 _placeholderScale = new Vector3(1.64f, 1.3f, 1f);
        Material _material;

        public ItemPool(Material itemMaterial) : base()
        {
            _material = itemMaterial;
        }

        protected override StaticMesh CreateObject()
        {
            StaticMesh smesh = StaticMesh.CreateStatic(_material);
            smesh.transform.rotation = _placeholderRot;
            smesh.transform.position = _placeholderPos;
            smesh.transform.localScale = _placeholderScale;

            return smesh;
        }

        protected override void OnReleased(StaticMesh obj)
        {
            obj.gameObject.transform.position = _placeholderPos;
        }

        protected override void OnRented(StaticMesh obj)
        {

        }

        protected override void DestroyObject(StaticMesh obj)
        {
            GameObject.Destroy(obj.gameObject);
        }
    }
}
