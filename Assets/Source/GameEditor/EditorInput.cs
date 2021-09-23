using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.GameEditor
{
    public class EditorInput
    {
        public EditorAction CurrentAction { get; set; }
        public float CurrentValue { get; set; }

        int _terrainLayerMask;
        MapController _mapController;

        public EditorInput(int terrainLayerMask, MapController mapController)
        {
            _terrainLayerMask = terrainLayerMask;
            _mapController = mapController;

            CurrentAction = EditorAction.IncreaseTileHeight;
            CurrentValue = 1f;
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0) && CurrentAction != EditorAction.None)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 10000f, _terrainLayerMask))
                {
                    switch (CurrentAction)
                    {
                        case EditorAction.IncreaseVerticeHeight:
                            _mapController.IncreaseVerticeHeight(hit.point.x, hit.point.z, CurrentValue);
                            break;

                        case EditorAction.DecreaseVerticeHeight:
                            _mapController.IncreaseVerticeHeight(hit.point.x, hit.point.z, -CurrentValue);
                            break;

                        case EditorAction.SetVerticeHeight:
                            _mapController.SetVerticeHeight(hit.point.x, hit.point.z, CurrentValue);
                            break;

                        case EditorAction.IncreaseTileHeight:
                            _mapController.IncreaseTileHeight(hit.point.x, hit.point.z, CurrentValue);
                            break;

                        case EditorAction.DecreaseTileHeight:
                            _mapController.IncreaseTileHeight(hit.point.x, hit.point.z, -CurrentValue);
                            break;

                        case EditorAction.SetTileHeight:
                            _mapController.SetTileHeight(hit.point.x, hit.point.z, CurrentValue);
                            break;

                        case EditorAction.SetTileId:
                            _mapController.SetTileId(hit.point.x, hit.point.z, (short)CurrentValue);
                            break;
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
