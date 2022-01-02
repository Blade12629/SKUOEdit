using UnityEngine;
using Assets.Source.Mapping.Actions;
using Assets.Source.Mapping.Brushes;

namespace Assets.Source.Mapping
{
    public static class MappingTool
    {
        public static int ToolSize { get; set; }
        public static int ToolValue { get; set; }
        public static MappingBrush Brush { get; private set; }

        static ActionHandler _actionHandler;

        static MappingTool()
        {
            _actionHandler = new ActionHandler();
        }

        /// <summary>
        /// Executes a mapping action
        /// </summary>
        /// <param name="type">Mapping action type</param>
        /// <param name="mousePos">Mouse position in the world</param>
        public static void ExecuteAction(ActionType type, Vector3 mousePos)
        {
            int mx = (int)mousePos.x;
            int mz = (int)mousePos.z;

            Brush.ExecuteBrush(mx, mz, type, ToolValue, _actionHandler.InvokeAction);
        }

        /// <summary>
        /// Resets the tool, should be called when the map is switched/unloaded
        /// </summary>
        public static void Reset()
        {
            _actionHandler.Clear();
        }
    }
}
