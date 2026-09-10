using System;

namespace odl3d;

/// <summary>
/// Represents errors that occur during input handling in the application.
/// </summary>
public class InputException : Exception
{
    /// <summary>
    /// Initializes a new instance of the InputException class with the specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InputException(string message) : base(message) { }
}