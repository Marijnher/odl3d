using System;
using System.Numerics;
using System.Collections.Generic;

namespace odl3d;

public class Model : Object
{
    public List<Object> Objects = new List<Object>();

    public Model(Scene<Object> scene, string filename) : base(scene)
    {
        string? daeFolder = System.IO.Path.GetDirectoryName(filename);
        if (daeFolder == null) throw new ArgumentException("Invalid filename: " + filename);
        (Mesh[] meshes, Texture[] textures) = DaeLoader.Load(filename, daeFolder);
        for (int i = 0; i < meshes.Length; i++)
        {
            // Creating this as an object means that a Model's sub-objects will also be maintained by the Scene,
            // not just by this Model class.
            Object obj = new Object(Scene, meshes[i], textures[i]);
            Objects.Add(obj);
        }
    }

    public override void Draw(Shader shader, Matrix4x4 viewProjection)
    {
        foreach (var obj in Objects)
        {
            obj.Position = Position;
            obj.Visible = Visible;
            obj.Draw(shader, viewProjection);
        }
    }

    public override void Dispose()
    {
        if (Disposed) return;
        base.Dispose();
        foreach (var obj in Objects)
        {
            obj.Dispose();
        }
        Objects.Clear();
    }
}