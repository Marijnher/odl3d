using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace odl3d.Renderer.OpenGLAdapter;

public static class ShaderPreprocessor
{
    static readonly Regex BlockBinding = new(
        @"layout\s*\(\s*std140\s*,\s*binding\s*=\s*(\d+)\s*\)\s*uniform\s+(\w+)",
        RegexOptions.Compiled);

    public static string Process(string source, List<(string Name, int Slot)> blocks)
    {
        return BlockBinding.Replace(source, m =>
        {
            blocks.Add((m.Groups[2].Value, int.Parse(m.Groups[1].Value)));
            return $"layout(std140) uniform {m.Groups[2].Value}";
        });
    }
}