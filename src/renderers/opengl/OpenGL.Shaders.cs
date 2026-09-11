using System;
using System.Numerics;
using System.Text;
using odl3d;
using static odl3d.GL;

namespace odl3d.Renderers;

public partial class OpenGL : IRenderer
{
    public uint CreateShader(ShaderType shaderType) => shaderType switch
    {
        ShaderType.Vertex => glCreateShader(GL_VERTEX_SHADER),
        ShaderType.Fragment => glCreateShader(GL_FRAGMENT_SHADER),
        _ => throw new ArgumentOutOfRangeException(nameof(shaderType), "Unknown shader type")
    };

    public void DeleteShader(uint shader) => glDeleteShader(shader);

    public void SetShaderSource(uint shader, string source) => glShaderSource(shader, 1, [source], [source.Length]);

    public bool CompileShader(uint shader)
    {
        glCompileShader(shader);
        glGetShaderiv(shader, GL_COMPILE_STATUS, out int success);
        return success == 1;
    }

    public string GetShaderLog(uint shader)
    {
        byte[] log = new byte[1024];
        glGetShaderInfoLog(shader, log.Length, out int len, log);
        return Encoding.ASCII.GetString(log, 0, len);
    }

    public uint CreateShaderProgram() => glCreateProgram();

    public void DeleteShaderProgram(uint program) => glDeleteProgram(program);

    public void AttachShader(uint program, uint shader) => glAttachShader(program, shader);

    public bool LinkShaderProgram(uint program) 
    {
        glLinkProgram(program);
        glGetProgramiv(program, GL_LINK_STATUS, out int success);
        return success == 1;
    }

    public string GetShaderProgramLog(uint program)
    {
        byte[] log = new byte[1024];
        glGetProgramInfoLog(program, log.Length, out int len, log);
        return Encoding.ASCII.GetString(log, 0, len);
    }

    public void UseShaderProgram(uint program) => glUseProgram(program);

    public int GetUniformLocation(uint program, string name) => glGetUniformLocation(program, name);

    public void SetUniformMatrix(int location, Matrix4x4 matrix) 
    {
        float[] data =
        [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        ];
        glUniformMatrix4fv(location, 1, 0, data);
    }

    public void SetUniformInt(int location, int value) => glUniform1i(location, value);

    public void SetUniformColor(int location, Color color) => glUniform4f(location, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
}