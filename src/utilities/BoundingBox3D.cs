using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// An axis-aligned bounding box in 3D space.
/// </summary>
public readonly struct BoundingBox3D
{
    public Vector3 Min { get; }
    public Vector3 Max { get; }
    public bool IsEmpty { get; }

    public static BoundingBox3D Empty => new(true);

    public BoundingBox3D(Vector3 min, Vector3 max)
    {
        if (min.X > max.X || min.Y > max.Y || min.Z > max.Z)
            throw new ArgumentException("Minimum bounds must not exceed maximum bounds.");

        Min = min;
        Max = max;
        IsEmpty = false;
    }

    private BoundingBox3D(bool isEmpty)
    {
        Min = Vector3.Zero;
        Max = Vector3.Zero;
        IsEmpty = isEmpty;
    }

    public BoundingBox3D Include(Vector3 point)
    {
        if (IsEmpty) return new BoundingBox3D(point, point);
        return new BoundingBox3D(Vector3.Min(Min, point), Vector3.Max(Max, point));
    }

    public BoundingBox3D Union(BoundingBox3D other)
    {
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;
        return new BoundingBox3D(Vector3.Min(Min, other.Min), Vector3.Max(Max, other.Max));
    }

    public BoundingBox3D Transform(Matrix4x4 matrix)
    {
        if (IsEmpty) return this;

        BoundingBox3D transformed = Empty;
        for (int x = 0; x < 2; x++)
        for (int y = 0; y < 2; y++)
        for (int z = 0; z < 2; z++)
        {
            Vector3 corner = new(
                x == 0 ? Min.X : Max.X,
                y == 0 ? Min.Y : Max.Y,
                z == 0 ? Min.Z : Max.Z);
            transformed = transformed.Include(Vector3.Transform(corner, matrix));
        }
        return transformed;
    }
}