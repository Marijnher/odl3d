namespace odl3d;

/// <summary>
/// Horizontal alignment of individual lines within a multi-line Text block. Only affects text with more than
/// one line, since a single line is always exactly as wide as the block itself.
/// </summary>
public enum TextAlign
{
    /// <summary>
    /// Aligns the text to the left edge of the block.
    /// </summary>
    Left,

    /// <summary>
    /// Aligns the text to the horizontal center of the block.
    /// </summary>
    Center,

    /// <summary>
    /// Aligns the text to the right edge of the block.
    /// </summary>
    Right
}
