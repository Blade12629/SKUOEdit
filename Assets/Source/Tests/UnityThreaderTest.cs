using Assets.Source.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Tests
{
    public class UnityThreaderTest : MonoBehaviour
    {
        private void Start()
        {
            UThreading.Init();

            Task.Run(() =>
            {
                while(true)
                {
                    Task.Delay(5000).ConfigureAwait(false).GetAwaiter().GetResult();
                    UThreading.InvokeAsync(() => Debug.Log("Iteration done")).ConfigureAwait(false);
                }
            });
        }
    }
}
