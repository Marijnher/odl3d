using System;
using System.Collections.Generic;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Walks a FreeType FT_Outline and flattens its conic/cubic Bézier segments into polylines.
/// </summary>
/// <remarks>
/// Implemented by reading the outline's point/tag/contour arrays directly instead of going through
/// FT_Outline_Decompose, which would require four native callbacks and marshaling of platform-dependent
/// FT_Vector structs (see FT.cs for why that layout cannot be trusted to Marshal.PtrToStructure).
/// </remarks>
internal static class OutlineDecomposer
{
    /// <summary>Points closer together than this (in font pixels) are merged; zero-length edges break normal generation.</summary>
    private const float MinSegmentLength = 1e-3f;

    private const int MaxSegmentsPerCurve = 64;

    /// <summary>
    /// Flattens every contour of the given outline.
    /// </summary>
    /// <param name="outline">Pointer to the FT_Outline of a loaded glyph slot.</param>
    /// <param name="flatness">Maximum allowed deviation, in font pixels, between a curve and its polyline.</param>
    /// <returns>A list of contours, each represented as a list of 2D points.</returns>
    public static List<List<Vector2>> Decompose(IntPtr outline, float flatness)
    {
        List<List<Vector2>> contours = new();
        int contourCount = FT.GetOutlineContourCount(outline);
        if (contourCount <= 0) return contours;

        IntPtr points = FT.GetOutlinePoints(outline);
        IntPtr tags = FT.GetOutlineTags(outline);
        IntPtr ends = FT.GetOutlineContours(outline);
        if (points == IntPtr.Zero || tags == IntPtr.Zero || ends == IntPtr.Zero) return contours;

        int start = 0;
        for (int contour = 0; contour < contourCount; contour++)
        {
            int end = FT.GetOutlineContourEnd(ends, contour);
            int count = end - start + 1;
            if (count >= 2)
            {
                List<Vector2> flattened = Flatten(points, tags, start, count, flatness);
                if (flattened.Count >= 3) contours.Add(flattened);
            }
            start = end + 1;
        }

        return contours;
    }

    private static List<Vector2> Flatten(IntPtr points, IntPtr tags, int start, int count, float flatness)
    {
        Vector2 Point(int i) => FT.GetOutlinePoint(points, start + Wrap(i, count));
        bool OnCurve(int i) => (FT.GetOutlineTag(tags, start + Wrap(i, count)) & FT.FT_CURVE_TAG_ON) != 0;
        bool Cubic(int i) => (FT.GetOutlineTag(tags, start + Wrap(i, count)) & FT.FT_CURVE_TAG_CUBIC) != 0;

        int firstOnCurve = -1;
        for (int i = 0; i < count; i++)
        {
            if (OnCurve(i)) { firstOnCurve = i; break; }
        }

        List<Vector2> result = new();
        Vector2 current;
        int index;
        int walkEnd;
        if (firstOnCurve >= 0)
        {
            current = Point(firstOnCurve);
            index = firstOnCurve + 1;
            // Point(walkEnd) wraps back to the starting point, so the walk closes the contour on its own.
            walkEnd = firstOnCurve + count;
        }
        else
        {
            // A contour of nothing but conic control points implicitly begins halfway between its last and first point.
            current = (Point(count - 1) + Point(0)) * 0.5f;
            index = 0;
            walkEnd = count - 1;
        }
        result.Add(current);

        while (index <= walkEnd)
        {
            if (OnCurve(index))
            {
                current = Point(index);
                Append(result, current);
                index++;
            }
            else if (Cubic(index))
            {
                Vector2 control1 = Point(index);
                Vector2 control2 = Point(index + 1);
                Vector2 end = Point(index + 2);
                AppendCubic(result, current, control1, control2, end, flatness);
                current = end;
                index += 3;
            }
            else
            {
                Vector2 control = Point(index);
                int next = index + 1;
                Vector2 end;
                if (OnCurve(next))
                {
                    end = Point(next);
                    index = next + 1;
                }
                else
                {
                    // Two consecutive conic control points share an implied on-curve point at their midpoint.
                    end = (control + Point(next)) * 0.5f;
                    index = next;
                }
                AppendQuadratic(result, current, control, end, flatness);
                current = end;
            }
        }

        while (result.Count >= 2 && NearlyEqual(result[0], result[^1])) result.RemoveAt(result.Count - 1);
        return result;
    }

    private static void AppendQuadratic(List<Vector2> result, Vector2 p0, Vector2 control, Vector2 p1, float flatness)
    {
        // A quadratic drawn with n segments deviates by at most |p0 - 2c + p1| / (8n^2).
        int segments = SegmentCount((p0 - 2f * control + p1).Length() / 8f, flatness);
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float u = 1f - t;
            Append(result, u * u * p0 + 2f * u * t * control + t * t * p1);
        }
    }

    private static void AppendCubic(List<Vector2> result, Vector2 p0, Vector2 control1, Vector2 control2, Vector2 p1, float flatness)
    {
        float deviation = 0.75f * MathF.Max(
            (p0 - 2f * control1 + control2).Length(),
            (control1 - 2f * control2 + p1).Length());
        int segments = SegmentCount(deviation, flatness);
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float u = 1f - t;
            Append(result, u * u * u * p0 + 3f * u * u * t * control1 + 3f * u * t * t * control2 + t * t * t * p1);
        }
    }

    private static int SegmentCount(float deviation, float flatness)
    {
        if (!(deviation > flatness)) return 1;
        return Math.Clamp((int)MathF.Ceiling(MathF.Sqrt(deviation / flatness)), 2, MaxSegmentsPerCurve);
    }

    private static void Append(List<Vector2> result, Vector2 point)
    {
        if (result.Count > 0 && NearlyEqual(result[^1], point)) return;
        result.Add(point);
    }

    private static bool NearlyEqual(Vector2 a, Vector2 b) => Vector2.DistanceSquared(a, b) < MinSegmentLength * MinSegmentLength;

    private static int Wrap(int index, int count) => (index % count + count) % count;
}
