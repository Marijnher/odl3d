using System;

namespace odl3d.Renderer;

public interface IRenderCapabilities
{
    bool SupportsWireframe { get; }
    bool SupportsComputeShaders { get; }
    bool SupportsGeometryShaders { get; }
    bool SupportsAnisotropicFiltering { get; }
    bool SupportsIndependentBlend { get; }
    bool SupportsIndirectDraw { get; }

    int MaxTextureSize { get; }
    int MaxAnisotropy { get; }
    int MaxVertexBufferSlots { get; }
    int MaxTextureSlots { get; }
}