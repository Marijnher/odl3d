using System;

namespace odl3d;

/// <summary>
/// Owns renderer-device and resource lifetime. Per-frame rendering is performed
/// through an explicitly passed <see cref="IRenderCommandEncoder"/>.
/// </summary>
public interface IRenderer : IDisposable
{
    void ConfigureWindow();
    void AttachWindow(IntPtr window);
    void SetVSync(bool enabled);
    void SetDrawableSize(int width, int height);
    void Initialize();
    IRenderFrame BeginFrame(Color clearColor);
    void Present(IRenderFrame frame, IntPtr window);

    uint CreateVertexArray();
    void DeleteVertexArray(VertexArray vao);
    uint CreateBuffer();
    void DeleteBuffer(Buffer buffer);
    void SetBufferData(BufferTarget target, uint buffer, float[] data, BufferHint hint);
    void SetBufferData(BufferTarget target, uint buffer, uint[] data, BufferHint hint);
    void ConfigureVertexArray(uint vao, uint vertexBuffer, uint indexBuffer);
    void ConfigureVertexAttribute(uint vao, uint vertexBuffer, int index, int size, int stride, int offset);

    uint CreateTexture();
    void DeleteTexture(Texture texture);
    void SetTextureParameters(uint texture, TextureFilter minFilter, MipmapFilter mipmap,
        TextureFilter magFilter, TextureWrap wrapH, TextureWrap wrapV, AnisotropicFilter anisotropic);
    void UploadTexture(Texture texture);
    void GenerateMipmaps(uint texture);

    uint CreateShader(ShaderType shaderType);
    void DeleteShader(Shader shader);
    void SetShaderSource(Shader shader, string source);
    bool CompileShader(Shader shader);
    string GetShaderLog(Shader shader);
    uint CreateShaderProgram();
    void DeleteShaderProgram(ShaderProgram program);
    void AttachShader(ShaderProgram program, Shader shader);
    bool LinkShaderProgram(ShaderProgram program);
    string GetShaderProgramLog(ShaderProgram program);
}
