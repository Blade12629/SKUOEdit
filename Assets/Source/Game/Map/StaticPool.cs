using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Map
{
    /// <summary>
    /// A pool which allows to reuse statics
    /// <para>Keeps growing by 100 everytime there is no static left to take</para>
    /// </summary>
    public static class StaticPool
    {
        static readonly Queue<GameObject> _pool = new Queue<GameObject>(_growSize);
        static readonly int _growSize = 100;
        static readonly Quaternion _defaultStaticRot = Quaternion.Euler(0, -90, 0);

        /// <summary>
        /// Rent a new object, if no objects available the pool will grow by 100 objects
        /// </summary>
        public static GameObject Rent()
        {
            if (_pool.Count == 0)
                Grow();

            return _pool.Dequeue();
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

            _pool.Enqueue(obj);
        }

        static void Grow()
        {
            for (int i = 0; i < _growSize; i++)
                _pool.Enqueue(CreatePoolObject());
        }

        static GameObject CreatePoolObject()
        {
            GameObject result = new GameObject("Static", typeof(MeshRenderer), typeof(MeshFilter));
            //result.SetActive(false);
            result.transform.rotation = _defaultStaticRot;

            result.GetComponent<MeshFilter>().mesh = new Mesh();

            return result;
        }
    }
}
