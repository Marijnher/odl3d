using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the different usage flags that can be applied to a texture.
/// </summary>
[Flags]
public enum TextureUsage
{
    /// <summary>
    /// No usage flags are set; the texture is not used for any specific purpose.
    /// </summary>
    None                    = 0x00,
    /// <summary>
    /// The texture can be sampled in shaders.
    /// </summary>
    Sampled                 = 0x01,
    /// <summary>
    /// The texture can be used as a render target.
    /// </summary>
    RenderTarget            = 0x02,
    /// <summary>
    /// The texture can be used as a depth-stencil attachment.
    /// </summary>
    DepthStencilAttachment  = 0x04,
    /// <summary>
    /// The texture can be used as a storage texture.
    /// </summary>
    Storage                 = 0x08,
    /// <summary>
    /// The texture can be used as a copy source.
    /// </summary>
    CopySource              = 0x10,
    /// <summary>
    /// The texture can be used as a copy destination.
    /// </summary>
    CopyDestination         = 0x20
}