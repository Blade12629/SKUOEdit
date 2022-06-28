using System;
using System.Collections.Generic;
using System.IO;
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

    public static T GetOrAdd<T>(this GameObject obj) where T : Component
    {
        T comp = obj.GetComponent<T>();

        if (comp == null)
            return obj.AddComponent<T>();

        return comp;
    }

    public static T GetOrAdd<T>(this Component comp) where T : Component
    {
        return comp.GetOrAdd<T>();
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

    public static void Write(this BinaryWriter w, Color32 c)
    {
        w.Write(c.r);
        w.Write(c.g);
        w.Write(c.b);
        w.Write(c.a);
    }

    public static Color32 ReadColor32(this BinaryReader r)
    {
        return new Color32(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
    }

    public static Vector3 ReadVector3(this BinaryReader r)
    {
        return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    }

    public static Vector2 ReadVector2(this BinaryReader r)
    {
        return new Vector2(r.ReadSingle(), r.ReadSingle());
    }

    public static void AddRange<T>(this List<T> list, params T[] values)
    {
        if (values == null || values.Length == 0)
            return;

        list.AddRange(values);
    }

    public static System.Drawing.Bitmap ToBitmap(this Texture2D texture)
    {
        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(texture.width, texture.height);
        Color32[] texPixels = texture.GetPixels32();

        int index = 0;
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                ref Color32 color = ref texPixels[index++];
                bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(color.a, color.r, color.g, color.b));
            }
        }

        return bmp;
    }
}
