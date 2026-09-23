using System;

namespace odl3d.Renderer.MetalAdapter;

internal class MetalRenderCapabilities : IRenderCapabilities
{
    public bool SupportsWireframe => throw new NotImplementedException();
    public bool SupportsComputeShaders => throw new NotImplementedException();
    public bool SupportsGeometryShaders => throw new NotImplementedException();
    public bool SupportsAnisotropicFiltering => throw new NotImplementedException();
    public bool SupportsIndependentBlend => throw new NotImplementedException();
    public bool SupportsIndirectDraw => throw new NotImplementedException();

    public int MaxTextureSize => throw new NotImplementedException();
    public int MaxAnisotropy => throw new NotImplementedException();
    public int MaxVertexBufferSlots => throw new NotImplementedException();
    public int MaxTextureSlots => throw new NotImplementedException();
}