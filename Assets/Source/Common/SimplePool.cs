using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Common
{
    public class SimplePool<T>
    {
        public int Available { get; private set; }
        public int Rented { get; private set; }
        public int Total { get; private set; }

        Queue<T> _pool;
        Func<T> _createPoolObj;

        public SimplePool(int initialCount, Func<T> createPoolObj)
        {
            _pool = new Queue<T>();
            _createPoolObj = createPoolObj;

            Grow(Math.Max(4, initialCount));
        }

        public T Rent()
        {
            if (Available == 0)
                Grow(GetGrowSize());

            Available--;
            Rented++;

            return _pool.Dequeue();
        }

        public void Return(T obj)
        {
            if (obj == null)
                return;

            Available++;
            Rented--;

            _pool.Enqueue(obj);
        }

        int GetGrowSize()
        {
            return (int)Math.Ceiling(Total / 4.0);
        }

        void Grow(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _pool.Enqueue(_createPoolObj());

                Available++;
                Total++;
            }
        }
    }
}
