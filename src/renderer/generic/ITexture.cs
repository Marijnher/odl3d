using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents a texture resource that encapsulates its dimensions, format, usage, sample count, and provides functionality for uploading texture data.
/// </summary>
public interface ITexture : IGPUResource
{
    /// <summary>
    /// Gets the width of the texture in pixels.
    /// </summary>
    uint Width { get; }

    /// <summary>
    /// Gets the height of the texture in pixels.
    /// </summary>
    uint Height { get; }

    /// <summary>
    /// Gets the depth of the texture in pixels.
    /// </summary>
    int Depth { get; }

    /// <summary>
    /// Gets the format of the texture.
    /// </summary>
    TextureFormat Format { get; }

    /// <summary>
    /// Gets the usage flags of the texture.
    /// </summary>
    TextureUsage Usage { get; }

    /// <summary>
    /// Gets the number of samples per texel for the texture.
    /// </summary>
    int SampleCount { get; }

    /// <summary>
    /// Uploads texture data to the specified mip level.
    /// </summary>
    /// <param name="data">The texture data to upload.</param>
    /// <param name="mipLevel">The mip level to upload the data to.</param>
    void Upload(byte[] data, int mipLevel = 0);
}