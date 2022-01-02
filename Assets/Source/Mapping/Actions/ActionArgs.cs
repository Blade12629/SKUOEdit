using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Mapping.Actions
{
    /// <summary>
    /// Contains info about an executed action
    /// </summary>
    public struct ActionArgs
    {
        public int X { get; }
        public int Z { get; }
        /// <summary>
        /// What type of action was executed?
        /// </summary>
        public ActionType Executed { get; }
        /// <summary>
        /// The value to use when reversing this action
        /// </summary>
        public int OldVal1 { get; }
        public int OldVal2 { get; }
        public int OldVal3 { get; }
        public int OldVal4 { get; }

        public ActionArgs(int x, int z, ActionType executed, int oldVal1) : this()
        {
            X = x;
            Z = z;
            Executed = executed;
            OldVal1 = oldVal1;
        }

        public ActionArgs(int x, int z, ActionType executed, int oldVal1, int oldVal2, int oldVal3, int oldVal4) : this(x, z, executed, oldVal1)
        {
            OldVal2 = oldVal2;
            OldVal3 = oldVal3;
            OldVal4 = oldVal4;
        }
    }
}
