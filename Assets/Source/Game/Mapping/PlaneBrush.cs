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
        static readonly Vector3[] _baseBrushPoints = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, 0),
        };

        public PlaneBrush(int size) : base(size)
        {

        }

        public PlaneBrush() : this(1)
        {

        }

        public override Vector3[] GetBrushPoints()
        {
            Vector3[] result = new Vector3[_baseBrushPoints.Length];
            Array.Copy(_baseBrushPoints, result, result.Length);

            if (Size <= 1)
                return result;

            unsafe
            {
                fixed(Vector3* rp = result)
                {
                    for (int i = 0; i < result.Length; i++)
                    {
                        Vector3* v = rp + i;

                        if (v->x > 0)
                            v->x += Size;
                        else if (v->x < 0)
                            v->x -= Size;

                        if (v->y > 0)
                            v->y += Size;
                        else if (v->y < 0)
                            v->y -= Size;

                        if (v->z > 0)
                            v->z += Size;
                        else if (v->z < 0)
                            v->z -= Size;
                    }
                }
            }

            return result;
        }
    }
}
