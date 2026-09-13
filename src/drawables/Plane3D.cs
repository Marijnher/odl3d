using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Represents a 3D plane object in the scene, defined by its width, height, and length, with an optional texture. Inherits from the Object class and uses a plane mesh for rendering.
/// </summary>
class Plane3D : Object3D
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plane3D"/> class, creating a 3D plane object with the specified width, height, length, and optional texture. The plane is added to the provided scene and uses a mesh created by the Mesh.CreatePlane method for rendering.
    /// </summary>
    /// <param name="scene">The scene to which this plane belongs.</param>
    /// <param name="width">The width of the plane.</param>
    /// <param name="height">The height of the plane.</param>
    /// <param name="length">The length of the plane.</param>
    /// <param name="texture">The texture for the plane, or null to draw without a texture.</param>
    public Plane3D(Scene<Object3D> scene, float width, float height, float length, Texture? texture = null) :
        base(scene, Mesh.CreatePlane(width, height, length), texture) { }
}