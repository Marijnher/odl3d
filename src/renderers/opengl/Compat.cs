using System;

namespace odl3d
{
    public enum BufferTarget{ArrayBuffer,ElementBuffer}
    public enum BufferHint{Static,Dynamic,Stream}
    public class Buffer{public uint Handle;}
    public class VertexArray{public uint Handle;}
}