using System;

namespace odl3d.Renderers;

public static partial class MetalNew
{
    public sealed class CommandEncoder : ObjCObject
    {
        public CommandEncoder(IntPtr handle) : base(handle) { }

        /// <summary>Issues a non-indexed draw using the currently configured render pipeline.</summary>
        public void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount) =>
            Send("drawPrimitives:vertexStart:vertexCount:", (nuint) primitiveType, (nuint) vertexStart, (nuint) vertexCount);

        public void EndEncoding() => Send("endEncoding");
    }

    public enum PrimitiveType : ulong
    {
        Point = 0,
        Line = 1,
        LineStrip = 2,
        Triangle = 3,
        TriangleStrip = 4
    }
}
