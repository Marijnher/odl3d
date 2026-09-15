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

        public void SetMinFilter(TextureFilter filter) =>
            Send("setMinFilter:", (nuint)filter);

        public void SetMagFilter(TextureFilter filter) =>
            Send("setMagFilter:", (nuint)filter);

        public void SetMipFilter(MipmapFilter filter) =>
            Send("setMipFilter:", (nuint)filter);

        public void SetAddressModeS(TextureWrap wrap) =>
            Send("setSAddressMode:", AddressMode(wrap));

        public void SetAddressModeT(TextureWrap wrap) =>
            Send("setTAddressMode:", AddressMode(wrap));

        public void SetMaxAnisotropy(AnisotropicFilter filter) =>
            Send("setMaxAnisotropy:", (nuint)filter);

        private static nuint AddressMode(TextureWrap wrap) => wrap switch
        {
            TextureWrap.Clamp => 0,
            TextureWrap.Repeat => 2,
            TextureWrap.Mirror => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(wrap))
        };
    }

    public sealed class SamplerState : ObjCObject
    {
        public SamplerState(IntPtr handle) : base(handle, true) { }
    }
}
