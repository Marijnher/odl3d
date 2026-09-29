using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace odl3d;

/// <summary>
/// Turns a string into a solid, extruded triangle mesh: every glyph's outline is flattened, its contours are
/// classified into filled shapes and holes by containment, the shapes are tessellated into front and back
/// caps, and every contour is swept along Z into a side wall. The result is centered on the origin, with X
/// and Y spanning the text block and Z spanning the extrusion depth.
/// </summary>
internal static class TextGeometry
{
    /// <summary>
    /// Number of floats per vertex: position (3), texture coordinate (2), normal (3).
    /// </summary>
    public const int FloatsPerVertex = 8;

    private readonly record struct Shape(List<Vector2> Outer, List<List<Vector2>> Holes);

    /// <summary>
    /// The geometry of one built text block, in world units, centered on the origin.
    /// </summary>
    internal sealed class Result
    {
        public required float[] Vertices { get; init; }
        public required uint[] Indices { get; init; }
        public float Width { get; init; }
        public float Height { get; init; }
    }

    /// <summary>
    /// Builds the geometry for a text block with the specified font, content, style, alignment, and decoration options, returning a Result containing the vertices and indices of the extruded mesh.
    /// </summary>
    /// <param name="font">The font to use for measuring and building glyphs.</param>
    /// <param name="content">The text content to build geometry for.</param>
    /// <param name="style">The font style (e.g., Bold, Italic).</param>
    /// <param name="align">The horizontal alignment of the text block.</param>
    /// <param name="underline">Whether to include an underline bar.</param>
    /// <param name="strikethrough">Whether to include a strikethrough bar.</param>
    /// <param name="depth">The extrusion depth of the text geometry.</param>
    /// <param name="pixelsPerWorldUnit">The scale factor from font pixels to world units.</param>
    /// <param name="smoothingAngleDegrees">The angle in degrees for smoothing normals on the extruded geometry.</param>
    /// <returns>A Result object containing the vertices and indices of the extruded mesh.</returns>
    public static Result Build(Font font, string content, FontStyle style, TextAlign align, bool underline,
        bool strikethrough, float depth, float pixelsPerWorldUnit, float smoothingAngleDegrees)
    {
        string[] lines = Font.SplitLines(content);
        float[] lineWidths = new float[lines.Length];
        float widest = 0f;
        for (int i = 0; i < lines.Length; i++)
        {
            lineWidths[i] = font.MeasureLine(lines[i], style);
            widest = MathF.Max(widest, lineWidths[i]);
        }

        float blockWidth = MathF.Max(widest, 1f);
        float blockHeight = MathF.Max(font.LineHeight * lines.Length, 1f);
        float barThickness = MathF.Max(1f, font.UnderlineThickness);

        Emitter emitter = new Emitter(depth, blockWidth, blockHeight, pixelsPerWorldUnit, smoothingAngleDegrees);

        for (int i = 0; i < lines.Length; i++)
        {
            float baseline = font.Ascender + font.LineHeight * i;
            float penX = align switch
            {
                TextAlign.Center => (blockWidth - lineWidths[i]) / 2f,
                TextAlign.Right => blockWidth - lineWidths[i],
                _ => 0f
            };
            float lineStartX = penX;

            int previousCodepoint = -1;
            foreach (Rune rune in lines[i].EnumerateRunes())
            {
                if (previousCodepoint >= 0) penX += font.GetKerning(previousCodepoint, rune.Value, style);

                GlyphOutline glyph = font.GetGlyphOutline(rune.Value, style);
                if (glyph.Contours.Count > 0)
                {
                    // The outline is relative to the pen and the baseline, with Y already pointing up.
                    Vector2 offset = new Vector2(penX - blockWidth / 2f, blockHeight / 2f - baseline);
                    emitter.AddShapes(Transform(glyph.Contours, offset, 1f / pixelsPerWorldUnit));
                }

                penX += glyph.Advance;
                previousCodepoint = rune.Value;
            }

            if (underline)
                emitter.AddShapes(Bar(lineStartX, penX, -font.UnderlinePosition, barThickness, blockWidth, blockHeight, baseline, pixelsPerWorldUnit));
            if (strikethrough)
                emitter.AddShapes(Bar(lineStartX, penX, font.Ascender * 0.35f, barThickness, blockWidth, blockHeight, baseline, pixelsPerWorldUnit));
        }

        return new Result
        {
            Vertices = emitter.BuildVertices(),
            Indices = emitter.BuildIndices(),
            Width = blockWidth / pixelsPerWorldUnit,
            Height = blockHeight / pixelsPerWorldUnit
        };
    }

