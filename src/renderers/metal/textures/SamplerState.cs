using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class SamplerDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLSamplerDescriptor");

        private SamplerDescriptor(IntPtr handle) : base(handle, true) { }

        public static SamplerDescriptor Create() =>
            new SamplerDescriptor(SendRaw(ClassPointer, "new"));

        public void SetMinFilter(nuint filter) =>
            Send("setMinFilter:", filter);

        public void SetMagFilter(nuint filter) =>
            Send("setMagFilter:", filter);

        public void SetMipFilter(nuint filter) =>
            Send("setMipFilter:", filter);

        public void SetAddressModeS(nuint wrap) =>
            Send("setSAddressMode:", wrap);

        public void SetAddressModeT(nuint wrap) =>
            Send("setTAddressMode:", wrap);

        public void SetAddressModeR(nuint wrap) =>
            Send("setRAddressMode:", wrap);

        public void SetMaxAnisotropy(nuint filter) =>
            Send("setMaxAnisotropy:", filter);
    }

    public sealed class SamplerState : ObjCObject
    {
        public SamplerState(IntPtr handle) : base(handle, true) { }
    }
}
