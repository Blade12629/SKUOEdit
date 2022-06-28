using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.UI.Special
{
    public class DisableOnReleaseBuild : MonoBehaviour
    {
        void Start()
        {
            if (!Debug.isDebugBuild)
                gameObject.SetActive(false);
        }
    }
}
