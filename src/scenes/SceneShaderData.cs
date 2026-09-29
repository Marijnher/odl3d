using System;
using System.Numerics;
using System.Runtime.InteropServices;

/// <summary>
/// Represents the shader data for an object, including the projection and view matrices.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 128)]
public struct SceneShaderData
{
    /// <summary>
    /// The projection matrix that transforms camera coordinates to clip space.
    /// </summary>
    public Matrix4x4 Projection;

    /// <summary>
    /// The view matrix that transforms world coordinates to camera coordinates.
    /// </summary>
    public Matrix4x4 View;
}