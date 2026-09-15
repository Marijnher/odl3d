using System;

namespace odl3d.Renderer;

public interface ITexture : IGPUResource
{
    int Width { get; }
    int Height { get; }
    int Depth { get; }

    TextureFormat Format { get; }
    TextureUsage Usage { get; }

    int MipLevels { get; }
    int SampleCount { get; }

    void Upload(ReadOnlySpan<byte> data, int mipLevel = 0);

    void GenerateMipmaps();
}