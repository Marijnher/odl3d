using System;
using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// Represents the shader data for an Object3D, including the model matrix, texture color, object color, and normal information.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 256)]
public struct ObjectShaderData
{
    /// <summary>
    /// The model matrix that transforms the object's local coordinates to world coordinates.
    /// </summary>
    public Matrix4x4 Model;

    /// <summary>
    /// The texture color to be used by the shader.
    /// </summary>
    public Vector4 TexColor;

    /// <summary>
    /// The object color to be used by the shader.
    /// </summary>
    public Vector4 ObjColor;

    private uint _useTexture;
    /// <summary>
    /// Indicates whether the object uses a texture (1 for true, 0 for false).
    /// </summary>
    public bool UseTexture { get => _useTexture == 1; set => _useTexture = value ? 1u : 0u; }

    private uint _hasNormals;
    /// <summary>
    /// Indicates whether the object has normals (1 for true, 0 for false).
    /// </summary>
    public bool HasNormals { get => _hasNormals == 1; set => _hasNormals = value ? 1u : 0u; }
}