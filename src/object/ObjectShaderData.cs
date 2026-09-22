using System;
using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 256)]
public struct ObjectShaderData
{
    public Matrix4x4 Model;
    public Vector4 TexColor;
    public Vector4 ObjColor;
    public uint UseTexture;
    public uint HasNormals;
}