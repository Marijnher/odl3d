using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class DepthStencilDescriptor : ObjCObject
    {
        public static IntPtr ClassPointer => Class("MTLDepthStencilDescriptor");

        public DepthStencilDescriptor(IntPtr handle) : base(handle, true) { }

        public static DepthStencilDescriptor Create() =>
            new DepthStencilDescriptor(SendRaw(ClassPointer, "new"));

        public void SetDepthCompareFunction(CompareFunction function) =>
            Send("setDepthCompareFunction:", (nuint) function);

        public void SetDepthWriteEnabled(bool enabled) =>
            Send("setDepthWriteEnabled:", enabled ? 1 : 0);
    }

    public sealed class DepthStencilState : ObjCObject
    {
        public DepthStencilState(IntPtr handle) : base(handle, true) { }
    }
}
