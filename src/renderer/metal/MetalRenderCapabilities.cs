namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents the rendering capabilities of the Metal graphics API.
/// </summary>
internal sealed class MetalRenderCapabilities : IRenderCapabilities
{
    private const int MetalMaxAnisotropy = 16;
    private const int MetalMaxTextureSlots = 128;

    public bool SupportsWireframe => true;
    public bool SupportsComputeShaders => true;
    public bool SupportsGeometryShaders => false;
    public bool SupportsAnisotropicFiltering => true;
    public bool SupportsIndependentBlend => true;
    public bool SupportsIndirectDraw => true;
    public int MaxTextureSize { get; }
    public int MaxAnisotropy => MetalMaxAnisotropy;
    public int MaxVertexBufferSlots => BufferSlots.MaxVertexBuffers;
    public int MaxTextureSlots => MetalMaxTextureSlots;

    public MetalRenderCapabilities(Metal.Device device)
    {
        MaxTextureSize = checked((int) device.MaxTextureDimension2D);
    }
}