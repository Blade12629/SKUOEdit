using System;
using Assets.Source.Mapping.Actions;

namespace Assets.Source.Mapping.Brushes
{
    public abstract class MappingBrush
    {
        public int Size { get => MappingTool.ToolSize; set => MappingTool.ToolSize = value; }

        /// <summary>
        /// Executes this brush
        /// </summary>
        /// <param name="centerX">X Coordinate of center</param>
        /// <param name="centerZ">Z Coordinate of center</param>
        /// <param name="type">Action Type</param>
        /// <param name="value">Action Value</param>
        /// <param name="toExecute">Invoked for each point that should be modified (type, x, z, value)</param>
        public abstract void ExecuteBrush(int centerX, int centerZ, ActionType type, int value, Action<ActionType, int, int, int> toExecute);
    }
}
