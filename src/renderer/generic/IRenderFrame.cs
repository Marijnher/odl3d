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
    /// Gets the color texture associated with the frame.
    /// </summary>
    ITexture ColorTexture { get; }

    /// <summary>
    /// Creates a render pass for the frame with the specified description.
    /// </summary>
    /// <param name="renderPassDescription">The description of the render pass to create.</param>
    /// <returns>The created render pass.</returns>
    IRenderPass CreateRenderPass(RenderPassDescription renderPassDescription);
    
    /// <summary>
    /// Presents the frame, making it visible on the screen.
    /// </summary>
    void Present();
}