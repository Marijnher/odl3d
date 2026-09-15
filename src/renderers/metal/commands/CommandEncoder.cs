using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class CommandEncoder : ObjCObject
    {
        public CommandEncoder(IntPtr handle) : base(handle) { }

        /// <summary>Issues a non-indexed draw using the currently configured render pipeline.</summary>
        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount) =>
            Send("drawPrimitives:vertexStart:vertexCount:", (nuint) primitiveType, (nuint) vertexStart, (nuint) vertexCount);

        public void DrawIndexedPrimitives(
                Buffer indexBuffer,
                uint? indexCount = null,
                IndexType indexType = IndexType.UInt32,
                nuint indexBufferOffset = 0,
                PrimitiveType primitiveType = PrimitiveType.Triangle
            ) =>
            Send("drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:",
                (nuint) primitiveType, indexCount ?? indexBuffer.Length, (nuint) indexType, indexBuffer.Handle, indexBufferOffset);

        public void EndEncoding() => Send("endEncoding");

        public void SetRenderPipelineState(RenderPipelineState pipeline) =>
            Send("setRenderPipelineState:", pipeline);

        public void SetVertexBuffer(Buffer buffer, nuint offset = 0, nuint index = 0) =>
            Send("setVertexBuffer:offset:atIndex:", buffer.Handle, offset, index);

        public void SetFragmentTexture(Texture texture, nuint index = 0) =>
            Send("setFragmentTexture:atIndex:", texture.Handle, index);
    }

    public enum PrimitiveType : ulong
    {
        Point = 0,
        Line = 1,
        LineStrip = 2,
        Triangle = 3,
        TriangleStrip = 4
    }

    public enum IndexType : ulong
    {
        UInt16 = 0,
        UInt32 = 1
    }
}
