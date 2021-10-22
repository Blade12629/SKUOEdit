using UnityEngine.Rendering;

namespace Assets.Source.Game.Map
{
    /// <summary>
    /// Vertex Layout
    /// </summary>
    public static class VertexLayout
    {
        /// <summary>
        /// Vertex Layout
        /// </summary>
        public static readonly VertexAttributeDescriptor[] Layout;
        public static readonly int VertexByteSize = 20; //X, Y, Z, UVX, UVY

        static VertexLayout()
        {
            Layout = new VertexAttributeDescriptor[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 0)
            };
        }
    }
}
