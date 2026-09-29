using System;

namespace odl3d;

/// <summary>
/// Style flags for rendering text. Bold and Italic can be combined. When a Font does not have a dedicated
/// face file for the requested combination, it falls back to synthesizing the style from the regular face
/// (embolden the outline for Bold, shear it for Italic).
/// </summary>
[Flags]
public enum FontStyle
{
    /// <summary>
    /// The regular style with no additional emphasis.
    /// </summary>
    Regular = 0,

    /// <summary>
    /// The bold style, typically used for emphasis.
    /// </summary>
    Bold = 1,

    /// <summary>
    /// The italic style, typically used for emphasis.
    /// </summary>
    Italic = 2
}
