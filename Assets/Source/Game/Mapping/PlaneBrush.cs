using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Mapping
{
    public sealed class PlaneBrush : MappingBrush
    {
        public PlaneBrush(int size) : base(size, GlobalMinSize, GlobalMaxSize)
        {
        }

        public PlaneBrush() : this(GlobalMinSize)
        {

        }

        public override Vector3[] GetBrushPoints(Vector3 offset)
        {
            Vector3[] result = new Vector3[Size * Size];

            int offsetX = (int)offset.x;
            int offsetZ = (int)offset.z;

            if (Size > 1)
            {
                float halfSize = Size / 2f;
                int ihalfSize = (int)halfSize;
                offsetX -= ihalfSize;
                offsetZ -= ihalfSize;

                if (halfSize == ihalfSize)
                {
                    offsetX++;
                    offsetZ++;
                }
            }

            int index = 0;
            for (int x = 0; x < Size; x++)
                for (int z = 0; z < Size; z++)
                    result[index++] = new Vector3(x + offsetX, 0, z + offsetZ);

            return result;
        }
    }
}
