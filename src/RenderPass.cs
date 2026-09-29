using System;

namespace odl3d;

/// <summary>
/// Identifies which rendering pass is currently active for a Scene3D: opaque geometry is drawn first with normal depth writes, then transparent geometry is drawn with depth writes disabled so it blends correctly against whatever was drawn behind it, regardless of scene/model ordering.
/// </summary>
public enum RenderPass
{
    /// <summary>
    /// Represents the rendering pass for opaque geometry, where depth writes are enabled and the geometry is drawn first.
    /// </summary>
    Opaque,

    /// <summary>
    /// Represents the rendering pass for transparent geometry, where depth writes are disabled and the geometry is drawn after opaque geometry to ensure correct blending.
    /// </summary>
    Transparent
}