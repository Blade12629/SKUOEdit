using Assets.Source.Mapping.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.Mapping.Brushes
{
    public class PlaneBrush : MappingBrush
    {
        public override void ExecuteBrush(int centerX, int centerZ, ActionType type, int value, Action<ActionType, int, int, int> toExecute)
        {
            int hsize = Size / 2;

            for (int x = centerX - hsize; x < centerX + hsize; x++)
            {
                for (int z = centerZ - hsize; z < centerZ + hsize; z++)
                {
                    toExecute(type, x, z, value);
                }
            }
        }
    }
}
