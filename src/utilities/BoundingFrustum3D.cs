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

    public bool Intersects(BoundingBox3D bounds) => bounds.IsEmpty ||
        !Outside(bounds, _left) &&
        !Outside(bounds, _right) &&
        !Outside(bounds, _bottom) &&
        !Outside(bounds, _top) &&
        !Outside(bounds, _near) &&
        !Outside(bounds, _far);

    private static Vector4 Normalize(Vector4 plane)
    {
        float length = new Vector3(plane.X, plane.Y, plane.Z).Length();
        return length > 0 ? plane / length : plane;
    }

    private static bool Outside(BoundingBox3D bounds, Vector4 plane)
    {
        Vector3 positiveVertex = new(
            plane.X >= 0 ? bounds.Max.X : bounds.Min.X,
            plane.Y >= 0 ? bounds.Max.Y : bounds.Min.Y,
            plane.Z >= 0 ? bounds.Max.Z : bounds.Min.Z);
        return Vector3.Dot(new Vector3(plane.X, plane.Y, plane.Z), positiveVertex) + plane.W < 0;
    }
}