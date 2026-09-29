using System;

/// <summary>
/// Represents errors that occur during rendering operations in the 3D rendering engine. This exception is thrown when an error related to rendering, such as an unsupported renderer identifier or a failure in creating a renderer instance, occurs. It provides a mechanism for handling and reporting rendering-related issues in a structured manner, allowing developers to catch and respond to rendering errors appropriately.
/// </summary>
public class RenderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderException"/> class with a specified error message. The message provides details about the rendering error that occurred, allowing developers to understand the nature of the issue and take appropriate action. This constructor is typically used when throwing a <see cref="RenderException"/> to indicate a specific rendering-related problem.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RenderException(string message) : base(message) { }
}