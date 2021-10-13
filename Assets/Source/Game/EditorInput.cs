﻿using Assets.Source.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Game
{
    public static class EditorInput
    {
        public static EditorAction CurrentAction { get; set; } = EditorAction.IncreaseTileHeight;
        public static int CurrentHeightValue { get; set; } = 1;
        public static short CurrentTileId { get; set; }
        public static int CurrentSize { get; set; } = 1;
        public static bool DisableInput { get; set; }

        static List<CachedInputAction> _actions = new List<CachedInputAction>(500);
        static int _actionIndex = -1;

        static readonly int _terrainLayerMask = 1 << 0;

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

        static void ExecuteInput(int xStart, int zStart)
        {
            List<int> oldValues = new List<int>();
            List<int> newValues = new List<int>();

            EditorAction action = CurrentAction;
            int size = CurrentSize;

            if (CurrentSize > 1)
            {
                size++;

                switch(action)
                {
                    case EditorAction.DecreaseTileHeight:
                        action = EditorAction.DecreaseVerticeHeight;
                        break;
                    case EditorAction.IncreaseTileHeight:
                        action = EditorAction.IncreaseVerticeHeight;
                        break;
                    case EditorAction.SetTileHeight:
                        action = EditorAction.SetVerticeHeight;
                        break;

                    default:
                        break;
                }
            }

            int xEnd = xStart + size;
            int zEnd = zStart + size;

            for (int x = xStart; x < xEnd; x++)
            {
                for (int z = zStart; z < zEnd; z++)
                {
                    ExecuteAction(action, x, z, CurrentAction == EditorAction.SetTileId ? CurrentTileId : CurrentHeightValue, oldValues, newValues);
                }
            }

            AddAction(new CachedInputAction(action, xStart, zStart, size, oldValues.ToArray(), newValues.ToArray()));
        }

        static void UndoAction()
        {
            CachedInputAction action = DecrementActions();

            if (action == null)
                return;

            int xEnd = action.X + action.Size;
            int zEnd = action.Z + action.Size;
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

            int xEnd = action.X + action.Size;
            int zEnd = action.Z + action.Size;
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
                    InvokeVertexAction(GameMap.Instance.DecreaseTileCornerHeight);
                    break;

                case EditorAction.SetVerticeHeight:
                    InvokeVertexAction(GameMap.Instance.SetTileCornerHeight);
                    break;


                case EditorAction.IncreaseTileHeight:
                    InvokeTileAction(GameMap.Instance.IncreaseTileHeight);
                    break;
                case EditorAction.DecreaseTileHeight:
                    InvokeTileAction(GameMap.Instance.DecreaseTileHeight);
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

            public int Size { get; }

            public int[] OldValues { get; }
            public int[] NewValues { get; }

            public CachedInputAction(EditorAction action, int x, int z, int size, int[] oldValues, int[] newValues)
            {
                Action = action;
                X = x;
                Z = z;
                Size = size;
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
