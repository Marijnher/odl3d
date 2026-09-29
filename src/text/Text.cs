using System;
using System.Numerics;

namespace odl3d;

/// <summary>
/// Shared base for styled Unicode text. It owns the content and its typographic settings; how that content
/// becomes geometry is left to the concrete subclasses, of which there are two families:
/// <list type="bullet">
/// <item>RasterizedText composes a glyph bitmap texture and maps it onto a quad (Text2D, TextBillboard).</item>
/// <item>Text3D tessellates and extrudes the glyph outlines into solid geometry.</item>
/// </list>
/// Every property setter rebuilds immediately, so the text always matches its settings.
/// </summary>
public abstract class Text : Object3D
{
    private Font _font;
    /// <summary>
    /// The font used to measure and build this text's glyphs.
    /// </summary>
    public Font Font
    {
        get => _font;
        set { if (_font == value) return; _font = value; Rebuild(); }
    }

    private string _content;
    /// <summary>
    /// The text to display. Rebuilds immediately when changed.
    /// </summary>
    public string Content
    {
        get => _content;
        set { if (_content == value) return; _content = value; Rebuild(); }
    }

    private FontStyle _style;
    /// <summary>
    /// Bold/Italic style to render this text with.
    /// </summary>
    public FontStyle Style
    {
        get => _style;
        set { if (_style == value) return; _style = value; Rebuild(); }
    }

    private TextAlign _align;
    /// <summary>
    /// Horizontal alignment of each line within the text block. Only visible with multiple lines.
    /// </summary>
    public TextAlign Align
    {
        get => _align;
        set { if (_align == value) return; _align = value; Rebuild(); }
    }

    private bool _underline;
    /// <summary>
    /// Whether to draw an underline bar beneath the text.
    /// </summary>
    public bool Underline
    {
        get => _underline;
        set { if (_underline == value) return; _underline = value; Rebuild(); }
    }

    private bool _strikethrough;
    /// <summary>
    /// Whether to draw a strikethrough bar through the text.
    /// </summary>
    public bool Strikethrough
    {
        get => _strikethrough;
        set { if (_strikethrough == value) return; _strikethrough = value; Rebuild(); }
    }

    /// <summary>
    /// Creates a new Text drawable in the given scene. Subclasses must call Rebuild() once their own fields
    /// are initialized; this constructor deliberately does not, since Rebuild is virtual.
    /// </summary>
    /// <param name="scene">The scene to which this text belongs.</param>
    /// <param name="font">The font to measure and build this text's glyphs with.</param>
    /// <param name="content">The initial text to display. May contain line breaks.</param>
    /// <param name="style">The initial Bold/Italic style.</param>
    /// <param name="align">Horizontal alignment of each line within the text block.</param>
    protected Text(Scene<Object3D> scene, Font font, string content, FontStyle style, TextAlign align)
        : base(scene, null, null)
    {
        _font = font;
        _content = content;
        _style = style;
        _align = align;
    }

    /// <summary>
    /// Measures this text's Content in its current Font/Style in font pixels, without building anything,
    /// useful for positioning other elements without waiting on (or even needing) the built geometry.
    /// </summary>
    /// <returns>The width and height of the text in font pixels.</returns>
    public Vector2 MeasureString() => Font.MeasureString(Content, Style);

    /// <summary>
    /// Rebuilds whatever this text draws with (a composed texture, a mesh, or both) from the current Font,
    /// Content, Style and decoration settings.
    /// </summary>
    protected abstract void Rebuild();
}

