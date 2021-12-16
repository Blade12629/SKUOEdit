using Assets.Source.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Threading
{
    // Threading.Invoke(() => Abc());

    

    public class UThreading : MonoBehaviour
    {
        public static UThreading Instance { get; private set; }

        SimplePool<EventWaitHandle> _handlePool;
        ConcurrentQueue<(Action, EventWaitHandle)> _toInvoke;
        
        /// <summary>
        /// Executes an action on the unity thread
        /// </summary>
        public static void Invoke(Action action)
        {
            Instance.InvokeInternal(action);
        }

        /// <summary>
        /// Executes an action on the unity thread async
        /// </summary>
        public static async Task InvokeAsync(Action action)
        {
            await Task.Run(() => Invoke(action)).ConfigureAwait(false);
        }

        void InvokeInternal(Action action)
        {
            EventWaitHandle handle = _handlePool.Rent();

            try
            {
                _toInvoke.Enqueue((action, handle));
                handle.WaitOne();
            }
            catch (Exception)
            {

            }

            _handlePool.Return(handle);
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _handlePool = new SimplePool<EventWaitHandle>(8, () => new EventWaitHandle(false, EventResetMode.AutoReset));
            _toInvoke = new ConcurrentQueue<(Action, EventWaitHandle)>();

            Instance = this;
        }

        void Update()
        {
            (Action, EventWaitHandle) invocation = (null, null);

            try
            {
                while (_toInvoke.TryDequeue(out invocation))
                {
                    invocation.Item1.Invoke();
                    invocation.Item2.Set();
                }
            }
            catch (Exception)
            {
                invocation.Item2?.Set();
            }
        }
    }
}
