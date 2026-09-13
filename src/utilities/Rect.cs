using System;

/// <summary>
/// A simple rectangle class representing a rectangular area defined by its top-left corner (X, Y) and its dimensions (Width, Height). Provides methods for checking containment and intersection with other rectangles.
/// </summary>
public class Rect
{
    /// <summary>
    /// The X coordinate of the top-left corner of the rectangle.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// The Y coordinate of the top-left corner of the rectangle.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Initializes a new instance of the Rect class with the specified position and size.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left corner of the rectangle.</param>
    /// <param name="y">The Y coordinate of the top-left corner of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public Rect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Determines whether the specified point (x, y) is contained within the bounds of the rectangle. Returns true if the point is inside the rectangle, and false otherwise.
    /// </summary>
    /// <param name="x">The X coordinate of the point to check.</param>
    /// <param name="y">The Y coordinate of the point to check.</param>
    /// <returns>true if the point is inside the rectangle; otherwise, false.</returns>
    public bool Contains(float x, float y)
    {
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }

    /// <summary>
    /// Determines whether the specified rectangle intersects with this rectangle. Returns true if the two rectangles overlap, and false otherwise.
    /// </summary>
    /// <param name="other">The rectangle to check for intersection with this rectangle.</param>
    /// <returns>true if the rectangles intersect; otherwise, false.</returns>
    public bool Intersects(Rect other)
    {
        return !(other.X > X + Width || other.X + other.Width < X || other.Y > Y + Height || other.Y + other.Height < Y);
    }

    /// <summary>
    /// Returns a string representation of the rectangle in the format "Rect(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})". This can be useful for debugging and logging purposes.
    /// </summary>
    /// <returns>A string representing the rectangle.</returns>
    public override string ToString()
    {
        return $"Rect(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";
    }
}