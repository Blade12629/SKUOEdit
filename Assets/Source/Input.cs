using System.Collections.Generic;
using UnityEngine;

using IP = UnityEngine.Input;


namespace Assets.Source
{
    /// <summary>
    /// This class is a wrapper around <see cref="UnityEngine.Input"/>
    /// <para>You can change any key to another by simply replacing it</para>
    /// <para>e.g. you want Key2 to count as Key1: ReplaceKey(Key2, Key1)</para>
    /// <para>e.g. you want Key1 to count as Key2 and vice versa: ReplaceKey(Key1, Key2) ReplaceKey(Key2, Key1)</para>
    /// </summary>
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

        public static bool GetKeyUnreplaced(KeyCode key)
            => IP.GetKey(key);
        public static bool GetKeyDownUnreplaced(KeyCode key)
            => IP.GetKeyDown(key);
        public static bool GetKeyUpUnreplaced(KeyCode key)
            => IP.GetKeyUp(key);

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

        public static void ReplaceKey(KeyCode original, KeyCode replacement)
        {
            _replacedKeys[original] = replacement;
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
