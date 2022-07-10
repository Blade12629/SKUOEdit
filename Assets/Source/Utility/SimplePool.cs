using System.Collections.Generic;

namespace Assets.Source.Utility
{
    public abstract class SimplePool<T> where T : class
    {
        public int Available => _pool.Count;
        public int Rented => _capacity - _pool.Count;

        Queue<T> _pool;
        int _capacity;
        int _extendCapacity;
        bool _hasBeenExtended;

        public SimplePool(int extendCapacity)
        {
            _extendCapacity = extendCapacity;
            _capacity = extendCapacity;
            _pool = new Queue<T>(extendCapacity);
        }

        public T Rent()
        {
            if (Available == 0)
                ExtendPool();

            T obj = _pool.Dequeue();
            OnRented(obj);
            return obj;
        }

        public void Release(T obj)
        {
            if (obj == null)
                return;

            _pool.Enqueue(obj);
            OnReleased(obj);
        }

        void ExtendPool()
        {
            int count;

            if (_hasBeenExtended)
            {
                count = _extendCapacity;
                _capacity += count;
            }
            else
            {
                count = _capacity;
                _hasBeenExtended = true;
            }

            for (int i = 0; i < count; i++)
            {
                T obj = CreateObject();
                _pool.Enqueue(obj);
            }
        }

        protected abstract T CreateObject();
        protected abstract void OnRented(T obj);
        protected abstract void OnReleased(T obj);
    }
}
