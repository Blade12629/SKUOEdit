using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.Diagnostic
{
    public class FPSCounter : MonoBehaviour
    {
        float _timePassed;
        int _lastFPS;

        [SerializeField] Text _fpsText;

        void Update()
        {
            _timePassed += Time.unscaledDeltaTime;

            if (_timePassed >= 1)
            {
                _timePassed = 0;

                int fps = (int)(1f / Time.unscaledDeltaTime);

                if (_lastFPS != fps)
                {
                    _lastFPS = fps;
                    _fpsText.text = $"FPS\n{_lastFPS}";
                }
            }
        }
    }
}
