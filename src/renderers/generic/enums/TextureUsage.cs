using System;

namespace odl3d.Renderer;

[Flags]
public enum TextureUsage
{
    None                    = 0x00,
    Sampled                 = 0x01,
    RenderTarget            = 0x02,
    DepthStencilAttachment  = 0x04,
    Storage                 = 0x08,
    CopySource              = 0x10,
    CopyDestination         = 0x20
}