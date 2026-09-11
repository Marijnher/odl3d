using System;
using odl3d;
using static odl3d.GL;

namespace odl3d.Renderers;

public partial class OpenGL : IRenderer
{
    public uint CreateTexture()
    {
        glGenTextures(1, out uint handle);
        return handle;
    }
    
    public void DeleteTexture(Texture texture)
    {
        uint handle = texture.Handle;
        glDeleteTextures(1, ref handle);
    }

    public void BindTexture(Texture? texture) => glBindTexture(GL_TEXTURE_2D, texture?.Handle ?? 0);

    public void SetTextureMinFilter(TextureFilter filterMode, MipmapFilter mipmapFilter)
    {
        int minFilterValue = (filterMode, mipmapFilter) switch
        {
            (TextureFilter.Nearest, MipmapFilter.None) => GL_NEAREST,
            (TextureFilter.Nearest, MipmapFilter.Nearest) => GL_NEAREST_MIPMAP_NEAREST,
            (TextureFilter.Nearest, MipmapFilter.Linear) => GL_NEAREST_MIPMAP_LINEAR,
            (TextureFilter.Linear, MipmapFilter.None) => GL_LINEAR,
            (TextureFilter.Linear, MipmapFilter.Nearest) => GL_LINEAR_MIPMAP_NEAREST,
            (TextureFilter.Linear, MipmapFilter.Linear) => GL_LINEAR_MIPMAP_LINEAR,
            (_, _) => throw new RenderException("Invalid combination of texture and mipmap filters.")
        };
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, minFilterValue);
    }

    public void SetTextureMagFilter(TextureFilter filterMode) => glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int) filterMode);

    public void SetTextureWrapModeH(TextureWrap wrapModeH) => glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (int) wrapModeH);

    public void SetTextureWrapModeV(TextureWrap wrapModeV) => glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (int) wrapModeV);

    public void SetTextureAnisotropicFilter(AnisotropicFilter anisotropicFilter) =>
        glTexParameterf(GL_TEXTURE_2D, GL_TEXTURE_MAX_ANISOTROPY_EXT, (int) anisotropicFilter);

    public void UploadTexture(Texture texture) =>
        glTexImage2D(GL.GL_TEXTURE_2D, 0, GL_RGBA, texture.Width, texture.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, texture.Pixels);

    public void GenerateMipmaps() => glGenerateMipmap(GL_TEXTURE_2D);
}