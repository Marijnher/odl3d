using System;

namespace odl3d.Renderer;

public interface ITexture : IGPUResource
{
    uint Width { get; }
    uint Height { get; }
    int Depth { get; }

    TextureFormat Format { get; }
    TextureUsage Usage { get; }

    int SampleCount { get; }

    void Upload(byte[] data, int mipLevel = 0);
}