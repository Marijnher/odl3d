using System;
using Assimp;

namespace odl3d;

public static class DaeLoader
{
    public static (Mesh[], Texture[]) Load(string filename, string textureFolder, float scale = 1f)
    {
        using var importer = new AssimpContext();

        var mScene = importer.ImportFile(
            filename,
            PostProcessSteps.Triangulate |
            PostProcessSteps.GenerateSmoothNormals |
            PostProcessSteps.FlipUVs
        );

        foreach (var mat in mScene.Materials)
        {
            if (mat.HasTextureDiffuse)
            {
                Assimp.TextureSlot tex;
                mat.GetMaterialTexture(TextureType.Diffuse, 0, out tex);
                Console.WriteLine(tex.FilePath);
            }
        }

        Mesh[] meshes = new Mesh[mScene.MeshCount];
        Texture[] textures = new Texture[mScene.MeshCount];

        for (int m = 0; m < mScene.MeshCount; m++)
        {
            Assimp.Mesh mMesh = mScene.Meshes[m];
            float[] vertices = new float[mMesh.VertexCount * 5];
            uint[] indices = new uint[mMesh.FaceCount * 3];
            for (int i = 0; i < mMesh.FaceCount; i++)
            {
                indices[i * 3 + 0] = (uint) mMesh.Faces[i].Indices[0];
                indices[i * 3 + 1] = (uint) mMesh.Faces[i].Indices[1];
                indices[i * 3 + 2] = (uint) mMesh.Faces[i].Indices[2];
            }
            for (int i = 0; i < mMesh.VertexCount; i++)
            {
                vertices[i * 5 + 0] = mMesh.Vertices[i].X / 50f * scale;
                vertices[i * 5 + 1] = mMesh.Vertices[i].Y / 50f * scale;
                vertices[i * 5 + 2] = mMesh.Vertices[i].Z / 50f * scale;
                vertices[i * 5 + 3] = mMesh.TextureCoordinateChannels[0][i].X;
                vertices[i * 5 + 4] = mMesh.TextureCoordinateChannels[0][i].Y;
            }
            // Create an Object for this mesh
            meshes[m] = new Mesh(vertices, indices);
            string texFilename = mScene.Materials[mMesh.MaterialIndex].TextureDiffuse.FilePath;
            textures[m] = new Texture(textureFolder + "/" + texFilename);
        }
        return (meshes, textures);
    }
}