using Assets.Source.Game.Map;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Source.Game
{
    public sealed class EditorInput
    {
        public static EditorInput Instance { get; private set; }

        public EditorAction CurrentAction { get; set; }
        public int CurrentValue { get; set; }
        public int CurrentTileId { get; set; }
        public bool DisableEditorInput { get; set; }

        readonly int _terrainLayerMask;
        List<InvokedEditorAction> _actionQueue;
        int _actionIndex = -1;


        public EditorInput()
        {
            Instance = this;
            _terrainLayerMask = 1 << 0;
            _actionQueue = new List<InvokedEditorAction>(500);

            CurrentAction = EditorAction.IncreaseTileHeight;
            CurrentValue = 1;

            UI.MappingTools.Instance.SetHeightValueInput(CurrentValue);
        }

        public void Update()
        {
            if (DisableEditorInput)
                return;

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    ReverseLastAction();
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

                        InvokeAction(CurrentAction, x, z, CurrentAction == EditorAction.SetTileId ? CurrentTileId : CurrentValue, false);
                    }
                }
            }
        }

        public void ClearActionQueue()
        {
            _actionQueue.Clear();
            _actionIndex = -1;
        }

        void InvokeAction(EditorAction action, int x, int z, int value, bool isRedoAction)
        {
            int ov0 = 0;
            int ov1 = 0;
            int ov2 = 0;
            int ov3 = 0;

            switch (CurrentAction)
            {
                case EditorAction.IncreaseVerticeHeight:
                    SetVertAction();
                    GameMap.Instance.IncreaseTileCornerHeight(x, z, value);
                    break;

                case EditorAction.DecreaseVerticeHeight:
                    SetVertAction();
                    GameMap.Instance.IncreaseTileCornerHeight(x, z, -value);
                    break;

                case EditorAction.SetVerticeHeight:
                    SetVertAction();
                    GameMap.Instance.SetTileCornerHeight(x, z, value);
                    break;

                case EditorAction.IncreaseTileHeight:
                    SetTileAction();
                    GameMap.Instance.IncreaseTileHeight(x, z, value);
                    break;

                case EditorAction.DecreaseTileHeight:
                    SetTileAction();
                    GameMap.Instance.IncreaseTileHeight(x, z, -value);
                    break;

                case EditorAction.SetTileHeight:
                    SetTileAction();
                    GameMap.Instance.SetTileHeight(x, z, value);
                    break;

                case EditorAction.SetTileId:
                    SetTileIdAction();
                    GameMap.Instance.SetTileId(x, z, (short)value);
                    break;
            }

            OnActionExecuted(action, x, z, ov0, ov1, ov2, ov3, isRedoAction);

            void SetTileAction()
            {
                ov0 = GameMap.Instance.GetTileCornerHeight(x, z);
                ov1 = GameMap.Instance.GetTileCornerHeight(x, z + 1);
                ov2 = GameMap.Instance.GetTileCornerHeight(x + 1, z + 1);
                ov3 = GameMap.Instance.GetTileCornerHeight(x + 1, z);
            }

            void SetVertAction()
            {
                ov0 = GameMap.Instance.GetTileCornerHeight(x, z);
            }

            void SetTileIdAction()
            {
                ov0 = GameMap.Instance.GetTileId(x, z);
            }
        }

        void OnActionExecuted(EditorAction action, int x, int z, int ov0, int ov1, int ov2, int ov3, bool isRedoAction)
        {
            if (_actionQueue.Count > 0)
            {
                // check if we reversed an action before
                if (_actionIndex < _actionQueue.Count - 1)
                {
                    // we are just redoing our action
                    if (isRedoAction)
                    {
                        _actionIndex++;
                        return;
                    }

                    // delete all actions that are infront of us since we did something new instead of redoing
                    while (_actionIndex < _actionQueue.Count - 1)
                        _actionQueue.RemoveAt(_actionQueue.Count - 1);
                }
            }

            int nv0 = 0;
            int nv1 = 0;
            int nv2 = 0;
            int nv3 = 0;

            switch (action)
            {
                case EditorAction.IncreaseVerticeHeight:
                case EditorAction.DecreaseVerticeHeight:
                case EditorAction.SetVerticeHeight:
                    nv0 = GameMap.Instance.GetTileCornerHeight(x, z);
                    break;

                case EditorAction.IncreaseTileHeight:
                case EditorAction.DecreaseTileHeight:
                case EditorAction.SetTileHeight:
                    nv0 = GameMap.Instance.GetTileCornerHeight(x, z);
                    nv1 = GameMap.Instance.GetTileCornerHeight(x, z + 1);
                    nv2 = GameMap.Instance.GetTileCornerHeight(x + 1, z + 1);
                    nv3 = GameMap.Instance.GetTileCornerHeight(x + 1, z);
                    break;

                case EditorAction.SetTileId:
                    nv0 = GameMap.Instance.GetTileId(x, z);
                    break;
            }

            _actionQueue.Add(new InvokedEditorAction(action, x, z, ov0, ov1, ov2, ov3, nv0, nv1, nv2, nv3));
            _actionIndex++;
        }

        public void ReverseLastAction()
        {
            if (_actionQueue.Count == 0 || _actionIndex == -1)
                return;

            InvokedEditorAction la = _actionQueue[_actionIndex--];

            switch (la.Action)
            {
                case EditorAction.SetVerticeHeight:
                case EditorAction.DecreaseVerticeHeight:
                case EditorAction.IncreaseVerticeHeight:
                    GameMap.Instance.SetTileCornerHeight(la.X, la.Z, la.OldValue0);
                    break;

                case EditorAction.DecreaseTileHeight:
                case EditorAction.IncreaseTileHeight:
                case EditorAction.SetTileHeight:
                    GameMap.Instance.SetTileCornerHeight(la.X, la.Z, la.OldValue0);
                    GameMap.Instance.SetTileCornerHeight(la.X, la.Z + 1, la.OldValue1);
                    GameMap.Instance.SetTileCornerHeight(la.X + 1, la.Z + 1, la.OldValue2);
                    GameMap.Instance.SetTileCornerHeight(la.X + 1, la.Z, la.OldValue3);
                    break;

                case EditorAction.SetTileId:
                    GameMap.Instance.SetTileId(la.X, la.Z, (short)la.OldValue0);
                    break;
            }
        }

        public void RedoAction()
        {
            if (_actionIndex == _actionQueue.Count - 1)
                return;

            InvokedEditorAction la = _actionQueue[++_actionIndex];

            switch (la.Action)
            {
                case EditorAction.SetVerticeHeight:
                case EditorAction.DecreaseVerticeHeight:
                case EditorAction.IncreaseVerticeHeight:
                    GameMap.Instance.SetTileCornerHeight(la.X, la.Z, la.NewValue0);
                    break;

                case EditorAction.DecreaseTileHeight:
                case EditorAction.IncreaseTileHeight:
                case EditorAction.SetTileHeight:
                    GameMap.Instance.SetTileCornerHeight(la.X, la.Z, la.NewValue0);
                    GameMap.Instance.SetTileCornerHeight(la.X, la.Z + 1, la.NewValue1);
                    GameMap.Instance.SetTileCornerHeight(la.X + 1, la.Z + 1, la.NewValue2);
                    GameMap.Instance.SetTileCornerHeight(la.X + 1, la.Z, la.NewValue3);
                    break;

                case EditorAction.SetTileId:
                    GameMap.Instance.SetTileId(la.X, la.Z, (short)la.NewValue0);
                    break;
            }
        }
    }

    struct InvokedEditorAction
    {
        public EditorAction Action { get; }

        public int NewValue0 { get; }
        public int NewValue1 { get; }
        public int NewValue2 { get; }
        public int NewValue3 { get; }

        public int OldValue0 { get; }
        public int OldValue1 { get; }
        public int OldValue2 { get; }
        public int OldValue3 { get; }

        public int X { get; }
        public int Z { get; }

        public InvokedEditorAction(EditorAction action, int x, int z,
                                   int oldValue0, int oldValue1, int oldValue2, int oldValue3,
                                   int newValue0, int newValue1, int newValue2, int newValue3) : this()
        {
            Action = action;
            X = x;
            Z = z;
            OldValue0 = oldValue0;
            OldValue1 = oldValue1;
            OldValue2 = oldValue2;
            OldValue3 = oldValue3;
            NewValue0 = newValue0;
            NewValue1 = newValue3;
            NewValue2 = newValue2;
            NewValue3 = newValue1;
        }

        public InvokedEditorAction(EditorAction action, int x, int z, int oldValue0, int newValue0) : this(action, x, z,
                                                                                                           oldValue0, 0, 0, 0,
                                                                                                           newValue0, 0, 0, 0)
        {

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
