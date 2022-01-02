using Assets.Source.Mapping.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Mapping.Brushes
{
    public class UserDefinedBrush : MappingBrush
    {
        public string AssemblyName { get; set; }
        public string ClassPath { get; set; }
        public string MethodName { get; set; }
        public UserBrushDomain BrushDomain { get; set; }

        public override void ExecuteBrush(int centerX, int centerZ, ActionType type, int value, Action<ActionType, int, int, int> toExecute)
        {
            MethodInfo m = BrushDomain.Get(AssemblyName, ClassPath, MethodName);
            m.Invoke(null, new object[] { centerX, centerZ, type, value, toExecute });
        }
    }

    public class UserBrushDomain : IDisposable
    {
        public bool IsDisposed { get; private set; }

        AppDomain _domain;
        Dictionary<string, Assembly> _brushAssemblies;

        public UserBrushDomain()
        {
            _domain = AppDomain.CreateDomain(nameof(UserBrushDomain));
        }

        ~UserBrushDomain()
        {
            Dispose(false);
        }

        public MethodInfo Get(string brush, string classpath, string methodname)
        {
            if (!_brushAssemblies.TryGetValue(brush, out Assembly asm))
                return null;

            Type t = asm.GetType(classpath);
            MethodInfo m = t.GetMethod(methodname);
            return m;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void LoadBrush(string name, string assemblyPath)
        {
            byte[] asmBytes = System.IO.File.ReadAllBytes(assemblyPath);
            Assembly asm = _domain.Load(asmBytes);

            _brushAssemblies.Add(name, asm);
        }

        void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                AppDomain.Unload(_domain);
            }
        }
    }

}
