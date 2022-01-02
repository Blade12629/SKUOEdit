using Server.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Mapping
{
    public sealed class CircleBrush : MappingBrush
    {
        public CircleBrush(int size) : base(size, GlobalMinSize, GlobalMaxSize)
        {
        }

        public CircleBrush() : this(GlobalMinSize)
        {

        }

        public override Vector3[] GetBrushPoints(Vector3 offset)
        {
            List<Vector3> points = new List<Vector3>();

            int halfSize = Size / 2;
            int zsteps = -1;
            int zstart = -1;

            for (int x = 0; x < halfSize + 1; x++)
            {
                zsteps += 2;
                zstart++;

                for (int z = 0; z < zsteps; z++)
                    points.Add(new Vector3(x, 0, z));
            }

            zsteps -= 2;
            zstart--;

            for (int x = 0; x < halfSize; x++)
            {
                for (int z = 0; z < zsteps; z++)
                    points.Add(new Vector3(x, 0, z));

                zsteps -= 2;
                zstart--;
            }

            return points.ToArray();
        }
    }
}
