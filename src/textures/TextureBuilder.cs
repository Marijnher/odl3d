using System;

namespace odl3d;

/// <summary>
/// Provides methods to create and manipulate textures with various patterns and colors.
/// </summary>
public class TextureBuilder
{
    /// <summary>
    /// Creates a new Texture with a checkerboard pattern of the two specified colors. The pattern starts at the specified (x, y) coordinates and covers the specified width and height.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left corner of the checkerboard pattern.</param>
    /// <param name="y">The Y coordinate of the top-left corner of the checkerboard pattern.</param>
    /// <param name="width">The width of the checkerboard pattern in pixels.</param>
    /// <param name="height">The height of the checkerboard pattern in pixels.</param>
    /// <param name="colorA">The first color of the checkerboard pattern.</param>
    /// <param name="colorB">The second color of the checkerboard pattern.</param>
    /// <param name="cellsize">The size of each square cell in the checkerboard pattern.</param>
    /// <returns>A new Texture initialized with the specified checkerboard pattern.</returns>
    public static Texture CreateCheckerboard(uint width, uint height, Color colorA, Color colorB, int cellsize = 8)
    {
        TextureBuilder builder = new TextureBuilder(width, height);
        builder.DrawCheckerboard(0, 0, width, height, colorA, colorB, cellsize);
        return builder.Build();
    }

    /// <summary>
    /// Creates a new Texture with a gradient defined by the four corner colors. The gradient is interpolated across the entire texture.
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    /// <param name="c1">Color of the top-left corner.</param>
    /// <param name="c2">Color of the top-right corner.</param>
    /// <param name="c3">Color of the bottom-left corner.</param>
    /// <param name="c4">Color of the bottom-right corner.</param>
    /// <returns>A new Texture initialized with the specified gradient.</returns>
    public static Texture CreateGradient(uint width, uint height, Color c1, Color c2, Color c3, Color c4)
    {
        TextureBuilder builder = new TextureBuilder(width, height);
        builder.DrawGradient(0, 0, width, height, c1, c2, c3, c4);
        return builder.Build();
    }
    
    /// <summary>
    /// Creates a new Texture filled with the specified solid color.
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    /// <param name="color">The solid color to fill the texture with.</param>
    /// <returns>A new Texture initialized with the specified solid color.</returns>
    public static Texture CreateSolid(uint width, uint height, Color color)
    {
        TextureBuilder builder = new TextureBuilder(width, height);
        builder.DrawSolid(0, 0, width, height, color);
        return builder.Build();
    }

    /// <summary>
    /// Creates a new Texture with a filled circle of the specified radius and color. The circle is centered within the texture.
    /// </summary>
    /// <param name="radius">Radius of the circle in pixels.</param>
    /// <param name="color">Color of the circle.</param>
    /// <returns>A new Texture initialized with the specified filled circle.</returns>
    public static Texture CreateCircle(int radius, Color color)
    {
        uint diameter = (uint) radius * 2 + 1;
        TextureBuilder builder = new TextureBuilder(diameter, diameter);
        builder.DrawCircle(radius, radius, radius, color);
        return builder.Build();
    }

    /// <summary>
    /// The existing Texture instance that this builder will modify. If null, the builder will create a new Texture when Build() is called.
    /// </summary>
    protected Texture? Texture;

    /// <summary>
    /// The pixel buffer that the builder will modify. This buffer is either taken from the existing Texture or newly allocated if creating a new Texture.
    /// </summary>
    protected byte[] Pixels;

    /// <summary>
    /// The width of the texture in pixels.
    /// </summary>
    public uint TextureWidth { get; }

    /// <summary>
    /// The height of the texture in pixels.
    /// </summary>
    public uint TextureHeight { get; }

    /// <summary>
    /// Initializes a new instance of the TextureBuilder class with an existing Texture. The builder will allow modifications to the texture's pixel data.
    /// </summary>
    /// <param name="texture">The existing Texture instance to be modified by the builder.</param>
    /// <exception cref="TextureException">Thrown if the provided texture has already been disposed.</exception>
    public TextureBuilder(Texture source, bool overwriteSource)
    {
        if (source.Disposed)
            throw new TextureException("Cannot build a TextureBuilder from a disposed texture.");
        if (overwriteSource)
        {
            Texture = source;
            Pixels = source.Pixels;
        }   
        else
        {
            Pixels = (byte[]) source.Pixels.Clone();
        }
        TextureWidth = source.Width;
        TextureHeight = source.Height;
    }

