using System;
using System.Runtime.Intrinsics.Arm;

namespace odl3d.Renderer.MetalAdapter;

public class MetalTexture : ITexture
{
    private Metal.Device Device;
    public Metal.Texture Texture;

    public uint Width { get; }
    public uint Height { get; }
    public int Depth { get; }

    public TextureFormat Format { get; }
    public TextureUsage Usage { get; }

    public int SampleCount { get; }
    public bool Disposed { get; private set; }

    private bool HasValidMipmaps;

    public MetalTexture(Metal.Device device, TextureDescription description, byte[]? initialData = null)
    {
        Device = device;
        Width = description.Width;
        Height = description.Height;
        Texture = Device.CreateTexture(
            initialData ?? new byte[Width * Height * 4],
            Width, Height, description.Format
        );
    }

    public void Upload(byte[] data, int mipLevel = 0)
    {
        nuint mipWidth = Math.Max(1, Width >> mipLevel);
        nuint mipHeight = Math.Max(1, Height >> mipLevel);
        nuint mipSize = mipWidth * mipHeight;
        Texture.Upload(data, mipLevel, mipSize * 4, mipWidth, mipHeight);
        HasValidMipmaps = false;
    }

    public void ValidateMipmaps(Metal.Device device, Metal.CommandQueue commandQueue)
    {
        if (HasValidMipmaps) return;
        device.GenerateMipmaps(commandQueue, Texture);
        HasValidMipmaps = true;
    }

    public void Dispose()
    {
        if (Disposed) return;
        Texture.Dispose();
        Disposed = true;
    }
}