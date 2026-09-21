using System;
using System.Runtime.InteropServices;

namespace odl3d;

/// <summary>
/// Minimal OpenGL 3.3 core function bindings, resolved dynamically via GLFW's GetProcAddress.
/// </summary>
internal static class GL
{
    public const uint GL_FALSE = 0;
    public const uint GL_TRUE = 1;
    public const uint GL_NEVER = 0x0200;
    public const uint GL_LESS = 0x0201;
    public const uint GL_EQUAL = 0x0202;
    public const uint GL_LEQUAL = 0x0203;
    public const uint GL_GREATER = 0x0204;
    public const uint GL_NOTEQUAL = 0x0205;
    public const uint GL_GEQUAL = 0x0206;
    public const uint GL_ALWAYS = 0x0207;
    public const uint GL_COLOR_BUFFER_BIT = 0x4000;
    public const uint GL_DEPTH_BUFFER_BIT = 0x0100;
    public const uint GL_DEPTH_TEST = 0x0B71;
    public const uint GL_SCISSOR_TEST = 0x0C11;
    public const uint GL_BLEND = 0x0BE2;
    public const uint GL_SRC_ALPHA = 0x0302;
    public const uint GL_ONE_MINUS_SRC_ALPHA = 0x0303;
    public const uint GL_VERTEX_SHADER = 0x8B31;
    public const uint GL_FRAGMENT_SHADER = 0x8B30;
    public const uint GL_COMPUTE_SHADER = 0x91B9;
    public const uint GL_COMPILE_STATUS = 0x8B81;
    public const uint GL_LINK_STATUS = 0x8B82;
    public const uint GL_ARRAY_BUFFER = 0x8892;
    public const uint GL_ELEMENT_ARRAY_BUFFER = 0x8893;
    public const uint GL_STATIC_DRAW = 0x88E4;
    public const uint GL_DYNAMIC_DRAW = 0x88E8;
    public const uint GL_STREAM_DRAW = 0x88E0;
    public const uint GL_UNSIGNED_BYTE = 0x1401;
    public const uint GL_UNSIGNED_SHORT = 0x1403;
    public const uint GL_INT = 0x1404;
    public const uint GL_UNSIGNED_INT = 0x1405;
    public const uint GL_FLOAT = 0x1406;
    public const uint GL_HALF_FLOAT = 0x140B;
    public const uint GL_TRIANGLES = 0x0004;
    public const uint GL_TRIANGLE_STRIP = 0x0005;
    public const uint GL_LINES = 0x0001;
    public const uint GL_LINE_STRIP = 0x0003;
    public const uint GL_TEXTURE_2D = 0x0DE1;
    public const uint GL_COPY_WRITE_BUFFER = 0x8F37;
    public const int GL_NEAREST_MIPMAP_NEAREST = 0x2700;
    public const int GL_LINEAR_MIPMAP_NEAREST = 0x2701;
    public const int GL_NEAREST_MIPMAP_LINEAR = 0x2702;
    public const int GL_LINEAR_MIPMAP_LINEAR = 0x2703;
    public const int GL_TEXTURE_MIN_FILTER = 0x2801;
    public const int GL_TEXTURE_MAG_FILTER = 0x2800;
    public const int GL_NEAREST = 0x2600;
    public const int GL_LINEAR = 0x2601;
    public const uint GL_TEXTURE_WRAP_S = 0x2802;
    public const uint GL_TEXTURE_WRAP_T = 0x2803;
    public const int GL_CLAMP_TO_EDGE = 0x812F;
    public const int GL_RGBA = 0x1908;
    public const int GL_REPEAT = 0x2901;
    public const int GL_MIRRORED_REPEAT = 0x8370;
    public const uint GL_TEXTURE0 = 0x84C0;
    public const uint GL_FRONT_AND_BACK = 0x0408;
    public const uint GL_LINE = 0x1B01;
    public const uint GL_FILL = 0x1B02;
    public const uint GL_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FE;
    public const uint GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FF;
    public const uint GL_UNIFORM_BUFFER = 0x8A11;
    public const uint GL_INVALID_INDEX = 0xFFFFFFFF;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glClearColor(float r, float g, float b, float a);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glClearDepth(double depth);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glClear(uint mask);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glViewport(int x, int y, int width, int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glScissor(int x, int y, int width, int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glEnable(uint cap);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDisable(uint cap);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGetFloatv(uint pname, out float data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint d_glCreateShader(uint type);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glShaderSource(uint shader, int count, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] strings, int[] length);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glCompileShader(uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGetShaderiv(uint shader, uint pname, out int param);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGetShaderInfoLog(uint shader, int maxLength, out int length, byte[] infoLog);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDeleteShader(uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint d_glCreateProgram();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glAttachShader(uint program, uint shader);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glLinkProgram(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGetProgramiv(uint program, uint pname, out int param);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGetProgramInfoLog(uint program, int maxLength, out int length, byte[] infoLog);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDeleteProgram(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glUseProgram(uint program);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int d_glGetUniformLocation(uint program, [MarshalAs(UnmanagedType.LPStr)] string name);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glUniformMatrix4fv(int location, int count, byte transpose, float[] value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glUniform1i(int location, int v0);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glUniform4f(int location, float v0, float v1, float v2, float v3);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint d_glGetUniformBlockIndex(uint program, [MarshalAs(UnmanagedType.LPStr)] string name);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glUniformBlockBinding(uint program, uint index, uint binding);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glBindBufferRange(uint target, uint index, uint buffer, IntPtr offset, IntPtr size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGenVertexArrays(int n, out uint arrays);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glBindVertexArray(uint array);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDeleteVertexArrays(int n, ref uint arrays);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGenBuffers(int n, out uint buffers);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glBindBuffer(uint target, uint buffer);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDeleteBuffers(int n, ref uint buffers);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glBufferData(uint target, IntPtr size, IntPtr data, uint usage);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glEnableVertexAttribArray(int index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDisableVertexAttribArray(int index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glVertexAttribPointer(uint index, int size, uint type, byte normalized, int stride, IntPtr pointerOffset);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glVertexAttribIPointer(uint index, int size, uint type, int stride, IntPtr pointerOffset);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glVertexAttribDivisor(uint index, uint divisor);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDrawArrays(uint mode, int first, int count);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDrawElements(uint mode, int count, uint type, IntPtr indices);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glPolygonMode(uint face, uint mode);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGenTextures(int n, out uint textures);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glBindTexture(uint target, uint texture);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDeleteTextures(int n, ref uint textures);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glTexParameteri(uint target, uint pname, int param);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glTexParameterf(uint target, uint pname, float param);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glTexImage2D(uint target, int level, int internalFormat, int width, int height, int border, uint format, uint type, byte[] pixels);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glActiveTexture(uint texture);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glGenerateMipmap(uint target);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glBlendFunc(uint sfactor, uint dfactor);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDepthMask(uint flag);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glDepthFunc(uint func);

#pragma warning disable CS8618
    public static d_glClearColor glClearColor;
    public static d_glClearDepth glClearDepth;
    public static d_glClear glClear;
    public static d_glViewport glViewport;
    public static d_glScissor glScissor;
    public static d_glEnable glEnable;
    public static d_glDisable glDisable;
    public static d_glGetFloatv glGetFloatv;

    public static d_glCreateShader glCreateShader;
    public static d_glShaderSource glShaderSource;
    public static d_glCompileShader glCompileShader;
    public static d_glGetShaderiv glGetShaderiv;
    public static d_glGetShaderInfoLog glGetShaderInfoLog;
    public static d_glDeleteShader glDeleteShader;
    public static d_glCreateProgram glCreateProgram;
    public static d_glAttachShader glAttachShader;
    public static d_glLinkProgram glLinkProgram;
    public static d_glGetProgramiv glGetProgramiv;
    public static d_glGetProgramInfoLog glGetProgramInfoLog;
    public static d_glDeleteProgram glDeleteProgram;
    public static d_glUseProgram glUseProgram;
    public static d_glGetUniformLocation glGetUniformLocation;
    public static d_glUniformMatrix4fv glUniformMatrix4fv;
    public static d_glUniform1i glUniform1i;
    public static d_glUniform4f glUniform4f;
    public static d_glGetUniformBlockIndex glGetUniformBlockIndex;
    public static d_glUniformBlockBinding glUniformBlockBinding;
    public static d_glBindBufferRange glBindBufferRange;

    public static d_glGenVertexArrays glGenVertexArrays;
    public static d_glBindVertexArray glBindVertexArray;
    public static d_glDeleteVertexArrays glDeleteVertexArrays;
    public static d_glGenBuffers glGenBuffers;
    public static d_glBindBuffer glBindBuffer;
    public static d_glDeleteBuffers glDeleteBuffers;
    public static d_glBufferData glBufferData;
    public static d_glEnableVertexAttribArray glEnableVertexAttribArray;
    public static d_glDisableVertexAttribArray glDisableVertexAttribArray;
    public static d_glVertexAttribPointer glVertexAttribPointer;
    public static d_glVertexAttribIPointer glVertexAttribIPointer;
    public static d_glVertexAttribDivisor glVertexAttribDivisor;
    public static d_glDrawArrays glDrawArrays;
    public static d_glDrawElements glDrawElements;
    public static d_glPolygonMode glPolygonMode;

    public static d_glGenTextures glGenTextures;
    public static d_glBindTexture glBindTexture;
    public static d_glDeleteTextures glDeleteTextures;
    public static d_glTexParameteri glTexParameteri;
    public static d_glTexParameterf glTexParameterf;
    public static d_glTexImage2D glTexImage2D;
    public static d_glActiveTexture glActiveTexture;
    public static d_glGenerateMipmap glGenerateMipmap;

    public static d_glBlendFunc glBlendFunc;
    public static d_glDepthMask glDepthMask;
    public static d_glDepthFunc glDepthFunc;
#pragma warning restore CS8618

    public static float MaxAnisotropy { get; private set; }

    public static bool Loaded { get; private set; }

    public static void Load()
    {
        if (Loaded) return;

        glClearColor = Get<d_glClearColor>("glClearColor");
        glClearDepth = Get<d_glClearDepth>("glClearDepth");
        glClear = Get<d_glClear>("glClear");
        glViewport = Get<d_glViewport>("glViewport");
        glScissor = Get<d_glScissor>("glScissor");
        glEnable = Get<d_glEnable>("glEnable");
        glDisable = Get<d_glDisable>("glDisable");
        glGetFloatv = Get<d_glGetFloatv>("glGetFloatv");

        glCreateShader = Get<d_glCreateShader>("glCreateShader");
        glShaderSource = Get<d_glShaderSource>("glShaderSource");
        glCompileShader = Get<d_glCompileShader>("glCompileShader");
        glGetShaderiv = Get<d_glGetShaderiv>("glGetShaderiv");
        glGetShaderInfoLog = Get<d_glGetShaderInfoLog>("glGetShaderInfoLog");
        glDeleteShader = Get<d_glDeleteShader>("glDeleteShader");
        glCreateProgram = Get<d_glCreateProgram>("glCreateProgram");
        glAttachShader = Get<d_glAttachShader>("glAttachShader");
        glLinkProgram = Get<d_glLinkProgram>("glLinkProgram");
        glGetProgramiv = Get<d_glGetProgramiv>("glGetProgramiv");
        glGetProgramInfoLog = Get<d_glGetProgramInfoLog>("glGetProgramInfoLog");
        glDeleteProgram = Get<d_glDeleteProgram>("glDeleteProgram");
        glUseProgram = Get<d_glUseProgram>("glUseProgram");
        glGetUniformLocation = Get<d_glGetUniformLocation>("glGetUniformLocation");
        glUniformMatrix4fv = Get<d_glUniformMatrix4fv>("glUniformMatrix4fv");
        glUniform1i = Get<d_glUniform1i>("glUniform1i");
        glUniform4f = Get<d_glUniform4f>("glUniform4f");
        glGetUniformBlockIndex = Get<d_glGetUniformBlockIndex>("glGetUniformBlockIndex");
        glUniformBlockBinding = Get<d_glUniformBlockBinding>("glUniformBlockBinding");
        glBindBufferRange = Get<d_glBindBufferRange>("glBindBufferRange");

        glGenVertexArrays = Get<d_glGenVertexArrays>("glGenVertexArrays");
        glBindVertexArray = Get<d_glBindVertexArray>("glBindVertexArray");
        glDeleteVertexArrays = Get<d_glDeleteVertexArrays>("glDeleteVertexArrays");
        glGenBuffers = Get<d_glGenBuffers>("glGenBuffers");
        glBindBuffer = Get<d_glBindBuffer>("glBindBuffer");
        glDeleteBuffers = Get<d_glDeleteBuffers>("glDeleteBuffers");
        glBlendFunc = Get<d_glBlendFunc>("glBlendFunc");
        glDepthMask = Get<d_glDepthMask>("glDepthMask");
        glDepthFunc = Get<d_glDepthFunc>("glDepthFunc");
        glBufferData = Get<d_glBufferData>("glBufferData");
        glEnableVertexAttribArray = Get<d_glEnableVertexAttribArray>("glEnableVertexAttribArray");
        glDisableVertexAttribArray = Get<d_glDisableVertexAttribArray>("glDisableVertexAttribArray");
        glVertexAttribPointer = Get<d_glVertexAttribPointer>("glVertexAttribPointer");
        glVertexAttribIPointer = Get<d_glVertexAttribIPointer>("glVertexAttribIPointer");
        glVertexAttribDivisor = Get<d_glVertexAttribDivisor>("glVertexAttribDivisor");
        glDrawArrays = Get<d_glDrawArrays>("glDrawArrays");
        glDrawElements = Get<d_glDrawElements>("glDrawElements");
        glPolygonMode = Get<d_glPolygonMode>("glPolygonMode");

        glGenTextures = Get<d_glGenTextures>("glGenTextures");
        glBindTexture = Get<d_glBindTexture>("glBindTexture");
        glDeleteTextures = Get<d_glDeleteTextures>("glDeleteTextures");
        glTexParameteri = Get<d_glTexParameteri>("glTexParameteri");
        glTexParameterf = Get<d_glTexParameterf>("glTexParameterf");
        glTexImage2D = Get<d_glTexImage2D>("glTexImage2D");
        glActiveTexture = Get<d_glActiveTexture>("glActiveTexture");
        glGenerateMipmap = Get<d_glGenerateMipmap>("glGenerateMipmap");

        glGetFloatv(GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out float maxAnisotropy);
        MaxAnisotropy = maxAnisotropy;

        Loaded = true;
    }

    private static TDelegate Get<TDelegate>(string name) where TDelegate : Delegate
    {
        IntPtr ptr = GLFW.glfwGetProcAddress(name);
        if (ptr == IntPtr.Zero)
            throw new EntryPointNotFoundException($"Could not find OpenGL function '{name}'.");
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
    }
}
