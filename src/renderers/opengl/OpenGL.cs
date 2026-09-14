using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static odl3d.GL;

namespace odl3d.Renderers;

/// <summary>
/// Provides an OpenGL implementation of the IRenderer interface, which defines methods for creating and managing graphics resources, rendering operations, and shader programs. The OpenGL class encapsulates the functionality of the OpenGL graphics API, allowing for efficient rendering of 3D scenes and objects. It includes methods for initializing the renderer, creating and deleting buffers, textures, and shaders, binding resources, setting data, and performing draw calls. The OpenGL class is responsible for handling the specifics of the OpenGL API while providing a consistent interface for rendering operations.
/// </summary>
public partial class OpenGL : IRenderer
{
    private uint currentProgram;
    private readonly Dictionary<(uint Program, string Name), int> uniformLocations = new();
    public IRenderFrame BeginFrame(Color clearColor)
    {
        ClearColor(clearColor);
        return new Frame(this);
    }

    public void BeginRenderPass() { }
    public void EndRenderPass() { }
    public IRenderCommandEncoder CreateCommandEncoder() => new CommandEncoder(this);

    private sealed class CommandEncoder : IRenderCommandEncoder
    {
        private readonly OpenGL renderer;
        public CommandEncoder(OpenGL renderer) => this.renderer = renderer;
        public void BindPipeline(ShaderProgram program) => renderer.UseShaderProgram(program);
        public void BindVertexArray(VertexArray? vertexArray) => renderer.BindVertexArray(vertexArray);
        public void BindTexture(int slot, Texture? texture) => renderer.BindTexture(texture);
        public void SetObjectConstants(in RenderObjectConstants constants)
        {
            renderer.UseObjectConstants(constants);
        }
        public void SetViewport(int x, int y, int width, int height) => renderer.SetViewport(x, y, width, height);
        public void SetDepthTest(bool enabled) => renderer.SetEnableDepthTest(enabled);
        public void SetDepthWrite(bool enabled) => renderer.SetDepthMask(enabled);
        public void SetBlend(bool enabled) => renderer.SetAlphaBlending(enabled);
        public void SetWireframe(bool enabled) => renderer.SetWireFrame(enabled);
        public void ClearColor(Color color) => renderer.ClearColor(color);
        public void ClearColorBuffer() => renderer.ClearColorBuffer();
        public void ClearDepthBuffer() => renderer.ClearDepthBuffer();
        public void DrawIndexed(int count) => renderer.DrawElements(count);
    }

    public void ConfigureWindow()
    {
        GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
        GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 3);
        GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_CORE_PROFILE);
        GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_FORWARD_COMPAT, GLFW.GLFW_TRUE);
    }

    public void AttachWindow(IntPtr window) { }

    public void SetVSync(bool enabled) => GLFW.glfwSwapInterval(enabled ? 1 : 0);

    public void SetDrawableSize(int width, int height) { }

    public void Present(IntPtr window) => GLFW.glfwSwapBuffers(window);
    public void Present(IRenderFrame frame, IntPtr window)
    {
        frame.Dispose();
        Present(window);
    }

    public void ConfigureVertexAttribute(uint vao, uint vertexBuffer, int index, int size, int stride, int offset)
    {
        glBindVertexArray(vao);
        glBindBuffer(GL_ARRAY_BUFFER, vertexBuffer);
        glEnableVertexAttribArray((uint)index);
        glVertexAttribPointer((uint)index, size, GL_FLOAT, 0, stride, (nint)offset * sizeof(float));
        glBindVertexArray(0);
    }
    public void ConfigureVertexArray(uint vao, uint vertexBuffer, uint indexBuffer)
    {
        glBindVertexArray(vao);
        glBindBuffer(GL_ARRAY_BUFFER, vertexBuffer);
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBuffer);
        glBindVertexArray(0);
    }

    public void SetTextureParameters(uint texture, TextureFilter minFilter, MipmapFilter mipmap,
        TextureFilter magFilter, TextureWrap wrapH, TextureWrap wrapV, AnisotropicFilter anisotropic)
    {
        glBindTexture(GL_TEXTURE_2D, texture);
        SetTextureMinFilter(minFilter, mipmap);
        SetTextureMagFilter(magFilter);
        SetTextureWrapModeH(wrapH);
        SetTextureWrapModeV(wrapV);
        SetTextureAnisotropicFilter(anisotropic);
        glBindTexture(GL_TEXTURE_2D, 0);
    }

    public void GenerateMipmaps(uint texture)
    {
        glBindTexture(GL_TEXTURE_2D, texture);
        GenerateMipmaps();
        glBindTexture(GL_TEXTURE_2D, 0);
    }

    private sealed class Frame : IRenderFrame
    {
        private readonly OpenGL renderer;
        public Frame(OpenGL renderer) => this.renderer = renderer;
        public IRenderCommandEncoder BeginRenderPass() => renderer.CreateCommandEncoder();
        public void EndRenderPass(IRenderCommandEncoder encoder) { }
        public void Dispose() { }
    }

    /// <summary>
    /// Initializes the OpenGL renderer, setting up the necessary OpenGL context and state for rendering. This method is called to prepare the renderer for use, ensuring that the OpenGL functions are available and that the rendering context is properly configured. The Initialize method should be called before any rendering operations are performed, allowing the renderer to set up its internal state and resources.
    /// </summary>
    public void Initialize()
    {
        if (!Loaded) Load();
    }

    public void Dispose() { }

    public void SetViewport(int x, int y, int width, int height) => glViewport(x, y, width, height);

    public void SetEnableDepthTest(bool enable)
    {
        if (enable)
            glEnable(GL_DEPTH_TEST);
        else
            glDisable(GL_DEPTH_TEST);
    }

    public void SetDepthMask(bool enable) => glDepthMask(enable ? GL_TRUE : GL_FALSE);

    public void SetAlphaBlending(bool enable)
    {
        if (enable)
            glEnable(GL_BLEND);
        else
            glDisable(GL_BLEND);
        glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    }

    public void SetWireFrame(bool enable) => glPolygonMode(GL_FRONT_AND_BACK, enable ? GL_LINE : GL_FILL);

    public void ClearColor(Color color) => glClearColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    public void ClearColorBuffer() => glClear(GL_COLOR_BUFFER_BIT);

    public void ClearDepthBuffer() => glClear(GL_DEPTH_BUFFER_BIT);

    public void DrawElements(int count) => glDrawElements(GL_TRIANGLES, count, GL_UNSIGNED_INT, IntPtr.Zero);
}