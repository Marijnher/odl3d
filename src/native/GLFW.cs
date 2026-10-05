using System;
using System.Runtime.InteropServices;

namespace odl3d;

/// <summary>
/// Minimal dynamic bindings for the GLFW native library (window/context/input only).
/// </summary>
internal static class GLFW
{
    public const int GLFW_FALSE = 0;
    public const int GLFW_TRUE = 1;
    public const int GLFW_RESIZABLE = 0x00020003;
    public const int GLFW_VISIBLE = 0x00020004;
    public const int GLFW_MAXIMIZED = 0x00020008;
    public const int GLFW_CONTEXT_VERSION_MAJOR = 0x00022002;
    public const int GLFW_CONTEXT_VERSION_MINOR = 0x00022003;
    public const int GLFW_OPENGL_FORWARD_COMPAT = 0x00022006;
    public const int GLFW_OPENGL_PROFILE = 0x00022008;
    public const int GLFW_OPENGL_CORE_PROFILE = 0x00032001;
    public const int GLFW_CLIENT_API = 0x00022001;
    public const int GLFW_NO_API = 0;
    public const int GLFW_NATIVE_COCOA = 0;
    public const int GLFW_RELEASE = 0;
    public const int GLFW_PRESS = 1;
    public const int GLFW_REPEAT = 2;
    public const int GLFW_CURSOR = 0x00033001;
    public const int GLFW_CURSOR_NORMAL = 0x00034001;
    public const int GLFW_CURSOR_DISABLED = 0x00034003;
    public const int GLFW_KEY_LAST = 348;
    public const int GLFW_KEY_ESCAPE = 256;
    public const int GLFW_KEY_W = 87;
    public const int GLFW_KEY_A = 65;
    public const int GLFW_KEY_S = 83;
    public const int GLFW_KEY_D = 68;
    public const int GLFW_KEY_SPACE = 32;
    public const int GLFW_KEY_LEFT_SHIFT = 340;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int d_glfwInit();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwTerminate();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwGetVersion(out int major, out int minor, out int revision);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwWindowHint(int hint, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwGetWindowSize(IntPtr window, out int width, out int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwGetFramebufferSize(IntPtr window, out int width, out int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSetWindowSize(IntPtr window, int width, int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSetWindowPos(IntPtr window, int xpos, int ypos);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwMaximizeWindow(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwRestoreWindow(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr d_glfwGetPrimaryMonitor();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr d_glfwGetMonitorWorkarea(IntPtr monitor, out int xpos, out int ypos, out int width, out int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr d_glfwCreateWindow(int width, int height, [MarshalAs(UnmanagedType.LPStr)] string title, IntPtr monitor, IntPtr share);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwDestroyWindow(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwMakeContextCurrent(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr d_glfwGetCurrentContext();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSwapBuffers(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSwapInterval(int interval);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwPollEvents();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int d_glfwWindowShouldClose(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSetWindowShouldClose(IntPtr window, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr d_glfwGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string procname);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate IntPtr d_glfwGetCocoaWindow(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int d_glfwGetKey(IntPtr window, int key);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwGetCursorPos(IntPtr window, out double xpos, out double ypos);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSetInputMode(IntPtr window, int mode, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate double d_glfwGetTime();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSetKeyCallback(IntPtr window, GLFWkeyfun? callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void GLFWkeyfun(IntPtr window, int key, int scancode, int action, int mods);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSetMouseButtonCallback(IntPtr window, GLFWmousebuttonfun? callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void GLFWmousebuttonfun(IntPtr window, int button, int action, int mods);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwSetCursorPosCallback(IntPtr window, GLFWcursorposfun? callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void d_glfwGetWindowContentScale(IntPtr window, out float xscale, out float yscale);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate void GLFWcursorposfun(IntPtr window, double xpos, double ypos);

#pragma warning disable CS8618
    public static d_glfwInit glfwInit;
    public static d_glfwTerminate glfwTerminate;
    public static d_glfwGetVersion glfwGetVersion;
    public static d_glfwWindowHint glfwWindowHint;
    public static d_glfwGetWindowSize glfwGetWindowSize;
    public static d_glfwGetFramebufferSize glfwGetFramebufferSize;
    public static d_glfwSetWindowSize glfwSetWindowSize;
    public static d_glfwSetWindowPos glfwSetWindowPos;
    public static d_glfwMaximizeWindow glfwMaximizeWindow;
    public static d_glfwRestoreWindow glfwRestoreWindow;
    public static d_glfwGetPrimaryMonitor glfwGetPrimaryMonitor;
    public static d_glfwGetMonitorWorkarea glfwGetMonitorWorkarea;
    public static d_glfwCreateWindow glfwCreateWindow;
    public static d_glfwDestroyWindow glfwDestroyWindow;
    public static d_glfwMakeContextCurrent glfwMakeContextCurrent;
    public static d_glfwGetCurrentContext glfwGetCurrentContext;
    public static d_glfwSwapBuffers glfwSwapBuffers;
    public static d_glfwSwapInterval glfwSwapInterval;
    public static d_glfwPollEvents glfwPollEvents;
    public static d_glfwWindowShouldClose glfwWindowShouldClose;
    public static d_glfwSetWindowShouldClose glfwSetWindowShouldClose;
    public static d_glfwGetProcAddress glfwGetProcAddress;
    public static d_glfwGetCocoaWindow glfwGetCocoaWindow;
    public static d_glfwGetKey glfwGetKey;
    public static d_glfwGetCursorPos glfwGetCursorPos;
    public static d_glfwSetInputMode glfwSetInputMode;
    public static d_glfwGetTime glfwGetTime;
    public static d_glfwSetKeyCallback glfwSetKeyCallback;
    public static d_glfwSetMouseButtonCallback glfwSetMouseButtonCallback;
    public static d_glfwSetCursorPosCallback glfwSetCursorPosCallback;
    public static d_glfwGetWindowContentScale glfwGetWindowContentScale;
#pragma warning restore CS8618

    private static IntPtr _library;
    private static int _loadCount;

    public static bool Loaded { get; private set; }

    public static void Load()
    {
        if (Loaded)
        {
            _loadCount++;
            return;
        }

        // Candidate native library names per OS; GLFW is not bundled and must be resolvable at runtime.
        string[] candidates =
            OperatingSystem.IsWindows() ? ["glfw3.dll", "glfw3"] :
            OperatingSystem.IsMacOS() ? ["libglfw.3.dylib", "libglfw.dylib"] :
                                         ["libglfw.so.3", "libglfw.so"];

        _library = NativeLibraryResolver.LoadFromBinOrSystem(candidates,
            "Could not locate the GLFW native library (glfw3.dll / libglfw.so.3 / libglfw.3.dylib). Install it or place it next to the executable.");

        glfwInit = NativeLibraryResolver.GetFunction<d_glfwInit>(_library, "glfwInit");
        glfwTerminate = NativeLibraryResolver.GetFunction<d_glfwTerminate>(_library, "glfwTerminate");
        glfwGetVersion = NativeLibraryResolver.GetFunction<d_glfwGetVersion>(_library, "glfwGetVersion");
        glfwWindowHint = NativeLibraryResolver.GetFunction<d_glfwWindowHint>(_library, "glfwWindowHint");
        glfwGetWindowSize = NativeLibraryResolver.GetFunction<d_glfwGetWindowSize>(_library, "glfwGetWindowSize");
        glfwGetFramebufferSize = NativeLibraryResolver.GetFunction<d_glfwGetFramebufferSize>(_library, "glfwGetFramebufferSize");
        glfwSetWindowSize = NativeLibraryResolver.GetFunction<d_glfwSetWindowSize>(_library, "glfwSetWindowSize");
        glfwSetWindowPos = NativeLibraryResolver.GetFunction<d_glfwSetWindowPos>(_library, "glfwSetWindowPos");
        glfwMaximizeWindow = NativeLibraryResolver.GetFunction<d_glfwMaximizeWindow>(_library, "glfwMaximizeWindow");
        glfwRestoreWindow = NativeLibraryResolver.GetFunction<d_glfwRestoreWindow>(_library, "glfwRestoreWindow");
        glfwGetPrimaryMonitor = NativeLibraryResolver.GetFunction<d_glfwGetPrimaryMonitor>(_library, "glfwGetPrimaryMonitor");
        glfwGetMonitorWorkarea = NativeLibraryResolver.GetFunction<d_glfwGetMonitorWorkarea>(_library, "glfwGetMonitorWorkarea");
        glfwCreateWindow = NativeLibraryResolver.GetFunction<d_glfwCreateWindow>(_library, "glfwCreateWindow");
        glfwDestroyWindow = NativeLibraryResolver.GetFunction<d_glfwDestroyWindow>(_library, "glfwDestroyWindow");
        glfwMakeContextCurrent = NativeLibraryResolver.GetFunction<d_glfwMakeContextCurrent>(_library, "glfwMakeContextCurrent");
        glfwGetCurrentContext = NativeLibraryResolver.GetFunction<d_glfwGetCurrentContext>(_library, "glfwGetCurrentContext");
        glfwSwapBuffers = NativeLibraryResolver.GetFunction<d_glfwSwapBuffers>(_library, "glfwSwapBuffers");
        glfwSwapInterval = NativeLibraryResolver.GetFunction<d_glfwSwapInterval>(_library, "glfwSwapInterval");
        glfwGetKey = NativeLibraryResolver.GetFunction<d_glfwGetKey>(_library, "glfwGetKey");
        glfwGetCursorPos = NativeLibraryResolver.GetFunction<d_glfwGetCursorPos>(_library, "glfwGetCursorPos");
        glfwSetInputMode = NativeLibraryResolver.GetFunction<d_glfwSetInputMode>(_library, "glfwSetInputMode");
        glfwGetTime = NativeLibraryResolver.GetFunction<d_glfwGetTime>(_library, "glfwGetTime");
        glfwPollEvents = NativeLibraryResolver.GetFunction<d_glfwPollEvents>(_library, "glfwPollEvents");
        glfwWindowShouldClose = NativeLibraryResolver.GetFunction<d_glfwWindowShouldClose>(_library, "glfwWindowShouldClose");
        glfwSetWindowShouldClose = NativeLibraryResolver.GetFunction<d_glfwSetWindowShouldClose>(_library, "glfwSetWindowShouldClose");
        glfwGetProcAddress = NativeLibraryResolver.GetFunction<d_glfwGetProcAddress>(_library, "glfwGetProcAddress");
        if (OperatingSystem.IsMacOS())
            glfwGetCocoaWindow = NativeLibraryResolver.GetFunction<d_glfwGetCocoaWindow>(_library, "glfwGetCocoaWindow");
        glfwSetKeyCallback = NativeLibraryResolver.GetFunction<d_glfwSetKeyCallback>(_library, "glfwSetKeyCallback");
        glfwSetMouseButtonCallback = NativeLibraryResolver.GetFunction<d_glfwSetMouseButtonCallback>(_library, "glfwSetMouseButtonCallback");
        glfwSetCursorPosCallback = NativeLibraryResolver.GetFunction<d_glfwSetCursorPosCallback>(_library, "glfwSetCursorPosCallback");
        glfwGetWindowContentScale = NativeLibraryResolver.GetFunction<d_glfwGetWindowContentScale>(_library, "glfwGetWindowContentScale");
        
        EnsureInitSucceeded(glfwInit());
        
        Loaded = true;
        _loadCount = 1;
    }

    public static void Terminate()
    {
        if (!Loaded) return;
        if (--_loadCount > 0) return;
        glfwTerminate();
        Loaded = false;
        _loadCount = 0;
    }

    internal static void EnsureInitSucceeded(int result)
    {
        if (result == GLFW_FALSE)
            throw new RenderException("Failed to initialize GLFW.");
    }

}
