using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a texture, including its dimensions, format, depth, sample count, and usage.
/// </summary>
public sealed record TextureDescription
{
    /// <summary>
    /// The width of the texture in texels.
    /// </summary>
    public required uint Width { get; init; }

    /// <summary>
    /// The height of the texture in texels.
    /// </summary>
    public required uint Height { get; init; }

    /// <summary>
    /// The format of the texture, specifying how texel data is stored and interpreted.
    /// </summary>
    public required TextureFormat Format { get; init; }

    /// <summary>
    /// The depth of the texture in texels. For 2D textures, this is typically 1.
    /// </summary>
    public int Depth { get; init; } = 1;

    /// <summary>
    /// The number of samples per texel for multisampled textures.
    /// </summary>
    public int SampleCount { get; init; } = 1;

    /// <summary>
    /// The intended usage of the texture, such as sampled, render target, or storage.
    /// </summary>
    public TextureUsage Usage { get; init; } = TextureUsage.Sampled;
}