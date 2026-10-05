using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// A snapshot of the OpenGL context's supported features and limits.
/// </summary>
internal sealed class OpenGLRenderCapabilities : IRenderCapabilities
{
    public bool SupportsWireframe { get; }
    public bool SupportsComputeShaders { get; }
    public bool SupportsGeometryShaders { get; }
    public bool SupportsAnisotropicFiltering { get; }
    public bool SupportsIndependentBlend { get; }
    public bool SupportsIndirectDraw { get; }
    public int MaxTextureSize { get; }
    public int MaxAnisotropy { get; }
    public int MaxVertexBufferSlots => GLVertexLayout.MaxSlots;
    public int MaxTextureSlots { get; }

    public OpenGLRenderCapabilities()
    {
        GL.glGetIntegerv(GL.GL_MAJOR_VERSION, out int majorVersion);
        GL.glGetIntegerv(GL.GL_MINOR_VERSION, out int minorVersion);

        GL.glGetIntegerv(GL.GL_NUM_EXTENSIONS, out int extensionCount);
        HashSet<string> extensions = new(StringComparer.Ordinal);
        for (uint i = 0; i < extensionCount; i++)
        {
            IntPtr extensionPointer = GL.glGetStringi(GL.GL_EXTENSIONS, i);
            string? extension = Marshal.PtrToStringAnsi(extensionPointer);
            if (extension != null) extensions.Add(extension);
        }

        bool VersionAtLeast(int major, int minor) =>
            majorVersion > major || majorVersion == major && minorVersion >= minor;
        bool HasExtension(string extension) => extensions.Contains(extension);

        SupportsWireframe = true;
        SupportsComputeShaders = VersionAtLeast(4, 3) || HasExtension("GL_ARB_compute_shader");
        SupportsGeometryShaders = VersionAtLeast(3, 2) || HasExtension("GL_ARB_geometry_shader4");
        SupportsAnisotropicFiltering = HasExtension("GL_EXT_texture_filter_anisotropic");
        SupportsIndependentBlend = VersionAtLeast(4, 0) || HasExtension("GL_ARB_draw_buffers_blend");
        SupportsIndirectDraw = VersionAtLeast(4, 0) || HasExtension("GL_ARB_draw_indirect");

        GL.glGetIntegerv(GL.GL_MAX_TEXTURE_SIZE, out int maxTextureSize);
        GL.glGetIntegerv(GL.GL_MAX_TEXTURE_IMAGE_UNITS, out int maxTextureSlots);
        MaxTextureSize = maxTextureSize;
        MaxTextureSlots = maxTextureSlots;

        if (SupportsAnisotropicFiltering)
        {
            GL.glGetFloatv(GL.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out float maxAnisotropy);
            MaxAnisotropy = Math.Max(1, (int) MathF.Floor(maxAnisotropy));
        }
        else
        {
            MaxAnisotropy = 1;
        }
    }
}