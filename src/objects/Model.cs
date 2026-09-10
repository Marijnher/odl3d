using System;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.VisualBasic;

namespace odl3d;

public class Model : Object
{
    public List<Object> Objects = new List<Object>();

    public Model(Scene<Object> scene, Mesh[] meshes, Texture?[] textures) : base(scene)
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            // Creating this as an object means that a Model's sub-objects will also be maintained by the Scene,
            // not just by this Model class.
            Object obj = new Object(scene, meshes[i], textures[i]);
            Objects.Add(obj);
        }
    }

    public static Model LoadDAE(Scene<Object> scene, string filename)
    {
        string? daeFolder = System.IO.Path.GetDirectoryName(filename);
        if (daeFolder == null) throw new ArgumentException("Invalid filename: " + filename);
        (Mesh[] meshes, Texture[] textures) = DaeLoader.Load(filename, daeFolder);
        return new Model(scene, meshes, textures);
    }

    public static Model LoadOBJ(Scene<Object> scene, string objFilename)
    {
        string? objFolder = System.IO.Path.GetDirectoryName(objFilename);
        if (objFolder == null) throw new ArgumentException("Invalid filename: " + objFilename);
        ObjFile objFile = ObjLoader.Load(objFilename);
        Dictionary<string, Material> materials = new Dictionary<string, Material>();
        if (objFile.MtlFilename != null)
        {
            materials = MtlLoader.Load(objFolder + "/" + objFile.MtlFilename);
        }
        Mesh[] meshes = new Mesh[objFile.Meshes.Count];
        Texture?[] textures = new Texture?[objFile.Meshes.Count];
        int idx = 0;
        foreach (var kvp in objFile.Meshes)
        {
            string objectName = kvp.Key;
            ObjGroup objGroup = kvp.Value;
            Mesh groupMesh = objGroup.Mesh;
            string? mtlName = objGroup.MtlName;
            Texture? tex = null;
            if (mtlName != null && materials.ContainsKey(mtlName))
            {
                Material mat = materials[mtlName];
                tex = new Texture(objFolder + "/" + mat.Name + ".png");
            }
            meshes[idx] = groupMesh;
            textures[idx] = tex;
            idx++;
        }
        return new Model(scene, meshes, textures);
    }

    public override void Draw(Shader shader, Matrix4x4 viewProjection)
    {
        foreach (var obj in Objects)
        {
            obj.Position = Position;
            obj.Rotation = Rotation;
            obj.Visible = Visible;
            if (Texture != null) obj.Texture = Texture;
            obj.Scale = Scale;
            obj.Color = Color;
            obj.TextureColor = TextureColor;
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