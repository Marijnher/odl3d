using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Provides functionality for preprocessing OpenGL shader source code, specifically handling uniform block bindings.
/// </summary>
internal static class ShaderPreprocessor
{
    /// <summary>
    /// The regular expression used to match uniform block bindings in the shader source code.
    /// </summary>
    private static readonly Regex BlockBinding = new(
        @"layout\s*\(\s*std140\s*,\s*binding\s*=\s*(\d+)\s*\)\s*uniform\s+(\w+)",
        RegexOptions.Compiled);

    /// <summary>
    /// Processes the shader source code, extracting uniform block bindings and updating the provided list of blocks.
    /// </summary>
    /// <param name="source">The shader source code to process.</param>
    /// <param name="blocks">The list of uniform block bindings to update.</param>
    /// <returns>The processed shader source code with uniform block bindings removed.</returns>
    public static string Process(string source, List<(string Name, int Slot)> blocks)
    {
        return BlockBinding.Replace(source, m =>
        {
            blocks.Add((m.Groups[2].Value, int.Parse(m.Groups[1].Value)));
            return $"layout(std140) uniform {m.Groups[2].Value}";
        });
    }
}