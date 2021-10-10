using System.Runtime.InteropServices;

namespace Assets.Source.Game.Map
{
    /// <summary>
    /// Vertex holding position and uv coordinates
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct Vertex
    {
        public float X;
        public float Y;
        public float Z;

        public float UvX;
        public float UvY;

        public Vertex(float x, float y, float z, float uvX, float uvY) : this()
        {
            X = x;
            Y = y;
            Z = z;
            UvX = uvX;
            UvY = uvY;
        }

        public Vertex(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
