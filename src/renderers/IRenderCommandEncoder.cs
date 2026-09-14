using System.Numerics;

namespace odl3d;

public readonly struct RenderObjectConstants
{
    public readonly Matrix4x4 Mvp;
    public readonly Matrix4x4 Model;
    public readonly Color Color;
    public readonly Color TextureColor;
    public readonly int UseTexture;
    public readonly int Lit;

    public RenderObjectConstants(Matrix4x4 mvp, Matrix4x4 model, Color color,
        Color textureColor, int useTexture, int lit)
    {
        Mvp = mvp;
        Model = model;
        Color = color;
        TextureColor = textureColor;
        UseTexture = useTexture;
        Lit = lit;
    }
}

/// <summary>
/// Records rendering commands for a single render pass.  Backends may execute
/// commands immediately (OpenGL) or defer them to a native command encoder
/// (Metal).
/// </summary>
public interface IRenderCommandEncoder
{
    void BindPipeline(ShaderProgram program);
    void BindVertexArray(VertexArray? vertexArray);
    void BindTexture(int slot, Texture? texture);
    void SetObjectConstants(in RenderObjectConstants constants);
    void SetViewport(int x, int y, int width, int height);
    void SetDepthTest(bool enabled);
    void SetDepthWrite(bool enabled);
    void SetBlend(bool enabled);
    void SetWireframe(bool enabled);
    void ClearColor(Color color);
    void ClearColorBuffer();
    void ClearDepthBuffer();
    void DrawIndexed(int count);
}