    private static List<List<Vector2>> Transform(List<List<Vector2>> contours, Vector2 offset, float scale)
    {
        List<List<Vector2>> transformed = new(contours.Count);
        foreach (List<Vector2> contour in contours)
        {
            List<Vector2> points = new(contour.Count);
            foreach (Vector2 point in contour) points.Add((point + offset) * scale);
            transformed.Add(points);
        }
        return transformed;
    }

    /// <summary>
    /// Builds the rectangular contour of an underline or strikethrough bar, in world units.
    /// </summary>
    private static List<List<Vector2>> Bar(float x0, float x1, float top, float thickness, float blockWidth, float blockHeight, float baseline, float pixelsPerWorldUnit)
    {
        if (x1 - x0 < 1f) return new List<List<Vector2>>();

        Vector2 offset = new Vector2(-blockWidth / 2f, blockHeight / 2f - baseline);
        float scale = 1f / pixelsPerWorldUnit;
        float bottom = top - thickness;
        return new List<List<Vector2>>
        {
            new List<Vector2>
            {
                (new Vector2(x0, bottom) + offset) * scale,
                (new Vector2(x1, bottom) + offset) * scale,
                (new Vector2(x1, top) + offset) * scale,
                (new Vector2(x0, top) + offset) * scale
            }
        };
    }

    /// <summary>
    /// Accumulates the interleaved vertex/index buffers for one text block.
    /// </summary>
    private sealed class Emitter
    {
        private const float MinimumContourArea = 1e-8f;

        private readonly List<float> _vertices = new();
        private readonly List<uint> _indices = new();
        private readonly float _halfDepth;
        private readonly float _blockWidth;
        private readonly float _blockHeight;
        private readonly float _pixelsPerWorldUnit;
        private readonly float _cosSmoothingAngle;
        private readonly bool _extruded;

        public Emitter(float depth, float blockWidth, float blockHeight, float pixelsPerWorldUnit, float smoothingAngleDegrees)
        {
            _halfDepth = depth / 2f;
            _blockWidth = blockWidth;
            _blockHeight = blockHeight;
            _pixelsPerWorldUnit = pixelsPerWorldUnit;
            _cosSmoothingAngle = MathF.Cos(Math.Clamp(smoothingAngleDegrees, 0f, 180f) * MathF.PI / 180f);
            _extruded = depth > 0f;
        }

        public float[] BuildVertices() => _vertices.ToArray();

        public uint[] BuildIndices() => _indices.ToArray();

        public void AddShapes(List<List<Vector2>> contours)
        {
            foreach (Shape shape in Classify(contours))
            {
                (List<Vector2> polygon, List<int> triangles) = PolygonTessellator.Triangulate(shape.Outer, shape.Holes);
                AddCap(polygon, triangles, true);
                if (_extruded)
                {
                    AddCap(polygon, triangles, false);
                    AddWall(shape.Outer);
                    foreach (List<Vector2> hole in shape.Holes) AddWall(hole);
                }
            }
        }

        /// <summary>
        /// Splits a glyph's contours into filled shapes and the holes they own. A contour nested inside an odd
        /// number of other contours is a hole; this is independent of the font's winding convention, so both
        /// TrueType (clockwise outer) and CFF (counter-clockwise outer) glyphs come out right.
        /// </summary>
        /// <param name="contours">The list of contours representing the glyph.</param>
        /// <returns>A list of shapes, each with an outer contour and its holes.</returns>
        private static List<Shape> Classify(List<List<Vector2>> contours)
        {
            int count = contours.Count;
            int[] depths = new int[count];
            bool[] usable = new bool[count];
            for (int i = 0; i < count; i++) usable[i] = MathF.Abs(PolygonTessellator.SignedArea(contours[i])) > MinimumContourArea;

            for (int i = 0; i < count; i++)
            {
                if (!usable[i]) continue;
                for (int j = 0; j < count; j++)
                {
                    if (i == j || !usable[j]) continue;
                    if (PolygonTessellator.ContainsPoint(contours[j], contours[i][0])) depths[i]++;
                }
            }

            List<Shape> shapes = new();
            int[] shapeOf = new int[count];
            for (int i = 0; i < count; i++)
            {
                shapeOf[i] = -1;
                if (!usable[i] || depths[i] % 2 != 0) continue;
                Orient(contours[i], true);
                shapeOf[i] = shapes.Count;
                shapes.Add(new Shape(contours[i], new List<List<Vector2>>()));
            }

            for (int i = 0; i < count; i++)
            {
                if (!usable[i] || depths[i] % 2 == 0) continue;

                int parent = -1;
                for (int j = 0; j < count; j++)
                {
                    if (shapeOf[j] < 0 || (parent >= 0 && depths[j] <= depths[parent])) continue;
                    if (PolygonTessellator.ContainsPoint(contours[j], contours[i][0])) parent = j;
                }
                if (parent < 0) continue;

                Orient(contours[i], false);
                shapes[shapeOf[parent]].Holes.Add(contours[i]);
            }

            return shapes;
        }

