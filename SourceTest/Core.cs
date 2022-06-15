//using Assets.SourceTest.Textures;
using Assets.SourceTest.World;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.SourceTest
{
    public class Core : MonoBehaviour
    {
        /// <summary>
        /// Current instance, core will only ever exist once
        /// </summary>
        public static Core Instance { get; private set; }

        //public TextureAtlas Textures => _textures;
        public Map Map => _map;


        [SerializeField] Map _map;

        /// <summary>
        /// Stores all invoke requests
        /// </summary>
        ConcurrentQueue<Action> _invocationQueue;
        /// <summary>
        /// Texture atlas used for the <see cref="Terrain"/>
        /// </summary>
        //TextureAtlas _textures;


        /// <summary>
        /// Invokes an action on the main thread when the next <see cref="Update"/> is called
        /// </summary>
        /// <param name="ac"></param>
        public void Invoke(Action ac)
        {
            Instance._invocationQueue.Enqueue(ac);
        }

        public void CleanUp()
        {
            while (_invocationQueue.Count > 0)
                _invocationQueue.TryDequeue(out Action _);

            _map.Clear();
        }


        void Start()
        {
            Instance = this;

            _invocationQueue = new ConcurrentQueue<Action>();

            Source.GameFiles.LoadClientFiles();
            _map.Initialize(95);

            // For now we load the map through here
            _map.LoadMap(@"D:\reposSSD\SKUOEdit\Test\map3LegacyMUL.uop", 2560, 2048);
        }

        void Update()
        {
            // run all queued invoke requests
            while (_invocationQueue.TryDequeue(out Action ac))
            {
                ac();
            }
        }
    }
}
