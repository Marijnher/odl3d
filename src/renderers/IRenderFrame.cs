namespace odl3d;

/// <summary>Owns one acquired frame and its render-pass command encoder.</summary>
public interface IRenderFrame : System.IDisposable
{
    IRenderCommandEncoder BeginRenderPass();
    void EndRenderPass(IRenderCommandEncoder encoder);
}