        private static void Orient(List<Vector2> contour, bool counterClockwise)
        {
            if (PolygonTessellator.SignedArea(contour) > 0f != counterClockwise) contour.Reverse();
        }

        private void AddCap(List<Vector2> polygon, List<int> triangles, bool front)
        {
            float z = front ? _halfDepth : -_halfDepth;
            Vector3 normal = new Vector3(0f, 0f, front ? 1f : -1f);
            uint start = (uint)(_vertices.Count / FloatsPerVertex);

            foreach (Vector2 point in polygon) AddVertex(new Vector3(point.X, point.Y, z), point, normal);

            for (int i = 0; i + 2 < triangles.Count; i += 3)
            {
                _indices.Add(start + (uint)triangles[i]);
                _indices.Add(start + (uint)triangles[front ? i + 1 : i + 2]);
                _indices.Add(start + (uint)triangles[front ? i + 2 : i + 1]);
            }
        }

        /// <summary>
        /// Sweeps a contour along Z into a closed band of quads. Vertex normals are averaged across adjacent
        /// edges whose angle falls within the smoothing angle, so flattened curves read as smooth while real
        /// corners stay sharp.
        /// </summary>
        /// <param name="contour">The contour to sweep along the Z axis to create the wall.</param>
        private void AddWall(List<Vector2> contour)
        {
            int count = contour.Count;
            if (count < 3) return;

            Vector2[] edgeNormals = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                Vector2 edge = contour[(i + 1) % count] - contour[i];
                float length = edge.Length();
                // Outward-facing in the contour's own winding: clockwise-rotated edge direction.
                edgeNormals[i] = length > 0f ? new Vector2(edge.Y, -edge.X) / length : Vector2.Zero;
            }

            Vector2[] vertexNormals = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                Vector2 incoming = edgeNormals[(i + count - 1) % count];
                Vector2 outgoing = edgeNormals[i];
                Vector2 sum = incoming + outgoing;
                vertexNormals[i] = Vector2.Dot(incoming, outgoing) >= _cosSmoothingAngle && sum.LengthSquared() > 0f
                    ? Vector2.Normalize(sum)
                    : outgoing;
            }

            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                if (edgeNormals[i] == Vector2.Zero) continue;

                Vector2 a = contour[i];
                Vector2 b = contour[next];
                Vector3 normalA = new Vector3(vertexNormals[i], 0f);
                // The end of this edge uses the next vertex's blended normal only if that blend includes this edge.
                Vector3 normalB = new Vector3(Vector2.Dot(edgeNormals[i], edgeNormals[next]) >= _cosSmoothingAngle ? vertexNormals[next] : edgeNormals[i], 0f);

                uint start = (uint)(_vertices.Count / FloatsPerVertex);
                AddVertex(new Vector3(a.X, a.Y, _halfDepth), a, normalA);
                AddVertex(new Vector3(a.X, a.Y, -_halfDepth), a, normalA);
                AddVertex(new Vector3(b.X, b.Y, -_halfDepth), b, normalB);
                AddVertex(new Vector3(b.X, b.Y, _halfDepth), b, normalB);
                _indices.Add(start);
                _indices.Add(start + 1);
                _indices.Add(start + 2);
                _indices.Add(start + 2);
                _indices.Add(start + 3);
                _indices.Add(start);
            }
        }

        private void AddVertex(Vector3 position, Vector2 planarPoint, Vector3 normal)
        {
            _vertices.Add(position.X);
            _vertices.Add(position.Y);
            _vertices.Add(position.Z);
            // Planar mapping over the whole text block, so a texture wraps continuously across caps and walls.
            _vertices.Add((planarPoint.X * _pixelsPerWorldUnit + _blockWidth / 2f) / _blockWidth);
            _vertices.Add((_blockHeight / 2f - planarPoint.Y * _pixelsPerWorldUnit) / _blockHeight);
            _vertices.Add(normal.X);
            _vertices.Add(normal.Y);
            _vertices.Add(normal.Z);
        }
    }
}
