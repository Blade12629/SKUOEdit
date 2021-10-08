using UnityEngine.Rendering;

namespace Assets.Source.Game
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
