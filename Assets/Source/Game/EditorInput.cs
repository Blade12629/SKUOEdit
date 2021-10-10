﻿using Assets.Source.Game.Map;
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

        int _terrainLayerMask;

        public EditorInput()
        {
            Instance = this;
            _terrainLayerMask = 1 << 0;

            CurrentAction = EditorAction.IncreaseTileHeight;
            CurrentValue = 1;

            UI.MappingTools.Instance.SetHeightValueInput(CurrentValue);
        }

        public void Update()
        {
            if (DisableEditorInput)
                return;

            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                if (CurrentAction != EditorAction.None && Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit, 10000f, _terrainLayerMask))
                    {
                        int x = (int)hit.point.x;
                        int z = (int)hit.point.z;

                        switch (CurrentAction)
                        {
                            case EditorAction.IncreaseVerticeHeight:

                                GameMap.Instance.IncreaseTileCornerHeight(x, z, CurrentValue);
                                break;

                            case EditorAction.DecreaseVerticeHeight:
                                GameMap.Instance.IncreaseTileCornerHeight(x, z, -CurrentValue);
                                break;

                            case EditorAction.SetVerticeHeight:
                                GameMap.Instance.SetTileCornerHeight(x, z, CurrentValue);
                                break;

                            case EditorAction.IncreaseTileHeight:
                                GameMap.Instance.IncreaseTileHeight(x, z, CurrentValue);
                                break;

                            case EditorAction.DecreaseTileHeight:
                                GameMap.Instance.IncreaseTileHeight(x, z, -CurrentValue);
                                break;

                            case EditorAction.SetTileHeight:
                                GameMap.Instance.SetTileHeight(x, z, CurrentValue);
                                break;

                            case EditorAction.SetTileId:
                                GameMap.Instance.SetTileId(x, z, (short)CurrentTileId);
                                break;
                        }
                    }
                }
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
