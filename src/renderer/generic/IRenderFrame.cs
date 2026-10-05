using System;
using System.Collections.Generic;
using odl3d;

namespace odl3d.Renderer;

/// <summary>
/// Represents a frame in the rendering pipeline, providing access to the color texture and the ability to create render passes and present the frame.
/// </summary>
public interface IRenderFrame : IDisposable
{
    /// <summary>
    /// Creates a render pass for the frame with the specified description.
    /// </summary>
    /// <param name="renderPassDescription">The description of the render pass to create.</param>
    /// <returns>The created render pass.</returns>
    IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription);

    /// <summary>
    /// Waits for all work submitted to this frame to complete and reads back its color target.
    /// All render passes of the frame must be ended. The frame remains usable afterwards, so more passes
    /// can be recorded and read back before <see cref="Present"/>.
    /// </summary>
    /// <returns>Tightly packed RGBA8 pixels (Width * Height * 4 bytes), with row 0 being the top row.</returns>
    /// <exception cref="RenderException">Thrown when the frame does not belong to an offscreen surface, or a render pass is still open.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the frame has been presented or disposed.</exception>
    byte[] ReadPixels();
    
    /// <summary>
    /// Presents the frame and releases frame-scoped resources. After this call, the frame cannot be used again.
    /// Dispose releases an unpresented frame without presenting it.
    /// </summary>
    void Present();
}