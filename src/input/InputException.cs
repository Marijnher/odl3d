using System;

public class InputException : Exception
{
    public InputException(string message) : base(message) { }
}