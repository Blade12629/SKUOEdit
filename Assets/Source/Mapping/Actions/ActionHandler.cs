using System.Collections.Generic;
using Assets.Source.Game.Map;

namespace Assets.Source.Mapping.Actions
{
    public class ActionHandler
    {
        Stack<ActionArgs> _actionStack;

        public ActionHandler()
        {
            _actionStack = new Stack<ActionArgs>();
        }

        /// <summary>
        /// Invokes an action and caches it
        /// </summary>
        public void InvokeAction(ActionType type, int x, int z, int value)
        {
            switch (type)
            {
                default:
                    return;

                case ActionType.TileIdSet:
                    {
                        short tid = GameMap.Instance.SetTileId(x, z, (short)value);
                        _actionStack.Push(new ActionArgs(x, z, type, tid));
                    }
                    break;

                case ActionType.TileInc:
                    {
                        int[] heights = GameMap.Instance.IncreaseTileHeight(x, z, value);
                        _actionStack.Push(new ActionArgs(x, z, type, heights[0], heights[1], heights[2], heights[3]));
                    }
                    break;

                case ActionType.TileSet:
                    {
                        int[] heights = GameMap.Instance.SetTileHeight(x, z, value);
                        _actionStack.Push(new ActionArgs(x, z, type, heights[0], heights[1], heights[2], heights[3]));
                    }
                    break;

                case ActionType.VertexInc:
                    {
                        int h = GameMap.Instance.IncreaseTileCornerHeight(x, z, value);
                        _actionStack.Push(new ActionArgs(x, z, type, h));
                    }
                    break;

                case ActionType.VertexSet:
                    {
                        int h = GameMap.Instance.SetTileCornerHeight(x, z, value);
                        _actionStack.Push(new ActionArgs(x, z, type, h));
                    }
                    break;
            }
        }

        /// <summary>
        /// Reverts the last executed action
        /// </summary>
        public void UndoAction()
        {
            if (_actionStack.Count == 0)
                return;

            ActionArgs args = _actionStack.Pop();

            switch (args.Executed)
            {
                default:
                    return;

                case ActionType.VertexInc:
                case ActionType.VertexSet:
                    GameMap.Instance.SetTileCornerHeight(args.X, args.Z, args.OldVal1);
                    break;

                case ActionType.TileSet:
                case ActionType.TileInc:
                    GameMap.Instance.SetTileCornerHeight(args.X, args.Z, args.OldVal1);
                    GameMap.Instance.SetTileCornerHeight(args.X, args.Z + 1, args.OldVal2);
                    GameMap.Instance.SetTileCornerHeight(args.X + 1, args.Z + 1, args.OldVal3);
                    GameMap.Instance.SetTileCornerHeight(args.X + 1, args.Z, args.OldVal4);
                    break;

                case ActionType.TileIdSet:
                    GameMap.Instance.SetTileId(args.X, args.Z, (short)args.OldVal1);
                    break;
            }
        }

        public void Clear()
        {
            _actionStack.Clear();
        }
    }
}
