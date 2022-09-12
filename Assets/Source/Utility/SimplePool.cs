using System.Collections.Generic;
using System;

namespace Assets.Source.Utility
{
    public abstract class SimplePool<T> where T : class
    {
        public int Total { get; private set; }
        public int Available => _pool.Count;
        public int Rented => Total - _pool.Count;

        Stack<T> _pool;

        public SimplePool()
        {
            _pool = new Stack<T>();
        }

        public T Rent()
        {
            if (Available == 0)
                Extend();

            T obj = _pool.Pop();
            OnRented(obj);

            return obj;
        }

        public void Release(T obj)
        {
            OnReleased(obj);
            _pool.Push(obj);
        }

        public void Clear()
        {
            while (_pool.TryPop(out T obj))
                DestroyObject(obj);
        }

        void Extend()
        {
            int growSize = GetGrowSize();

            for (int i = 0; i < growSize; i++)
                _pool.Push(CreateObject());

            Total += growSize;
        }

        int GetGrowSize()
        {
            if (Total == 0)
                return 16;
            else
                return Total / 2;
        }

        protected abstract void DestroyObject(T obj);
        protected abstract T CreateObject();
        protected abstract void OnRented(T obj);
        protected abstract void OnReleased(T obj);
    }
}
