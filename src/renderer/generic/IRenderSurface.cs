using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents a render surface that provides information about the surface dimensions, color and depth formats, and allows acquiring render frames.
/// </summary>
public interface IRenderSurface : IGPUResource
{
    /// <summary>
    /// Gets the width of the render surface.
    /// </summary>
    uint Width { get; }

    /// <summary>
    /// Gets the height of the render surface.
    /// </summary>
    uint Height { get; }

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization (VSync) is enabled for the render surface.
    /// </summary>
    bool VSync { get; set; }

    /// <summary>
    /// Gets the color format of the render surface.
    /// </summary>
    TextureFormat ColorFormat { get; }

    /// <summary>
    /// Gets the depth format of the render surface, if available.
    /// </summary>
    TextureFormat? DepthFormat { get; }

    /// <summary>
    /// Gets the sample count of the render surface.
    /// </summary>
    int SampleCount { get; }

    /// <summary>
    /// Resizes the render surface to the specified width and height.
    /// </summary>
    /// <param name="width">The new width of the render surface.</param>
    /// <param name="height">The new height of the render surface.</param>
    void Resize(int width, int height);

    /// <summary>
    /// Acquires a render frame from the render surface.
    /// </summary>
    /// <returns>The acquired render frame, or null if no frame is available.</returns>
    IRenderFrame? AcquireFrame();
}