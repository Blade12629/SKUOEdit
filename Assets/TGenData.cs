using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public class TGenData : MonoBehaviour
    {
        public GameObject[] Trees;
        public GameObject Wall_East;
        public GameObject Wall_North;
        public GameObject Corner_Large;
        public GameObject Corner_Small;
        public GameObject Floor;

        public GameObject Roof_North;
        public GameObject Roof_South;
        public GameObject Roof_East;
        public GameObject Roof_West;
        public GameObject Roof_Flat;

        public Texture2D Grass, Dirt, Sand, Ice, Cliff;

        public Texture2D Jungle, Swamp;

        public Texture2D CobbleStone;
        public Texture2D WoodFloor;
        public RenderTexture text;
    }
}
