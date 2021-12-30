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
            throw new NotImplementedException();
        }
    }
}
