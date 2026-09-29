using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Provides extension methods for converting Matrix4x4 instances to different representations.
/// </summary>
public static class MatrixConversion
{
    /// <summary>
    /// Converts the given Matrix4x4 instance to a one-dimensional float array in row-major order.
    /// </summary>
    /// <param name="matrix">The Matrix4x4 instance to convert.</param>
    /// <returns>A float array containing the matrix elements in row-major order.</returns>
    public static float[] ToArray(this Matrix4x4 matrix)
    {
        return [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        ];
    }
}