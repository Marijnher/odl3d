using System;
using System.Numerics;
using System.Text;
using odl3d;
using static odl3d.GL;

namespace odl3d.Renderers;

public partial class OpenGL : IRenderer
{
    private void UseObjectConstants(in RenderObjectConstants constants)
    {
        SetUniformMatrix(GetUniformLocationForCurrent("uMVP"), constants.Mvp);
        SetUniformMatrix(GetUniformLocationForCurrent("uModel"), constants.Model);
        SetUniformInt(GetUniformLocationForCurrent("uUseTexture"), constants.UseTexture);
        SetUniformInt(GetUniformLocationForCurrent("uLit"), constants.Lit);
        SetUniformColor(GetUniformLocationForCurrent("uColor"), constants.Color);
        SetUniformColor(GetUniformLocationForCurrent("texColor"), constants.TextureColor);
    }

    private int GetUniformLocationForCurrent(string name)
    {
        var key = (currentProgram, name);
        if (!uniformLocations.TryGetValue(key, out int location))
        {
            location = glGetUniformLocation(currentProgram, name);
            uniformLocations[key] = location;
        }
        return location;
    }
    public uint CreateShader(ShaderType shaderType) => shaderType switch
    {
        ShaderType.Vertex => glCreateShader(GL_VERTEX_SHADER),
        ShaderType.Fragment => glCreateShader(GL_FRAGMENT_SHADER),
        _ => throw new ArgumentOutOfRangeException(nameof(shaderType), "Unknown shader type")
    };

    public void DeleteShader(Shader shader) => glDeleteShader(shader.Handle);

    public void SetShaderSource(Shader shader, string source) => glShaderSource(shader.Handle, 1, [source], [source.Length]);

    public bool CompileShader(Shader shader)
    {
        glCompileShader(shader.Handle);
        glGetShaderiv(shader.Handle, GL_COMPILE_STATUS, out int success);
        return success == 1;
    }

    public string GetShaderLog(Shader shader)
    {
        byte[] log = new byte[1024];
        glGetShaderInfoLog(shader.Handle, log.Length, out int len, log);
        return Encoding.ASCII.GetString(log, 0, len);
    }

    public uint CreateShaderProgram() => glCreateProgram();

    public void DeleteShaderProgram(ShaderProgram program) => glDeleteProgram(program.Handle);

    public void AttachShader(ShaderProgram program, Shader shader) => glAttachShader(program.Handle, shader.Handle);

    public bool LinkShaderProgram(ShaderProgram program) 
    {
        glLinkProgram(program.Handle);
        glGetProgramiv(program.Handle, GL_LINK_STATUS, out int success);
        return success == 1;
    }

    public string GetShaderProgramLog(ShaderProgram program)
    {
        byte[] log = new byte[1024];
        glGetProgramInfoLog(program.Handle, log.Length, out int len, log);
        return Encoding.ASCII.GetString(log, 0, len);
    }

    public void UseShaderProgram(ShaderProgram program)
    {
        currentProgram = program.Handle;
        glUseProgram(program.Handle);
    }

    public int GetUniformLocation(ShaderProgram program, string name) => glGetUniformLocation(program.Handle, name);

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