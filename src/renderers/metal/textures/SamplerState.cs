using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class SamplerDescriptor : ObjCObject
    {
        private static IntPtr ClassPointer => Class("MTLSamplerDescriptor");

        private SamplerDescriptor(IntPtr handle) : base(handle, true) { }

        public static SamplerDescriptor Create() =>
            new SamplerDescriptor(SendRaw(ClassPointer, "new"));

        public void SetMinFilter(odl3d.TextureFilter filter) =>
            Send("setMinFilter:", (nuint)filter);

        public void SetMagFilter(odl3d.TextureFilter filter) =>
            Send("setMagFilter:", (nuint)filter);

        public void SetMipFilter(odl3d.MipmapFilter filter) =>
            Send("setMipFilter:", (nuint)filter);

        public void SetAddressModeS(odl3d.TextureWrap wrap) =>
            Send("setSAddressMode:", AddressMode(wrap));

        public void SetAddressModeT(odl3d.TextureWrap wrap) =>
            Send("setTAddressMode:", AddressMode(wrap));

        public void SetMaxAnisotropy(AnisotropicFilter filter) =>
            Send("setMaxAnisotropy:", (nuint)filter);

        private static nuint AddressMode(odl3d.TextureWrap wrap) => wrap switch
        {
            odl3d.TextureWrap.Clamp => 0,
            odl3d.TextureWrap.Repeat => 2,
            odl3d.TextureWrap.Mirror => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(wrap))
        };
    }

    public sealed class SamplerState : ObjCObject
    {
        public SamplerState(IntPtr handle) : base(handle, true) { }
    }
}
