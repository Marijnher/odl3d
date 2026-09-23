using System;
using System.Collections.Generic;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Triangulates a filled contour with any number of holes. Each hole is first bridged into the outer contour
/// with a zero-width cut, producing a single (degenerate but simple) ring, which is then ear-clipped.
/// Expects the outer contour wound counter-clockwise and every hole wound clockwise.
/// </summary>
internal static class PolygonTessellator
{
    private const float Epsilon = 1e-7f;

    /// <summary>
    /// Triangulates the given contour set.
    /// </summary>
    /// <param name="outer">The outer contour of the polygon, wound counter-clockwise.</param>
    /// <param name="holes">A list of hole contours, each wound clockwise.</param>
    /// <returns>
    /// The merged ring, containing every outer and hole vertex (bridge vertices appear twice), and the
    /// triangle list as index triplets into that ring.
    /// </returns>
    public static (List<Vector2> Polygon, List<int> Indices) Triangulate(List<Vector2> outer, List<List<Vector2>> holes)
    {
        List<Vector2> polygon = outer;
        if (holes.Count > 0)
        {
            // Bridging the rightmost hole first guarantees every later bridge still sees a simple polygon.
            List<List<Vector2>> ordered = new(holes);
            ordered.Sort((a, b) => MaxX(b).CompareTo(MaxX(a)));
            polygon = new List<Vector2>(outer);
            foreach (List<Vector2> hole in ordered) polygon = Bridge(polygon, hole);
        }

        return (polygon, EarClip(polygon));
    }

    /// <summary>
    /// Computes twice the signed area of the given contour.
    /// Positive for counter-clockwise contours in a Y-up coordinate system.
    /// </summary>
    /// <param name="contour">The contour for which to compute the signed area.</param>
    /// <returns>Twice the signed area of the contour. Positive for counter-clockwise contours in a Y-up coordinate system.</returns>
    public static float SignedArea(List<Vector2> contour)
    {
        float area = 0f;
        for (int i = 0, j = contour.Count - 1; i < contour.Count; j = i++)
            area += contour[j].X * contour[i].Y - contour[i].X * contour[j].Y;
        return area;
    }

    /// <summary>
    /// Determines whether the given point lies inside the specified contour using the crossing-number test.
    /// Points exactly on the boundary may report either result.
    /// </summary>
    /// <param name="contour">The contour to test against.</param>
    /// <param name="point">The point to test for containment.</param>
    /// <returns>True if the point lies inside the contour, false otherwise.</returns>
    public static bool ContainsPoint(List<Vector2> contour, Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = contour.Count - 1; i < contour.Count; j = i++)
        {
            Vector2 a = contour[i];
            Vector2 b = contour[j];
            if (a.Y > point.Y != b.Y > point.Y &&
                point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
                inside = !inside;
        }
        return inside;
    }

    private static float MaxX(List<Vector2> contour)
    {
        float max = float.NegativeInfinity;
        foreach (Vector2 point in contour) max = MathF.Max(max, point.X);
        return max;
    }

    private static List<Vector2> Bridge(List<Vector2> polygon, List<Vector2> hole)
    {
        int holeStart = 0;
        for (int i = 1; i < hole.Count; i++)
        {
            if (hole[i].X > hole[holeStart].X) holeStart = i;
        }

        int bridge = FindBridgeVertex(polygon, hole[holeStart]);
        if (bridge < 0) return polygon;

        List<Vector2> merged = new(polygon.Count + hole.Count + 2);
        for (int i = 0; i <= bridge; i++) merged.Add(polygon[i]);
        for (int i = 0; i < hole.Count; i++) merged.Add(hole[(holeStart + i) % hole.Count]);
        merged.Add(hole[holeStart]);
        for (int i = bridge; i < polygon.Count; i++) merged.Add(polygon[i]);
        return merged;
    }