    /// <summary>
    /// Initializes a new instance of the TextureBuilder class with the specified width and height. A new pixel buffer is allocated for the texture.
    /// </summary>
    /// <param name="width">The width of the texture in pixels.</param>
    /// <param name="height">The height of the texture in pixels.</param>
    /// <exception cref="TextureException">Thrown if the width or height is less than 1.</exception>
    public TextureBuilder(uint width, uint height)
    {
        if (width < 1 || height < 1)
            throw new TextureException("Width and height must be greater than 0.");
        TextureWidth = width;
        TextureHeight = height;
        Pixels = new byte[width * height * 4];
    }

    /// <summary>
    /// Builds and returns the Texture instance based on the current state of the builder. If the builder was initialized with an existing Texture, that Texture is updated; otherwise, a new Texture is created.
    /// </summary>
    /// <returns>The built Texture instance.</returns>
    public Texture Build()
    {
        if (Texture == null)
        {
            return new Texture(TextureWidth, TextureHeight, Pixels);
        }
        Texture.Invalidate();
        return Texture;
    }

    /// <summary>
    /// Guards that the specified region is within the bounds of the texture and that the coordinates and dimensions are valid.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left corner of the region.</param>
    /// <param name="y">The Y coordinate of the top-left corner of the region.</param>
    /// <param name="width">The width of the region in pixels.</param>
    /// <param name="height">The height of the region in pixels.</param>
    /// <exception cref="TextureException">Thrown if the region is out of bounds or if the coordinates or dimensions are invalid.</exception>
    private void GuardBounds(int x, int y, uint width, uint height)
    {
        if (x < 0 || y < 0)
            throw new TextureException("X and Y coordinates must be non-negative.");
        if (width < 1 || height < 1)
            throw new TextureException("Width and height must be greater than 0.");
        if (x + width > TextureWidth || y + height > TextureHeight)
            throw new TextureException("The specified region exceeds the texture bounds.");
    }

    /// <summary>
    /// Draws a single pixel of the specified color at the given coordinates within the texture.
    /// </summary>
    /// <param name="x">The X coordinate of the pixel.</param>
    /// <param name="y">The Y coordinate of the pixel.</param>
    /// <param name="color">The color of the pixel (RGBA).</param>
    /// <returns>The current TextureBuilder instance for chaining.</returns>
    public TextureBuilder DrawPixel(int x, int y, Color color)
    {
        GuardBounds(x, y, 1, 1);
        int o = (int) (y * TextureWidth + x) * 4;
        Pixels[o    ] = color.R;
        Pixels[o + 1] = color.G;
        Pixels[o + 2] = color.B;
        Pixels[o + 3] = color.A;
        return this;
    }

    /// <summary>
    /// Draws a solid rectangle of the specified color at the given position and size within the texture.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left corner of the rectangle.</param>
    /// <param name="y">The Y coordinate of the top-left corner of the rectangle.</param>
    /// <param name="width">The width of the rectangle in pixels.</param>
    /// <param name="height">The height of the rectangle in pixels.</param>
    /// <param name="color">The color of the rectangle (RGBA).</param>
    public TextureBuilder DrawSolid(int x, int y, uint width, uint height, Color color)
    {
        GuardBounds(x, y, width, height);

        for (int dy = y; dy < y + height; dy++)
        {
            for (int dx = x; dx < x + width; dx++)
            {
                int o = (int) (dy * TextureWidth + dx) * 4;
                Pixels[o    ] = color.R;
                Pixels[o + 1] = color.G;
                Pixels[o + 2] = color.B;
                Pixels[o + 3] = color.A;
            }
        }
        return this;
    }

