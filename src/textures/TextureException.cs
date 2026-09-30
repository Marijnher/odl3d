using System;

namespace odl3d;

/// <summary>
/// Represents errors that occur specifically with texture operations in the odl3d framework.
/// </summary>
public class TextureException : Exception
{
    /// <summary>
    /// Initializes a new instance of the TextureException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public TextureException(string message) : base(message) { }
}