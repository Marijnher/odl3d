using System;
using System.Runtime.Intrinsics.Arm;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a Metal texture and provides access to its properties.
/// </summary>
internal class MetalTexture : ITexture
{
    private Metal.Device Device;

    /// <summary>
    /// Gets the underlying Metal texture object.
    /// </summary>
    public Metal.Texture Texture;

    /// <summary>
    /// Gets the width of the texture.
    /// </summary>
    public uint Width { get; }

    /// <summary>
    /// Gets the height of the texture.
    /// </summary>
    public uint Height { get; }

    /// <summary>
    /// Gets the depth of the texture.
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Gets the format of the texture.
    /// </summary>
    public TextureFormat Format { get; }

    /// <summary>
    /// Gets the usage flags of the texture.
    /// </summary>
    public TextureUsage Usage { get; }

    /// <summary>
    /// Gets the sample count of the texture.
    /// </summary>
    public int SampleCount { get; }

    /// <summary>
    /// Gets a value indicating whether the texture has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the texture has valid mipmaps.
    /// </summary>
    private bool HasValidMipmaps;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalTexture"/> class.
    /// </summary>
    /// <param name="device">The Metal device used to create the texture.</param>
    /// <param name="description">The description of the texture.</param>
    /// <param name="initialData">The initial data for the texture, if any.</param>
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

    /// <summary>
    /// Uploads data to the specified mip level of the texture.
    /// </summary>
    /// <param name="data">The data to upload to the texture.</param>
    /// <param name="mipLevel">The mip level to which the data should be uploaded.</param>
    public void Upload(byte[] data, int mipLevel = 0)
    {
        nuint mipWidth = Math.Max(1, Width >> mipLevel);
        nuint mipHeight = Math.Max(1, Height >> mipLevel);
        Texture.Upload(data, (nuint) mipLevel, mipWidth * 4, mipWidth, mipHeight);
        HasValidMipmaps = false;
    }

    /// <summary>
    /// Validates and generates mipmaps for the texture if they are not already valid.
    /// </summary>
    /// <param name="device">The Metal device used to generate mipmaps.</param>
    /// <param name="commandQueue">The command queue used to submit the mipmap generation commands.</param>
    public void ValidateMipmaps(Metal.Device device, Metal.CommandQueue commandQueue)
    {
        if (HasValidMipmaps) return;
        device.GenerateMipmaps(commandQueue, Texture);
        HasValidMipmaps = true;
    }

    /// <summary>
    /// Disposes the texture and releases its resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Texture.Dispose();
        Disposed = true;
    }
}