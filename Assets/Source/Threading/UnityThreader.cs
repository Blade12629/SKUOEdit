//#define RUNSAFE

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
    public class UnityThreader : MonoBehaviour
    {
        static readonly int _maxInvokesPerUpdate = 100;
        static UnityThreader _instance;

        ConcurrentQueue<UnityThreaderInvokeArgs> _invokeEndQueue;

        public UnityThreader()
        {
            _invokeEndQueue = new ConcurrentQueue<UnityThreaderInvokeArgs>();
            _instance = this;
        }

        /// <summary>
        /// Invokes the action on a diffrent thread and returns the thread if thread pool isn't used
        /// </summary>
        public static Thread RunThreaded(UnityThreaderInvokeArgs args, bool useThreadPool)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            Thread result = null;

            args.Prepare();

            if (useThreadPool)
                ThreadPool.QueueUserWorkItem(new WaitCallback(o => RunAsyncActionSync(args)));
            else
            {
                result = new Thread(new ThreadStart(() => RunAsyncActionSync(args)));
                result.IsBackground = true;

                result.Start();
            }

            return result;
        }

        public static async Task RunAsync(UnityThreaderInvokeArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            args.Prepare();
            await Task.Run(() => RunAsyncActionSync(args)).ConfigureAwait(false);
        }


        static void RunAsyncActionSync(UnityThreaderInvokeArgs args)
        {
#if RUNSAFE
            try
            {
#endif
                args.InvokeAsyncActionSync();
                _instance._invokeEndQueue.Enqueue(args);
#if RUNSAFE
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
#endif
            args.Finish();
        }

        void Update()
        {
            for (int i = 0; i < _maxInvokesPerUpdate; i++)
            {
                if (!_invokeEndQueue.TryDequeue(out UnityThreaderInvokeArgs args))
                    break;

#if RUNSAFE
                try
                {
#endif
                    args.InvokeUnityThreadAction();
#if RUNSAFE
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
#endif
            }
        }
    }

    public class UnityThreaderInvokeArgs
    {
        Action<object> _asyncAction;
        Action<object> _unityThreadAction;
        object _args;
        EventWaitHandle _ewh;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncAction">Action to be executed async</param>
        /// <param name="unityThreadAction">Action to be executed on the unity thread afterwards</param>
        /// <param name="args">Actions args</param>
        public UnityThreaderInvokeArgs(Action<object> asyncAction, Action<object> unityThreadAction, object args)
        {
            _ewh = new EventWaitHandle(true, EventResetMode.ManualReset);
            _asyncAction = asyncAction;
            _unityThreadAction = unityThreadAction;
            _args = args;

            if (_asyncAction == null)
                throw new InvalidOperationException("Async action cannot be null");
        }

        /// <param name="asyncAction">Action to be executed async</param>
        /// <param name="args">Actions args</param>
        public UnityThreaderInvokeArgs(Action<object> asyncAction, object args) : this(asyncAction, null, args)
        {

        }

        /// <summary>
        /// Waits until the async action has executed or the time runs out ww
        /// </summary>
        public void WaitForCompletion(int msTimeout)
        {
            if (_ewh == null)
                return;

            _ewh.WaitOne(msTimeout);
        }

        /// <summary>
        /// Prepares the async action
        /// </summary>
        public void Prepare()
        {
            _ewh = new EventWaitHandle(false, EventResetMode.AutoReset);
        }

        /// <summary>
        /// Invokes the async action in a synchronous manner
        /// </summary>
        public void InvokeAsyncActionSync()
        {
            _asyncAction?.Invoke(_args);
        }

        /// <summary>
        /// Invokes the unity thread action
        /// </summary>
        public void InvokeUnityThreadAction()
        {
            _unityThreadAction?.Invoke(_args);
        }

        /// <summary>
        /// Finishes the async action
        /// </summary>
        public void Finish()
        {
            _ewh?.Set();
        }
    }
}
