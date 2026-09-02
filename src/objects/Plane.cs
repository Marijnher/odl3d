using System;
using System.Numerics;

namespace odl3d;

class Plane : Object
{
    public Plane(Scene<Object> scene, float width, float height, float length, Texture? texture = null) :
        base(scene, Mesh.CreatePlane(width, height, length), texture) { }
}