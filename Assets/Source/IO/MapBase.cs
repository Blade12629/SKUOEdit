using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Source.IO
{
    public abstract class MapBase
    {
        public int Width { get; protected set; }
        public int Depth { get; protected set; }

        public int BlockWidth { get; protected set; }
        public int BlockDepth { get; protected set; }

        public bool IsLoaded { get; protected set; }

        protected void SetSize(int width, int depth)
        {
            Width = width;
            Depth = depth;
            BlockWidth = width / 8;
            BlockDepth = depth / 8;
        }
    }
}
