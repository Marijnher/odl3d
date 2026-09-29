using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal sampler descriptor, which describes the properties of a texture sampler.
    /// </summary>
    public sealed class SamplerDescriptor : ObjCObject
    {
        /// <summary>
        /// Gets or sets the minification filter for the sampler.
        /// </summary>
        public nuint MinFilter
        {
            get => GetUInt32("minFilter");
            set => Send("setMinFilter:", value);
        }

        /// <summary>
        /// Gets or sets the magnification filter for the sampler.
        /// </summary>
        public nuint MagFilter
        {
            get => GetUInt32("magFilter");
            set => Send("setMagFilter:", value);
        }

        /// <summary>
        /// Gets or sets the mipmap filter for the sampler.
        /// </summary>
        public nuint MipFilter
        {
            get => GetUInt32("mipFilter");
            set => Send("setMipFilter:", value);
        }

        /// <summary>
        /// Gets or sets the address mode for the S (U) texture coordinate.
        /// </summary>
        public nuint AddressModeS
        {
            get => GetUInt32("sAddressMode");
            set => Send("setSAddressMode:", value);
        }

        /// <summary>
        /// Gets or sets the address mode for the T (V) texture coordinate.
        /// </summary>
        public nuint AddressModeT
        {
            get => GetUInt32("tAddressMode");
            set => Send("setTAddressMode:", value);
        }

        /// <summary>
        /// Gets or sets the address mode for the R (W) texture coordinate.
        /// </summary>
        public nuint AddressModeR
        {
            get => GetUInt32("rAddressMode");
            set => Send("setRAddressMode:", value);
        }

        /// <summary>
        /// Gets or sets the maximum anisotropy for the sampler.
        /// </summary>
        public nuint MaxAnisotropy
        {
            get => GetUInt32("maxAnisotropy");
            set => Send("setMaxAnisotropy:", value);
        }

        /// <summary>
        /// Gets the class pointer for the Metal sampler descriptor.
        /// </summary>
        private static IntPtr ClassPointer => Class("MTLSamplerDescriptor");

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerDescriptor"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal sampler descriptor object.</param>
        private SamplerDescriptor(IntPtr handle) : base(handle, true) { }

        /// <summary>
        /// Creates a new instance of the <see cref="SamplerDescriptor"/> class.
        /// </summary>
        /// <returns>A new <see cref="SamplerDescriptor"/> instance.</returns>
        public static SamplerDescriptor Create() =>
            new SamplerDescriptor(SendRaw(ClassPointer, "new"));
    }

    /// <summary>
    /// Represents a Metal sampler state, which encapsulates the configuration of a texture sampler.
    /// </summary>
    public sealed class SamplerState : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerState"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal sampler state object.</param>
        public SamplerState(IntPtr handle) : base(handle, true) { }
    }
}
