using System;
using System.Numerics;

namespace odl3d;

public abstract class Drawable : InputHost
{
    /// <summary>
    /// The position of the object in world space.
    /// </summary>
    public Vector3 Position = Vector3.Zero;

    /// <summary>
    /// Indicates whether this drawable object is visible. If set to false, the object will not be rendered.
    /// </summary>
    public bool Visible = true;
}