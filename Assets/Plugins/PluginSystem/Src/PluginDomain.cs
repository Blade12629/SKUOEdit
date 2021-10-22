using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Plugins.PluginSystem.Src
{
    public class PluginDomain
    {
        AppDomain _domain;

        public static PluginDomain CreateDomain()
        {
            PluginDomain domain = new PluginDomain();
            domain.Create();

            return domain;
        }

        public void LoadPlugin(IPluginAPI api, string assemblyName)
        {
            PluginEntryPoint entry = (PluginEntryPoint)_domain.CreateInstanceAndUnwrap(assemblyName, "Plugins.EntryPoint");
            entry.LoadPlugin(api);
        }

        public void Destroy()
        {
            _domain.DomainUnload += (_, s) => Debug.Log("Domain Getting Destroyed");
            AppDomain.Unload(_domain);
        }

        void Create()
        {
            _domain = AppDomain.CreateDomain("Unity.MapEditor.PluginDomain");
        }
    }

    public interface IPluginAPI
    {
        public void Log(object msg);
    }

    public class PluginAPI : IPluginAPI
    {
        public void Log(object msg)
        {
            Console.WriteLine(msg);
        }
    }

    public abstract class PluginEntryPoint : MarshalByRefObject
    {
        public IPluginAPI API { get; private set; }

        public void LoadPlugin(IPluginAPI api)
        {
            API = api;
            API.Log("Initialized Plugin X");
        }
    }
}
