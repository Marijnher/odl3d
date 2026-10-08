using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Represents a drawable object in the 3D scene, providing properties for position, rotation, and visibility.
/// </summary>
public abstract class Drawable : InputHost
{
    /// <summary>
    /// The position of the object in world space.
    /// </summary>
    public Vector3 Position = Vector3.Zero;

    /// <summary>
    /// The rotation of the object in world space, represented as Euler angles (in degrees) around the X, Y, and Z axes.
    /// </summary>
    public Vector3 Rotation = Vector3.Zero;

    /// <summary>
    /// Indicates whether this drawable object is visible. If set to false, the object will not be rendered.
    /// </summary>
    public bool Visible = true;

    /// <summary>
    /// Gets this drawable's axis-aligned bounds in world space. Return <see cref="BoundingBox3D.Empty"/> when the drawable has no known bounds.
    /// </summary>
    public virtual BoundingBox3D GetWorldBounds() => BoundingBox3D.Empty;
}