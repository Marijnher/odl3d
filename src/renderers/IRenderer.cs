using System;
using System.Numerics;

namespace odl3d;

public interface IRenderer : IDisposable
{
    public void Initialize();

    public void SetViewport(int x, int y, int width, int height);

    public void SetEnableDepthTest(bool enable);

    public void SetDepthMask(bool enable);

    public void SetAlphaBlending(bool enable);

    public void SetWireFrame(bool enable);

    #region Texture Methods
    public uint CreateTexture();
    public void DeleteTexture(Texture texture);
    public void BindTexture(Texture? texture);
    public void SetTextureMinFilter(TextureFilter filterMode, MipmapFilter mipmapFilter);
    public void SetTextureMagFilter(TextureFilter filterMode);
    public void SetTextureWrapModeH(TextureWrap wrapModeH);
    public void SetTextureWrapModeV(TextureWrap wrapModeV);
    public void SetTextureAnisotropicFilter(AnisotropicFilter anisotropicFilter);
    public void UploadTexture(Texture texture);
    public void GenerateMipmaps();
    #endregion

    #region Shader Methods
    public uint CreateShader(ShaderType shaderType);
    public void DeleteShader(uint shader);
    public void SetShaderSource(uint shader, string source);
    public bool CompileShader(uint shader);
    public string GetShaderLog(uint shader);
    public uint CreateShaderProgram();
    public void DeleteShaderProgram(uint program);
    public void AttachShader(uint program, uint shader);
    public bool LinkShaderProgram(uint program);
    public string GetShaderProgramLog(uint program);
    public void UseShaderProgram(uint program);
    public int GetUniformLocation(uint program, string name);
    public void SetUniformMatrix(int location, Matrix4x4 data);
    public void SetUniformInt(int location, int data);
    public void SetUniformColor(int location, Color color);
    #endregion

    public void ClearColor(Color color);

    public void ClearColorBuffer();

    public void ClearDepthBuffer();
}