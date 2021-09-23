using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    [ExecuteInEditMode]
    class SetNeighbours : MonoBehaviour
    {
        public Terrain ThisTerrain, Left, Right, Top, Bottom;
        public void Start()
        {
            ThisTerrain.SetNeighbors(Left, Top, Right, Bottom);
            Debug.Log("Set Neighbours on" + ThisTerrain.name);
        }
    }
}
