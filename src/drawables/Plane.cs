using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Represents a 3D plane object in the scene, defined by its width, height, and length, with an optional texture. Inherits from the Object class and uses a plane mesh for rendering.
/// </summary>
class Plane : Object
{
    public Plane(Scene<Object> scene, float width, float height, float length, Texture? texture = null) :
        base(scene, Mesh.CreatePlane(width, height, length), texture) { }
}