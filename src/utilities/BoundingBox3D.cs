using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// An axis-aligned bounding box in 3D space.
/// </summary>
public readonly struct BoundingBox3D
{
    /// <summary>
    /// The minimum corner of the bounding box.
    /// </summary>
    public Vector3 Min { get; }

    /// <summary>
    /// The maximum corner of the bounding box.
    /// </summary>
    public Vector3 Max { get; }

    /// <summary>
    /// Indicates whether the bounding box is empty.
    /// </summary>
    public bool IsEmpty { get; }

    /// <summary>
    /// Represents an empty bounding box.
    /// </summary>
    public static BoundingBox3D Empty => new(true);

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox3D"/> class with the specified minimum and maximum corners.
    /// </summary>
    /// <param name="min">The minimum corner of the bounding box.</param>
    /// <param name="max">The maximum corner of the bounding box.</param>
    /// <exception cref="ArgumentException">Thrown when the minimum corner exceeds the maximum corner.</exception>
    public BoundingBox3D(Vector3 min, Vector3 max)
    {
        if (min.X > max.X || min.Y > max.Y || min.Z > max.Z)
            throw new ArgumentException("Minimum bounds must not exceed maximum bounds.");

        Min = min;
        Max = max;
        IsEmpty = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox3D"/> class as empty.
    /// </summary>
    /// <param name="isEmpty">Indicates whether the bounding box should be initialized as empty.</param>
    private BoundingBox3D(bool isEmpty)
    {
        Min = Vector3.Zero;
        Max = Vector3.Zero;
        IsEmpty = isEmpty;
    }

    /// <summary>
    /// Returns a new bounding box that includes the specified point.
    /// </summary>
    /// <param name="point">The point to include in the bounding box.</param>
    /// <returns>A new bounding box that includes the specified point.</returns>
    public BoundingBox3D Include(Vector3 point)
    {
        if (IsEmpty) return new BoundingBox3D(point, point);
        return new BoundingBox3D(Vector3.Min(Min, point), Vector3.Max(Max, point));
    }

    /// <summary>
    /// Returns a new bounding box that represents the union of the current bounding box and the specified bounding box.
    /// </summary>
    /// <param name="other">The other bounding box to union with.</param>
    /// <returns>A new bounding box that represents the union of the current bounding box and the specified bounding box.</returns>
    public BoundingBox3D Union(BoundingBox3D other)
    {
        if (IsEmpty) return other;
        if (other.IsEmpty) return this;
        return new BoundingBox3D(Vector3.Min(Min, other.Min), Vector3.Max(Max, other.Max));
    }

    /// <summary>
    /// Returns a new bounding box that represents the current bounding box transformed by the specified matrix.
    /// </summary>
    /// <param name="matrix">The transformation matrix to apply to the bounding box.</param>
    /// <returns>A new bounding box that represents the current bounding box transformed by the specified matrix.</returns>
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