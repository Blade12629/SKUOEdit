using Assets.Source.Game.Map;
using Assets.Source.Game.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game
{
    // TODO: rework editorinput and brushes + optimize speed
    public static class EditorInput
    {
        public static EditorAction CurrentAction { get; set; } = EditorAction.IncreaseTileHeight;
        public static int CurrentHeightValue { get; set; } = 1;
        public static short CurrentTileId { get; set; }
        public static int CurrentSize
        {
            get => _currentSize;
            set
            {
                _currentSize = Math.Max(1, value);
                _brush.Size = _currentSize;

                if (_currentSize > 1)
                    _brush.Size++;
            }
        }
        public static bool DisableInput { get; set; }

        static List<CachedInputAction> _actions = new List<CachedInputAction>(500);
        static int _actionIndex = -1;

        static MappingBrush _brush;
        static MappingBrush _defaultBrush => new PlaneBrush();

        static readonly int _terrainLayerMask = 1 << 0;

        static int _currentSize;

        static EditorInput()
        {
            _brush = _defaultBrush;
            _currentSize = 1;
        }

        public static void InitializeUIPart()
        {
            UI.MappingTools.Instance.SetHeightValueInput(CurrentHeightValue);
            UI.MappingTools.Instance.SetTileIdInput(CurrentTileId);
            UI.MappingTools.Instance.SetSelectionSizeValue(CurrentSize);
        }

        public static void ClearActions()
        {
            _actions.Clear();
            _actionIndex = -1;
        }

        public static void UpdateInput()
        {
            if (DisableInput)
                return;

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    UndoAction();
                }
                else if (Input.GetKeyDown(KeyCode.Y))
                {
                    RedoAction();
                }
            }

            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                if (CurrentAction != EditorAction.None && Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit, 10000f, _terrainLayerMask))
                    {
                        int x = (int)hit.point.x;
                        int z = (int)hit.point.z;

                        ExecuteInput(x, z);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the current brush, if null sets the default brush
        /// </summary>
        /// <param name="brush"></param>
        public static void SetBrush(MappingBrush brush)
        {
            if (brush == null)
                brush = _defaultBrush;

            _brush = brush;
        }

        public static MappingBrush GetBrush()
        {
            if (_brush == null)
                _brush = _defaultBrush;

            return _brush;
        }

        static void ExecuteInput(int xStart, int zStart)
        {
            List<int> oldValues = new List<int>();
            List<int> newValues = new List<int>();

            EditorAction action = CurrentAction;
            int sizew = CurrentSize;
            int sizeh = CurrentSize;

            if (CurrentSize > 1)
            {
                switch(action)
                {
                    case EditorAction.DecreaseTileHeight:
                        action = EditorAction.DecreaseVerticeHeight;
                        sizeh++;
                        sizew++;
                        break;
                    case EditorAction.IncreaseTileHeight:
                        action = EditorAction.IncreaseVerticeHeight;
                        sizeh++;
                        sizew++;
                        break;
                    case EditorAction.SetTileHeight:
                        action = EditorAction.SetVerticeHeight;
                        sizeh++;
                        sizew++;
                        break;

                    case EditorAction.DecreaseVerticeHeight:
                    case EditorAction.IncreaseVerticeHeight:
                    case EditorAction.SetVerticeHeight:
                        sizeh++;
                        sizew++;
                        break;


                    default:
                        break;
                }
            }

            Vector3[] points = _brush.GetBrushPoints(new Vector3(xStart, 0, zStart));

            unsafe
            {
                fixed(Vector3* pointsp = points)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        Vector3 point = pointsp[i];
                        ExecuteAction(action, (int)point.x, (int)point.z, CurrentAction == EditorAction.SetTileId ? CurrentTileId : CurrentHeightValue, oldValues, newValues);
                    }
                }
            }

            AddAction(new CachedInputAction(action, _brush, xStart, zStart, sizew, sizeh, oldValues.ToArray(), newValues.ToArray()));
        }

        static void UndoAction()
        {
            CachedInputAction action = DecrementActions();

            if (action == null)
                return;

            int xEnd = action.X + action.SizeW;
            int zEnd = action.Z + action.SizeH;
            int index = 0;
            bool firstSet = true;

            for (int x = action.X; x < xEnd; x++)
            {
                for (int z = action.Z; z < zEnd; z++)
                {
                    switch (action.Action)
                    {
                        case EditorAction.DecreaseTileHeight:
                        case EditorAction.IncreaseTileHeight:
                        case EditorAction.SetTileHeight:
                            if (firstSet)
                            {
                                firstSet = false;
                                GameMap.Instance.SetTileCornerHeight(x, z, action.OldValues[index]);
                            }

                            index++;
                            GameMap.Instance.SetTileCornerHeight(x, z + 1, action.OldValues[index++]);
                            GameMap.Instance.SetTileCornerHeight(x + 1, z + 1, action.OldValues[index++]);
                            GameMap.Instance.SetTileCornerHeight(x + 1, z, action.OldValues[index++]);
                            break;

                        case EditorAction.DecreaseVerticeHeight:
                        case EditorAction.IncreaseVerticeHeight:
                        case EditorAction.SetVerticeHeight:
                            GameMap.Instance.SetTileCornerHeight(x, z, action.OldValues[index++]);
                            break;

                        case EditorAction.SetTileId:
                            GameMap.Instance.SetTileId(x, z, (short)action.OldValues[index++]);
                            break;

                        default:
                            continue;
                    }
                }
            }
        }

        static void RedoAction()
        {
            CachedInputAction action = IncrementActions();

            if (action == null)
                return;

            int xEnd = action.X + action.SizeW;
            int zEnd = action.Z + action.SizeH;
            int index = 0;

            for (int x = action.X; x < xEnd; x++)
            {
                for (int z = action.Z; z < zEnd; z++)
                {
                    switch (action.Action)
                    {
                        case EditorAction.DecreaseTileHeight:
                        case EditorAction.IncreaseTileHeight:
                        case EditorAction.SetTileHeight:
                            GameMap.Instance.SetTileCornerHeight(x,     z,      action.NewValues[index++]);
                            GameMap.Instance.SetTileCornerHeight(x,     z + 1,  action.NewValues[index++]);
                            GameMap.Instance.SetTileCornerHeight(x + 1, z + 1,  action.NewValues[index++]);
                            GameMap.Instance.SetTileCornerHeight(x + 1, z,      action.NewValues[index++]);
                            break;

                        case EditorAction.DecreaseVerticeHeight:
                        case EditorAction.IncreaseVerticeHeight:
                        case EditorAction.SetVerticeHeight:
                            GameMap.Instance.SetTileCornerHeight(x, z, action.NewValues[index++]);
                            break;

                        case EditorAction.SetTileId:
                            GameMap.Instance.SetTileId(x, z, (short)action.NewValues[index++]);
                            break;

                        default:
                            continue;
                    }
                }
            }
        }

        static CachedInputAction DecrementActions()
        {
            if (_actionIndex == -1)
                return null;

            return _actions[_actionIndex--];
        }

        static CachedInputAction IncrementActions()
        {
            if (_actionIndex == _actions.Count - 1)
                return null;

            return _actions[++_actionIndex];
        }

        static void AddAction(CachedInputAction action)
        {
            while (_actionIndex < _actions.Count - 1)
                _actions.RemoveAt(_actions.Count - 1);

            if (_actions.Count == 500) // actionqueue is at it's limit
            {
                _actions.RemoveAt(0);
                _actionIndex--;
            }

            _actions.Add(action);
            _actionIndex++;
        }

        static void ExecuteAction(EditorAction action, int x, int z, int value, List<int> oldValues, List<int> newValues)
        {
            switch(action)
            {
                case EditorAction.IncreaseVerticeHeight:
                    InvokeVertexAction(GameMap.Instance.IncreaseTileCornerHeight);
                    break;

                case EditorAction.DecreaseVerticeHeight:
                    InvokeVertexAction((x_, z_, value_) => GameMap.Instance.IncreaseTileCornerHeight(x_, z_, -value_));
                    break;

                case EditorAction.SetVerticeHeight:
                    InvokeVertexAction(GameMap.Instance.SetTileCornerHeight);
                    break;


                case EditorAction.IncreaseTileHeight:
                    InvokeTileAction(GameMap.Instance.IncreaseTileHeight);
                    break;
                case EditorAction.DecreaseTileHeight:
                    InvokeTileAction((x_, z_, value_) => GameMap.Instance.IncreaseTileHeight(x_, z_, -value_));
                    break;
                case EditorAction.SetTileHeight:
                    InvokeTileAction(GameMap.Instance.SetTileHeight);
                    break;


                case EditorAction.SetTileId:
                    oldValues?.Add(GameMap.Instance.GetTileId(x, z));
                    GameMap.Instance.SetTileId(x, z, (short)value);
                    newValues?.Add(GameMap.Instance.GetTileId(x, z));
                    break;
            }

            void InvokeTileAction(Action<int, int, int> toExecute)
            {
                oldValues?.AddRange(GameMap.Instance.GetTileCornerHeight(x,     z),
                                    GameMap.Instance.GetTileCornerHeight(x,     z + 1),
                                    GameMap.Instance.GetTileCornerHeight(x + 1, z + 1),
                                    GameMap.Instance.GetTileCornerHeight(x + 1, z));

                toExecute(x, z, value);


                newValues?.AddRange(GameMap.Instance.GetTileCornerHeight(x,     z),
                                    GameMap.Instance.GetTileCornerHeight(x,     z + 1),
                                    GameMap.Instance.GetTileCornerHeight(x + 1, z + 1),
                                    GameMap.Instance.GetTileCornerHeight(x + 1, z));
            }

            void InvokeVertexAction(Action<int, int, int> toExecute)
            {
                oldValues?.Add(GameMap.Instance.GetTileCornerHeight(x, z));
                toExecute(x, z, value);
                newValues?.Add(GameMap.Instance.GetTileCornerHeight(x, z));
            }
        }

        class CachedInputAction
        {
            public EditorAction Action { get; }

            public int X { get; }
            public int Z { get; }

            public int SizeW { get; }
            public int SizeH { get; }

            public int[] OldValues { get; }
            public int[] NewValues { get; }

            public MappingBrush Brush { get; }

            public CachedInputAction(EditorAction action, MappingBrush brush, int x, int z, int sizew, int sizeh, int[] oldValues, int[] newValues)
            {
                Action = action;
                Brush = brush;
                X = x;
                Z = z;
                SizeW = sizew;
                SizeH = sizeh;
                OldValues = oldValues;
                NewValues = newValues;
            }
        }
    }

    public enum EditorAction
    {
        None,

        IncreaseVerticeHeight,
        DecreaseVerticeHeight,
        SetVerticeHeight,

        IncreaseTileHeight,
        DecreaseTileHeight,
        SetTileHeight,

        SetTileId
    }
}
