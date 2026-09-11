using System.Collections.Generic;
using System.Numerics;

namespace odl3d;

/// <summary>
/// A single glyph's vector outline, flattened into straight-line contours expressed in font pixels with the
/// origin on the baseline and Y pointing up. Contours are stored in the order FreeType reports them; whether
/// a contour is a filled shape or a hole is decided later by containment (see TextGeometry), so this type is
/// independent of the TrueType/CFF winding conventions.
/// </summary>
/// <param name="Contours">The flattened contours. Each is implicitly closed; the last point is not repeated.</param>
/// <param name="Advance">How far the pen moves after this glyph, in pixels, including synthetic-bold widening.</param>
internal readonly record struct GlyphOutline(List<List<Vector2>> Contours, float Advance);
