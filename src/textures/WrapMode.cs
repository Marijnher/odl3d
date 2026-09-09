namespace odl3d;

public enum TextureWrap
{
    Repeat = (int) GL.GL_REPEAT,
    Clamp  = (int) GL.GL_CLAMP_TO_EDGE,
    Mirror = (int) GL.GL_MIRRORED_REPEAT,
    Border = 0
}