using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class Extensions
{
    public static void Switch<T>(this T[] array, int indexA, int indexB)
    {
        T a = array[indexA];
        array[indexA] = array[indexB];
        array[indexB] = a;
    }

    public static T Swap<T>(T obj, ref T other) where T : struct
    {
        T result = other;
        other = obj;
        return result;
    }

    public static Color ToColor(this ushort hue)
    {
        float r = ((hue & 0x7c00) >> 10) * (255 / 31);
        float g = ((hue & 0x3e0) >> 5) * (255 / 31);
        float b = (hue & 0x1f) * (255 / 31);
        float a = 1f;

        return new Color(r / 255f, g / 255f, b / 255f, a);
    }

    public static Vector2 Reverse(this Vector2 v)
    {
        return new Vector2(v.y, v.x);
    }

    public static float Min(this float a, params float[] other)
    {
        for (int i = 0; i < other.Length; i++)
            a = Math.Min(a, other[i]);

        return a;
    }

    public static float Max(this float a, params float[] other)
    {
        for (int i = 0; i < other.Length; i++)
            a = Math.Max(a, other[i]);

        return a;
    }

    public static int Swap(this int a, ref int b)
    {
        int c = a;

        a = b;
        b = c;

        return a;
    }

    public static void Write(this BinaryWriter w, Vector3 v)
    {
        w.Write(v.x);
        w.Write(v.y);
        w.Write(v.z);
    }

    public static void Write(this BinaryWriter w, Vector2 v)
    {
        w.Write(v.x);
        w.Write(v.y);
    }

    public static Vector3 ReadVector3(this BinaryReader r)
    {
        return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    }

    public static Vector2 ReadVector2(this BinaryReader r)
    {
        return new Vector2(r.ReadSingle(), r.ReadSingle());
    }

    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
}
