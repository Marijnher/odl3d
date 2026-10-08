using System.Numerics;

namespace odl3d;

/// <summary>
/// A view frustum extracted from a row-vector view-projection matrix using a zero-to-one depth range.
/// </summary>
public readonly struct BoundingFrustum3D
{
    private readonly Vector4 _left;
    private readonly Vector4 _right;
    private readonly Vector4 _bottom;
    private readonly Vector4 _top;
    private readonly Vector4 _near;
    private readonly Vector4 _far;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingFrustum3D"/> struct with the specified view-projection matrix.
    /// </summary>
    /// <param name="viewProjection">The row-vector view-projection matrix using a zero-to-one depth range.</param>
    public BoundingFrustum3D(Matrix4x4 viewProjection)
    {
        _left = Normalize(new Vector4(
            viewProjection.M11 + viewProjection.M14,
            viewProjection.M21 + viewProjection.M24,
            viewProjection.M31 + viewProjection.M34,
            viewProjection.M41 + viewProjection.M44));
        _right = Normalize(new Vector4(
            viewProjection.M14 - viewProjection.M11,
            viewProjection.M24 - viewProjection.M21,
            viewProjection.M34 - viewProjection.M31,
            viewProjection.M44 - viewProjection.M41));
        _bottom = Normalize(new Vector4(
            viewProjection.M12 + viewProjection.M14,
            viewProjection.M22 + viewProjection.M24,
            viewProjection.M32 + viewProjection.M34,
            viewProjection.M42 + viewProjection.M44));
        _top = Normalize(new Vector4(
            viewProjection.M14 - viewProjection.M12,
            viewProjection.M24 - viewProjection.M22,
            viewProjection.M34 - viewProjection.M32,
            viewProjection.M44 - viewProjection.M42));
        _near = Normalize(new Vector4(
            viewProjection.M13,
            viewProjection.M23,
            viewProjection.M33,
            viewProjection.M43));
        _far = Normalize(new Vector4(
            viewProjection.M14 - viewProjection.M13,
            viewProjection.M24 - viewProjection.M23,
            viewProjection.M34 - viewProjection.M33,
            viewProjection.M44 - viewProjection.M43));
    }

    /// <summary>
    /// Determines whether the bounding frustum intersects with the specified bounding box.
    /// </summary>
    /// <param name="bounds">The bounding box to test for intersection with the frustum.</param>
    /// <returns><c>true</c> if the bounding frustum intersects with the specified bounding box; otherwise, <c>false</c>.</returns>
    public bool Intersects(BoundingBox3D bounds) => bounds.IsEmpty ||
        !Outside(bounds, _left) &&
        !Outside(bounds, _right) &&
        !Outside(bounds, _bottom) &&
        !Outside(bounds, _top) &&
        !Outside(bounds, _near) &&
        !Outside(bounds, _far);

    /// <summary>
    /// Normalizes the specified plane equation so that the normal vector has a length of 1.
    /// </summary>
    /// <param name="plane">The plane equation to normalize.</param>
    /// <returns>The normalized plane equation.</returns>
    private static Vector4 Normalize(Vector4 plane)
    {
        float length = new Vector3(plane.X, plane.Y, plane.Z).Length();
        return length > 0 ? plane / length : plane;
    }

    /// <summary>
    /// Determines whether the specified bounding box is outside the given plane of the frustum.
    /// </summary>
    /// <param name="bounds">The bounding box to test.</param>
    /// <param name="plane">The plane equation to test against.</param>
    /// <returns><c>true</c> if the bounding box is outside the plane; otherwise, <c>false</c>.</returns>
    private static bool Outside(BoundingBox3D bounds, Vector4 plane)
    {
        Vector3 positiveVertex = new(
            plane.X >= 0 ? bounds.Max.X : bounds.Min.X,
            plane.Y >= 0 ? bounds.Max.Y : bounds.Min.Y,
            plane.Z >= 0 ? bounds.Max.Z : bounds.Min.Z);
        return Vector3.Dot(new Vector3(plane.X, plane.Y, plane.Z), positiveVertex) + plane.W < 0;
    }
}