namespace odl3d;

public partial class Mesh
{
    /// <summary>
    /// Returns a shared Mesh instance representing a unit quad centered at the origin, with vertices at (-0.5, -0.5), (0.5, -0.5), (0.5, 0.5), and (-0.5, 0.5), and texture coordinates (0, 1), (1, 1), (1, 0), and (0, 0). This quad can be used for rendering sprites or other textured objects in 2D or 3D space. The mesh is created once and reused for all calls to this property.
    /// </summary>
    public static Mesh Quad => _quad ??= CreateQuad();
    private static Mesh? _quad;

    /// <summary>
    /// Returns a shared Mesh instance identical to Quad but with its V texture coordinate flipped, so a
    /// top-down CPU pixel buffer (row 0 = top, as produced by Texture) samples right-side up once rendered
    /// through Scene2D's ortho projection, without needing to flip the pixel data itself.
    /// </summary>
    public static Mesh QuadFlippedV => _quadFlippedV ??= CreateQuad(flipV: true);
    private static Mesh? _quadFlippedV;

    /// <summary>
    /// Creates a new Mesh instance representing a unit quad centered at the origin, with texture coordinates scaled by the specified u and v multipliers. This allows for repeating or stretching the texture across the quad.
    /// </summary>
    /// <param name="uMult">The multiplier for the U (horizontal) texture coordinate.</param>
    /// <param name="vMult">The multiplier for the V (vertical) texture coordinate.</param>
    /// <param name="flipV">If true, swaps the top/bottom V coordinates so the texture samples upright rather than upside-down.</param>
    /// <returns>A new Mesh instance representing the unit quad with scaled texture coordinates.</returns>
    public static Mesh CreateQuad(float uMult = 1.0f, float vMult = 1.0f, bool flipV = false)
    {
        float vTop = flipV ? vMult : 0f;
        float vBottom = flipV ? 0f : vMult;
        float[] vertices =
        {
            // x,     y,    z,      u,    v
            -0.5f, -0.5f, 0f,       0f, vBottom,
             0.5f, -0.5f, 0f,       uMult, vBottom,
             0.5f,  0.5f, 0f,       uMult, vTop,
            -0.5f,  0.5f, 0f,       0f, vTop,
        };
        uint[] indices = { 0, 1, 2, 2, 3, 0 };
        return new Mesh(vertices, indices);
    }
}
