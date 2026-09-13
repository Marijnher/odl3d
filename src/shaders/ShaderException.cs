using System;

/// <summary>
/// An exception that is thrown when a shader fails to compile or link. This exception can be used to indicate errors in shader source code or issues in the underlying renderer's shader compilation and linking process. The message of the exception typically contains the error log from the shader compiler or linker, providing details about what went wrong.
/// </summary>
class ShaderException : Exception
{
    /// <summary>
    /// Creates a new ShaderException with the specified error message. The message should describe the reason for the exception, such as a compilation or linking error in the shader program. This constructor can be used to create an instance of ShaderException when a shader fails to compile or link, allowing the application to handle the error appropriately.
    /// </summary>
    /// <param name="message">The error message describing the reason for the exception.</param>
    public ShaderException(string message) : base(message) { }
}