    /// <summary>
    /// Creates a new Texture with the given width and height, filling the pixel buffer with a checkerboard pattern of the two specified colors. The pixel data is initialized to a checkerboard pattern of the specified colors, with each square being cellSize pixels wide and tall.
    /// </summary>
    /// <param name="width">Width of the texture in pixels.</param>
    /// <param name="height">Height of the texture in pixels.</param>
    /// <param name="colorA">First color of the checkerboard pattern (RGB).</param>
    /// <param name="colorB">Second color of the checkerboard pattern (RGB).</param>
    /// <param name="cellSize">Size of each square in the checkerboard pattern in pixels.</param>
    /// <returns>A new Texture initialized with the checkerboard pattern.</returns>
    public TextureBuilder DrawCheckerboard(int x, int y, uint width, uint height, Color colorA, Color colorB, int cellSize = 8)
    {
        GuardBounds(x, y, width, height);
            
        for (int dy = y; dy < y + height; dy++)
        {
            for (int dx = x; dx < x + width; dx++)
            {
                bool isA = (((dx - x) / cellSize) + ((dy - y) / cellSize)) % 2 == 0;
                Color c = isA ? colorA : colorB;
                int o = (int) (dy * TextureWidth + dx) * 4;
                Pixels[o    ] = c.R;
                Pixels[o + 1] = c.G;
                Pixels[o + 2] = c.B;
                Pixels[o + 3] = c.A;
            }
        }
        return this;
    }

    /// <summary>
    /// Draws a filled circle on the texture at the specified center coordinates with the given radius and color.
    /// </summary>
    /// <param name="centerX">X-coordinate of the circle's center.</param>
    /// <param name="centerY">Y-coordinate of the circle's center.</param>
    /// <param name="radius">Radius of the circle in pixels.</param>
    /// <param name="color">Color of the circle.</param>
    /// <returns>The TextureBuilder instance for chaining.</returns>
    public TextureBuilder DrawCircle(int centerX, int centerY, int radius, Color color)
    {
        GuardBounds(centerX - radius, centerY - radius, (uint) (2 * radius + 1), (uint) (2 * radius + 1));

        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;
                if (dx * dx + dy * dy <= radius * radius)
                {
                    int o = (int) (y * TextureWidth + x) * 4;
                    Pixels[o    ] = color.R;
                    Pixels[o + 1] = color.G;
                    Pixels[o + 2] = color.B;
                    Pixels[o + 3] = color.A;
                }
            }
        }
        return this;
    }

    /// <summary>
    /// Creates a new Texture with a gradient defined by the four corner colors. The gradient is interpolated across the specified rectangular region.
    /// </summary>
    /// <param name="x">X-coordinate of the top-left corner of the gradient region.</param>
    /// <param name="y">Y-coordinate of the top-left corner of the gradient region.</param>
    /// <param name="width">Width of the gradient region in pixels.</param>
    /// <param name="height">Height of the gradient region in pixels.</param>
    /// <param name="c1">Color of the top-left corner.</param>
    /// <param name="c2">Color of the top-right corner.</param>
    /// <param name="c3">Color of the bottom-left corner.</param>
    /// <param name="c4">Color of the bottom-right corner.</param>
    /// <returns>A new Texture initialized with the specified gradient.</returns>
    public TextureBuilder DrawGradient(int x, int y, uint width, uint height, Color c1, Color c2, Color c3, Color c4)
    {
        GuardBounds(x, y, width, height);

        for (int dy = y; dy < y + height; dy++)
        {
            for (int dx = x; dx < x + width; dx++)
            {
                double xl = dx - x;
                double xr = x + width - 1 - dx;
                double yt = dy - y;
                double yb = y + height - 1 - dy;
                double fxr = xl / (xl + xr);
                double fxl = 1 - fxr;
                double fyb = yt / (yt + yb);
                double fyt = 1 - fyb;
                double f1 = fxl * fyt;
                double f2 = fxr * fyt;
                double f3 = fxl * fyb;
                double f4 = fxr * fyb;
                int o = (int) (dy * TextureWidth + dx) * 4;
                Pixels[o    ] = (byte) Math.Round(f1 * c1.R + f2 * c2.R + f3 * c3.R + f4 * c4.R);
                Pixels[o + 1] = (byte) Math.Round(f1 * c1.G + f2 * c2.G + f3 * c3.G + f4 * c4.G);
                Pixels[o + 2] = (byte) Math.Round(f1 * c1.B + f2 * c2.B + f3 * c3.B + f4 * c4.B);
                Pixels[o + 3] = (byte) Math.Round(f1 * c1.A + f2 * c2.A + f3 * c3.A + f4 * c4.A);
            }
        }
        return this;
    }
}