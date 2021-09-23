using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
