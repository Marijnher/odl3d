using System;
using System.Numerics;
using System.Text;
using odl3d;
using static odl3d.GL;

namespace odl3d.Renderer;

public partial class OpenGL : IRendererOld
{
    public uint CreateShader(ShaderStage shaderType) => shaderType switch
    {
        ShaderStage.Vertex => glCreateShader(GL_VERTEX_SHADER),
        ShaderStage.Fragment => glCreateShader(GL_FRAGMENT_SHADER),
        _ => throw new ArgumentOutOfRangeException(nameof(shaderType), "Unknown shader type")
    };

    public void DeleteShader(Shader shader) { } //glDeleteShader(shader.Handle);

    public void SetShaderSource(Shader shader, string source) { }//glShaderSource(shader.Handle, 1, [source], [source.Length]);

    public bool CompileShader(Shader shader)
    {
        // glCompileShader(shader.Handle);
        // glGetShaderiv(shader.Handle, GL_COMPILE_STATUS, out int success);
        // return success == 1;
        return false;
    }

    public string GetShaderLog(Shader shader)
    {
        // byte[] log = new byte[1024];
        // glGetShaderInfoLog(shader.Handle, log.Length, out int len, log);
        // return Encoding.ASCII.GetString(log, 0, len);
        return "No OpenGL shader support";
    }

    public uint CreateShaderProgram() => glCreateProgram();

    public void DeleteShaderProgram(ShaderPipeline program) { }//=> glDeleteProgram(program.Handle);

    public void AttachShader(ShaderPipeline program, Shader shader) { } //=> glAttachShader(program.Handle, shader.Handle);

    public bool LinkShaderProgram(ShaderPipeline program) 
    {
        // glLinkProgram(program.Handle);
        // glGetProgramiv(program.Handle, GL_LINK_STATUS, out int success);
        // return success == 1;
        return false;
    }

    public string GetShaderProgramLog(ShaderPipeline program)
    {
        // byte[] log = new byte[1024];
        // glGetProgramInfoLog(program.Handle, log.Length, out int len, log);
        // return Encoding.ASCII.GetString(log, 0, len);
        return "No OpenGL shader support";
    }

    public void UseShaderProgram(ShaderPipeline program) { } //=> glUseProgram(program.Handle);

    public int GetUniformLocation(ShaderPipeline program, string name) => -1; //=> glGetUniformLocation(program.Handle, name);

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