    /// <summary>
    /// Finds a polygon vertex that can see <paramref name="origin"/> (the hole's rightmost point): the
    /// polygon edge first hit by a ray cast to the right, refined by any reflex vertex that blocks it.
    /// </summary>
    /// <param name="polygon">The outer polygon contour.</param>
    /// <param name="origin">The rightmost point of the hole to connect.</param>
    /// <returns>The index of the bridge vertex in the polygon, or -1 if no suitable vertex is found.</returns>
    private static int FindBridgeVertex(List<Vector2> polygon, Vector2 origin)
    {
        int count = polygon.Count;
        int candidate = -1;
        float closestX = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % count];
            if (a.Y > origin.Y == b.Y > origin.Y) continue;

            float x = a.X + (origin.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X);
            if (x < origin.X || x >= closestX) continue;

            closestX = x;
            candidate = a.X > b.X ? i : (i + 1) % count;
        }

        if (candidate < 0) return -1;

        Vector2 intersection = new Vector2(closestX, origin.Y);
        Vector2 corner = polygon[candidate];
        float bestTangent = Tangent(origin, corner);

        for (int i = 0; i < count; i++)
        {
            Vector2 vertex = polygon[i];
            if (vertex.X <= origin.X || !IsReflex(polygon, i)) continue;
            if (!PointInTriangle(origin, intersection, corner, vertex)) continue;

            float tangent = Tangent(origin, vertex);
            if (tangent < bestTangent)
            {
                bestTangent = tangent;
                candidate = i;
            }
        }

        return candidate;
    }

    private static float Tangent(Vector2 origin, Vector2 vertex)
    {
        float dx = vertex.X - origin.X;
        return dx <= 0f ? float.PositiveInfinity : MathF.Abs(vertex.Y - origin.Y) / dx;
    }

    private static bool IsReflex(List<Vector2> polygon, int index)
    {
        Vector2 previous = polygon[(index + polygon.Count - 1) % polygon.Count];
        Vector2 current = polygon[index];
        Vector2 next = polygon[(index + 1) % polygon.Count];
        return Cross(current - previous, next - current) < 0f;
    }

    private static List<int> EarClip(List<Vector2> polygon)
    {
        List<int> triangles = new();
        int count = polygon.Count;
        if (count < 3) return triangles;

        List<int> remaining = new(count);
        for (int i = 0; i < count; i++) remaining.Add(i);

        int attempts = 2 * count;
        int v = count - 1;
        while (remaining.Count > 2)
        {
            int n = remaining.Count;
            int u = v >= n ? 0 : v;
            v = u + 1 >= n ? 0 : u + 1;
            int w = v + 1 >= n ? 0 : v + 1;

            // After a full sweep without finding an ear the ring is self-intersecting or numerically
            // degenerate; clip anyway so the glyph is merely imperfect instead of missing.
            bool forced = --attempts <= 0;
            if (forced || IsEar(polygon, remaining, u, v, w))
            {
                if (!forced || MathF.Abs(Cross(polygon[remaining[v]] - polygon[remaining[u]], polygon[remaining[w]] - polygon[remaining[u]])) > Epsilon)
                {
                    triangles.Add(remaining[u]);
                    triangles.Add(remaining[v]);
                    triangles.Add(remaining[w]);
                }
                remaining.RemoveAt(v);
                attempts = 2 * remaining.Count;
                v = u;
            }
        }

        return triangles;
    }

    private static bool IsEar(List<Vector2> polygon, List<int> remaining, int u, int v, int w)
    {
        Vector2 a = polygon[remaining[u]];
        Vector2 b = polygon[remaining[v]];
        Vector2 c = polygon[remaining[w]];
        if (Cross(b - a, c - a) <= Epsilon) return false;

        for (int i = 0; i < remaining.Count; i++)
        {
            if (i == u || i == v || i == w) continue;
            Vector2 point = polygon[remaining[i]];
            // Bridge vertices are duplicated on purpose, so a coincident point never disqualifies an ear.
            if (point == a || point == b || point == c) continue;
            if (PointInTriangle(a, b, c, point)) return false;
        }

        return true;
    }

    private static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 point)
    {
        float ab = Cross(b - a, point - a);
        float bc = Cross(c - b, point - b);
        float ca = Cross(a - c, point - c);
        return (ab >= 0f && bc >= 0f && ca >= 0f) || (ab <= 0f && bc <= 0f && ca <= 0f);
    }

    private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
}
