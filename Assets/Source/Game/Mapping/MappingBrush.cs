using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game.Mapping
{
    public abstract class MappingBrush
    {
        public static readonly int GlobalMaxSize = 20;
        public static readonly int GlobalMinSize = 1;

        public int MaxSize
        {
            get => _maxSize;
            set => _maxSize = Math.Min(value, GlobalMaxSize);
        }
        public int MinSize
        {
            get => _minSize;
            set => _minSize = Math.Max(value, GlobalMinSize);
        }
        public int Size
        {
            get => _size;
            set => _size = Math.Max(MinSize, Math.Min(MaxSize, value));
        }

        int _maxSize;
        int _minSize;
        int _size;

        public MappingBrush(int size, int minSize, int maxSize)
        {
            MinSize = minSize;
            MaxSize = maxSize;
            Size = size;
        }

        public abstract Vector3[] GetBrushPoints(Vector3 offset);
    }
}
