using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Source.Common;

namespace Assets.Source.Game.Map
{
    /// <summary>
    /// A pool which allows to reuse statics
    /// <para>Keeps growing by 100 everytime there is no static left to take</para>
    /// </summary>
    public static class StaticPool
    {
        static readonly SimplePool<GameObject> _pool = new SimplePool<GameObject>(25, CreatePoolObject);
        static readonly Quaternion _defaultStaticRotOrtographic = Quaternion.Euler(90, 225, 0);
        static readonly Quaternion _defaultStaticRotPerspective = Quaternion.Euler(0, -90, 0);

        static readonly Vector3 _defaultScaleOrtographic = new Vector3(1.5f, 1.5f, 1f);

        public static GameObject Rent()
        {
            return _pool.Rent();
        }

        /// <summary>
        /// Rent a new object, if no objects available the pool will grow by 100 objects
        /// <para>optionally you can choose to enable the gameobject before it is returned</para>
        /// </summary>
        public static GameObject Rent(bool enableObject)
        {
            GameObject obj = Rent();

            if (!obj.activeSelf && enableObject)
                obj.SetActive(true);

            return obj;
        }
        /// <summary>
        /// Returns the object back to the pool and disable it
        /// </summary>
        public static void Return(GameObject obj)
        {
            Return(obj, true);
        }

        /// <summary>
        /// Returns the object back to the pool and optionally disable it
        /// </summary>
        public static void Return(GameObject obj, bool disableObject)
        {
            if (disableObject && obj.activeSelf)
                obj.SetActive(false);

            _pool.Return(obj);
        }

        public static void ReturnRange(List<GameObject> obj, bool disableObject)
        {
            for (int i = 0; i < obj.Count; i++)
            {
                Return(obj[i], disableObject);
            }
        }

        public static void ReturnRange(List<GameObject> obj)
        {
            for (int i = 0; i < obj.Count; i++)
            {
                Return(obj[i]);
            }
        }

        static GameObject CreatePoolObject()
        {
            GameObject result = new GameObject("Static", typeof(SpriteRenderer));
            result.SetActive(false);
            result.transform.rotation = _defaultStaticRotOrtographic;
            result.transform.localScale = _defaultScaleOrtographic;

            return result;
        }
    }
}
