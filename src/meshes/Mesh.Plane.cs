using System.Collections.Generic;

namespace odl3d;

public partial class Mesh
{
    private readonly record struct Vertex(float X, float Y, float Z, float U, float V);

    private readonly record struct PlaneSideUvs(float LeftU, float RightU, float TopV, float BottomV);

    /// <summary>
    /// Creates a new Mesh instance representing a plane with the given width, height, and length, centered at the origin. The plane's texture coordinates are scaled by the specified u and v multipliers, allowing for repeating or stretching the texture across the plane.
    /// </summary>
    /// <param name="width">The width of the plane.</param>
    /// <param name="height">The height of the plane.</param>
    /// <param name="length">The length of the plane along the z-axis.</param>
    /// <param name="uMult">The multiplier for the U (horizontal) texture coordinate.</param>
    /// <param name="vMult">The multiplier for the V (vertical) texture coordinate.</param>
    /// <param name="flipV">If true, swaps the top/bottom V coordinates so the texture samples upright rather than upside-down.</param>
    /// <param name="sampleEdgesOnSides">If true, side faces sample only the nearest edge row or column of the texture.</param>
    /// <returns>A new Mesh instance representing the plane with scaled texture coordinates.</returns>
    public static Mesh CreatePlane(float width, float height, float length, float uMult = 1.0f, float vMult = 1.0f, bool flipV = false, bool sampleEdgesOnSides = false)
    {
        float vTop = flipV ? vMult : 0f;
        float vBottom = flipV ? 0f : vMult;
        PlaneSideUvs? sideUvs = sampleEdgesOnSides ? new PlaneSideUvs(0f, uMult, vTop, vBottom) : null;
        return CreatePlaneMesh(width, height, length, 0f, uMult, vTop, vBottom, sideUvs, false);
    }

    public static Mesh CreateTextPlane(float width, float height, float length, float leftU, float rightU, float topV, float bottomV) =>
        CreatePlaneMesh(width, height, length, 0f, 1f, 0f, 1f, new PlaneSideUvs(leftU, rightU, topV, bottomV), true);

    private static Mesh CreatePlaneMesh(float width, float height, float length, float faceLeftU, float faceRightU, float faceTopV, float faceBottomV, PlaneSideUvs? sideUvs, bool backMatchesFront)
    {
        List<float> vertices = new();
        List<uint> indices = new();

        AddFrontFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV);
        AddBackFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV, backMatchesFront);
        AddBottomFace(vertices, indices, width, length, faceLeftU, faceRightU, faceTopV, faceBottomV, sideUvs);
        AddTopFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV, sideUvs);
        AddRightFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV, sideUvs);
        AddLeftFace(vertices, indices, width, height, length, faceLeftU, faceRightU, faceTopV, faceBottomV, sideUvs);

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }

    private static void AddFrontFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV) =>
        AddFace(vertices, indices,
            new Vertex(-width / 2f, 0f, length / 2f, leftU, bottomV),
            new Vertex(width / 2f, 0f, length / 2f, rightU, bottomV),
            new Vertex(width / 2f, height, length / 2f, rightU, topV),
            new Vertex(-width / 2f, height, length / 2f, leftU, topV));

    private static void AddBackFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV, bool matchesFront) =>
        AddFace(vertices, indices,
            new Vertex(width / 2f, 0f, -length / 2f, matchesFront ? rightU : leftU, bottomV),
            new Vertex(-width / 2f, 0f, -length / 2f, matchesFront ? leftU : rightU, bottomV),
            new Vertex(-width / 2f, height, -length / 2f, matchesFront ? leftU : rightU, topV),
            new Vertex(width / 2f, height, -length / 2f, matchesFront ? rightU : leftU, topV));

    private static void AddBottomFace(List<float> vertices, List<uint> indices, float width, float length, float leftU, float rightU, float topV, float bottomV, PlaneSideUvs? sideUvs) =>
        AddFace(vertices, indices,
            new Vertex(-width / 2f, 0f, -length / 2f, leftU, bottomV),
            new Vertex(width / 2f, 0f, -length / 2f, rightU, bottomV),
            new Vertex(width / 2f, 0f, length / 2f, rightU, sideUvs?.BottomV ?? topV),
            new Vertex(-width / 2f, 0f, length / 2f, leftU, sideUvs?.BottomV ?? topV));

    private static void AddTopFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV, PlaneSideUvs? sideUvs) =>
        AddFace(vertices, indices,
            new Vertex(-width / 2f, height, length / 2f, leftU, topV),
            new Vertex(width / 2f, height, length / 2f, rightU, topV),
            new Vertex(width / 2f, height, -length / 2f, rightU, sideUvs?.TopV ?? bottomV),
            new Vertex(-width / 2f, height, -length / 2f, leftU, sideUvs?.TopV ?? bottomV));

    private static void AddRightFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV, PlaneSideUvs? sideUvs) =>
        AddFace(vertices, indices,
            new Vertex(width / 2f, 0f, length / 2f, sideUvs?.RightU ?? leftU, bottomV),
            new Vertex(width / 2f, 0f, -length / 2f, rightU, bottomV),
            new Vertex(width / 2f, height, -length / 2f, rightU, topV),
            new Vertex(width / 2f, height, length / 2f, sideUvs?.RightU ?? leftU, topV));

    private static void AddLeftFace(List<float> vertices, List<uint> indices, float width, float height, float length, float leftU, float rightU, float topV, float bottomV, PlaneSideUvs? sideUvs) =>
        AddFace(vertices, indices,
            new Vertex(-width / 2f, 0f, -length / 2f, sideUvs?.LeftU ?? rightU, bottomV),
            new Vertex(-width / 2f, 0f, length / 2f, leftU, bottomV),
            new Vertex(-width / 2f, height, length / 2f, leftU, topV),
            new Vertex(-width / 2f, height, -length / 2f, sideUvs?.LeftU ?? rightU, topV));

    private static void AddFace(List<float> vertices, List<uint> indices, Vertex a, Vertex b, Vertex c, Vertex d)
    {
        uint start = (uint)(vertices.Count / 5);
        AddVertex(vertices, a);
        AddVertex(vertices, b);
        AddVertex(vertices, c);
        AddVertex(vertices, d);
        indices.Add(start);
        indices.Add(start + 1);
        indices.Add(start + 2);
        indices.Add(start + 2);
        indices.Add(start + 3);
        indices.Add(start);
    }

    private static void AddVertex(List<float> vertices, Vertex vertex)
    {
        vertices.Add(vertex.X);
        vertices.Add(vertex.Y);
        vertices.Add(vertex.Z);
        vertices.Add(vertex.U);
        vertices.Add(vertex.V);
    }
}
