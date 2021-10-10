using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using IP = UnityEngine.Input;


namespace Assets.Source
{
    public static class Input
    {
        public static Vector2 mouseScrollDelta => IP.mouseScrollDelta;
        public static Vector3 mousePosition => IP.mousePosition;

        static Dictionary<KeyCode, KeyCode> _replacedKeys;

        static Input()
        {
            _replacedKeys = new Dictionary<KeyCode, KeyCode>();
        }

        public static bool GetKey(KeyCode key)
            => IP.GetKey(ReplaceKey(key));
        public static bool GetKeyDown(KeyCode key)
            => IP.GetKeyDown(ReplaceKey(key));
        public static bool GetKeyUp(KeyCode key)
            => IP.GetKeyUp(ReplaceKey(key));

        public static bool GetMouseButton(int button)
            => IP.GetMouseButton(button);
        public static bool GetMouseButtonDown(int button)
            => IP.GetMouseButtonDown(button);
        public static bool GetMouseButtonUp(int button)
            => IP.GetMouseButtonUp(button);

        public static float GetAxis(string axisName)
            => IP.GetAxis(axisName);

        public static float GetAxisRaw(string axisName)
            => IP.GetAxisRaw(axisName);

        public static void ReplaceKey(KeyCode original, KeyCode replaced)
        {
            _replacedKeys[original] = replaced;
        }

        public static void DeleteReplacement(KeyCode original)
        {
            _replacedKeys.Remove(original);
        }

        static KeyCode ReplaceKey(KeyCode key)
        {
            if (_replacedKeys.ContainsKey(key))
                return _replacedKeys[key];

            return key;
        }
    }
}
