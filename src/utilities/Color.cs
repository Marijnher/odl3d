using System.Numerics;

namespace odl3d;

/// <summary>
/// Represents a color with red, green, blue, and alpha (transparency) components. Each component is stored as a byte (0-255), allowing for 256 levels of intensity for each color channel. The Color struct can be used to define colors for rendering objects in 2D and 3D scenes, providing a simple way to specify RGBA values for visual elements.
/// </summary>
public struct Color
{
    /// <summary>
    /// Predefined static property representing the color black (0, 0, 0). This can be used as a convenient reference for black color in rendering operations.
    /// </summary>
    public static Color Black => new Color(0, 0, 0);
    /// <summary>
    /// Predefined static property representing the color white (255, 255, 255). This can be used as a convenient reference for white color in rendering operations.
    /// </summary>
    public static Color White => new Color(255, 255, 255);
    /// <summary>
    /// Predefined static property representing the color transparent (0, 0, 0, 0). This can be used as a convenient reference for fully transparent color in rendering operations.
    /// </summary>
    public static Color Red => new Color(255, 0, 0);
    /// <summary>
    /// Predefined static property representing the color green (0, 255, 0). This can be used as a convenient reference for green color in rendering operations.
    /// </summary>
    public static Color Green => new Color(0, 255, 0);
    /// <summary>
    /// Predefined static property representing the color blue (0, 0, 255). This can be used as a convenient reference for blue color in rendering operations.
    /// </summary>
    public static Color Blue => new Color(0, 0, 255);
    /// <summary>
    /// Predefined static property representing the color transparent (0, 0, 0, 0). This can be used as a convenient reference for fully transparent color in rendering operations.
    /// </summary>
    public static Color Alpha => new Color(0, 0, 0, 0);
    /// <summary>
    /// Predefined static property representing the color yellow (255, 255, 0). This can be used as a convenient reference for yellow color in rendering operations.
    /// </summary>
    public static Color Yellow => new Color(255, 255, 0);
    /// <summary>
    /// Predefined static property representing the color cyan (0, 255, 255). This can be used as a convenient reference for cyan color in rendering operations.
    /// </summary>
    public static Color Aqua => new Color(0, 255, 255);
    /// <summary>
    /// Predefined static property representing the color magenta (255, 0, 255). This can be used as a convenient reference for magenta color in rendering operations.
    /// </summary>
    public static Color Magenta => new Color(255, 0, 255);
    /// <summary>
    /// Predefined static property representing the color gray (128, 128, 128). This can be used as a convenient reference for gray color in rendering operations.
    /// </summary>
    public static Color Gray => new Color(128, 128, 128);

    /// <summary>
    /// The red component of the color, represented as a byte (0-255). A value of 0 indicates no red, while 255 indicates full red intensity.
    /// </summary>
    public byte R;

    /// <summary>
    /// The green component of the color, represented as a byte (0-255). A value of 0 indicates no green, while 255 indicates full green intensity.
    /// </summary>
    public byte G;

    /// <summary>
    /// The blue component of the color, represented as a byte (0-255). A value of 0 indicates no blue, while 255 indicates full blue intensity.
    /// </summary>
    public byte B;

    /// <summary>
    /// The alpha (transparency) component of the color, represented as a byte (0-255). A value of 0 indicates full transparency, while 255 indicates full opacity. This component allows for blending and transparency effects when rendering objects in 2D and 3D scenes.
    /// </summary>
    public byte A;

    /// <summary>
    /// Initializes a new instance of the Color struct with the specified red, green, blue, and optional alpha components. The alpha component defaults to 255 (fully opaque) if not provided. This constructor allows for easy creation of Color instances with specific RGBA values for use in rendering and visual effects.
    /// </summary>
    /// <param name="r">The red component of the color (0-255).</param>
    /// <param name="g">The green component of the color (0-255).</param>
    /// <param name="b">The blue component of the color (0-255).</param>
    /// <param name="a">The alpha (transparency) component of the color (0-255).</param>
    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Vector4 ToVector4() => new Vector4(R / 255f, G / 255f, B / 255f, A / 255f);
}
