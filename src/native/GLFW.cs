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
    public const int GLFW_RELEASE = 0;
    public const int GLFW_PRESS = 1;
    public const int GLFW_REPEAT = 2;
    public const int GLFW_CURSOR = 0x00033001;
    public const int GLFW_CURSOR_NORMAL = 0x00034001;
    public const int GLFW_CURSOR_DISABLED = 0x00034003;
    public const int GLFW_KEY_LAST = 348;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_glfwInit();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwTerminate();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwWindowHint(int hint, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwGetWindowSize(IntPtr window, out int width, out int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSetWindowSize(IntPtr window, int width, int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSetWindowPos(IntPtr window, int xpos, int ypos);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwMaximizeWindow(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwRestoreWindow(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr d_glfwGetPrimaryMonitor();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwGetMonitorWorkarea(IntPtr monitor, out int xpos, out int ypos, out int width, out int height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr d_glfwCreateWindow(int width, int height, [MarshalAs(UnmanagedType.LPStr)] string title, IntPtr monitor, IntPtr share);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwDestroyWindow(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwMakeContextCurrent(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSwapBuffers(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSwapInterval(int interval);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwPollEvents();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_glfwWindowShouldClose(IntPtr window);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSetWindowShouldClose(IntPtr window, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr d_glfwGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string procname);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_glfwGetKey(IntPtr window, int key);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwGetCursorPos(IntPtr window, out double xpos, out double ypos);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSetInputMode(IntPtr window, int mode, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate double d_glfwGetTime();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSetKeyCallback(IntPtr window, GLFWkeyfun? callback);
    public delegate void GLFWkeyfun(IntPtr window, int key, int scancode, int action, int mods);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSetMouseButtonCallback(IntPtr window, GLFWmousebuttonfun? callback);
    public delegate void GLFWmousebuttonfun(IntPtr window, int button, int action, int mods);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_glfwSetCursorPosCallback(IntPtr window, GLFWcursorposfun? callback);
    public delegate void GLFWcursorposfun(IntPtr window, double xpos, double ypos);

#pragma warning disable CS8618
    public static d_glfwInit glfwInit;
    public static d_glfwTerminate glfwTerminate;
    public static d_glfwWindowHint glfwWindowHint;
    public static d_glfwGetWindowSize glfwGetWindowSize;
    public static d_glfwSetWindowSize glfwSetWindowSize;
    public static d_glfwSetWindowPos glfwSetWindowPos;
    public static d_glfwMaximizeWindow glfwMaximizeWindow;
    public static d_glfwRestoreWindow glfwRestoreWindow;
    public static d_glfwGetPrimaryMonitor glfwGetPrimaryMonitor;
    public static d_glfwGetMonitorWorkarea glfwGetMonitorWorkarea;
    public static d_glfwCreateWindow glfwCreateWindow;
    public static d_glfwDestroyWindow glfwDestroyWindow;
    public static d_glfwMakeContextCurrent glfwMakeContextCurrent;
    public static d_glfwSwapBuffers glfwSwapBuffers;
    public static d_glfwSwapInterval glfwSwapInterval;
    public static d_glfwPollEvents glfwPollEvents;
    public static d_glfwWindowShouldClose glfwWindowShouldClose;
    public static d_glfwSetWindowShouldClose glfwSetWindowShouldClose;
    public static d_glfwGetProcAddress glfwGetProcAddress;
    public static d_glfwGetKey glfwGetKey;
    public static d_glfwGetCursorPos glfwGetCursorPos;
    public static d_glfwSetInputMode glfwSetInputMode;
    public static d_glfwGetTime glfwGetTime;
    public static d_glfwSetKeyCallback glfwSetKeyCallback;
    public static d_glfwSetMouseButtonCallback glfwSetMouseButtonCallback;
    public static d_glfwSetCursorPosCallback glfwSetCursorPosCallback;
#pragma warning restore CS8618

    private static IntPtr _library;

    public static bool Loaded { get; private set; }

    public static void Load()
    {
        if (Loaded) return;

        // Candidate native library names per OS; GLFW is not bundled and must be resolvable at runtime.
        string[] candidates =
            OperatingSystem.IsWindows() ? ["glfw3.dll", "glfw3"] :
            OperatingSystem.IsMacOS() ? ["libglfw.3.dylib", "libglfw.dylib"] :
                                         ["libglfw.so.3", "libglfw.so"];

        foreach (string candidate in candidates)
        {
            if (NativeLibrary.TryLoad(candidate, out _library)) break;
        }
        if (_library == IntPtr.Zero)
            throw new DllNotFoundException("Could not locate the GLFW native library (glfw3.dll / libglfw.so.3 / libglfw.3.dylib). Install it or place it next to the executable.");

        glfwInit = GetFunction<d_glfwInit>("glfwInit");
        glfwTerminate = GetFunction<d_glfwTerminate>("glfwTerminate");
        glfwWindowHint = GetFunction<d_glfwWindowHint>("glfwWindowHint");
        glfwGetWindowSize = GetFunction<d_glfwGetWindowSize>("glfwGetWindowSize");
        glfwSetWindowSize = GetFunction<d_glfwSetWindowSize>("glfwSetWindowSize");
        glfwSetWindowPos = GetFunction<d_glfwSetWindowPos>("glfwSetWindowPos");
        glfwMaximizeWindow = GetFunction<d_glfwMaximizeWindow>("glfwMaximizeWindow");
        glfwRestoreWindow = GetFunction<d_glfwRestoreWindow>("glfwRestoreWindow");
        glfwGetPrimaryMonitor = GetFunction<d_glfwGetPrimaryMonitor>("glfwGetPrimaryMonitor");
        glfwGetMonitorWorkarea = GetFunction<d_glfwGetMonitorWorkarea>("glfwGetMonitorWorkarea");
        glfwCreateWindow = GetFunction<d_glfwCreateWindow>("glfwCreateWindow");
        glfwDestroyWindow = GetFunction<d_glfwDestroyWindow>("glfwDestroyWindow");
        glfwMakeContextCurrent = GetFunction<d_glfwMakeContextCurrent>("glfwMakeContextCurrent");
        glfwSwapBuffers = GetFunction<d_glfwSwapBuffers>("glfwSwapBuffers");
        glfwSwapInterval = GetFunction<d_glfwSwapInterval>("glfwSwapInterval");
        glfwGetKey = GetFunction<d_glfwGetKey>("glfwGetKey");
        glfwGetCursorPos = GetFunction<d_glfwGetCursorPos>("glfwGetCursorPos");
        glfwSetInputMode = GetFunction<d_glfwSetInputMode>("glfwSetInputMode");
        glfwGetTime = GetFunction<d_glfwGetTime>("glfwGetTime");
        glfwPollEvents = GetFunction<d_glfwPollEvents>("glfwPollEvents");
        glfwWindowShouldClose = GetFunction<d_glfwWindowShouldClose>("glfwWindowShouldClose");
        glfwSetWindowShouldClose = GetFunction<d_glfwSetWindowShouldClose>("glfwSetWindowShouldClose");
        glfwGetProcAddress = GetFunction<d_glfwGetProcAddress>("glfwGetProcAddress");
        glfwSetKeyCallback = GetFunction<d_glfwSetKeyCallback>("glfwSetKeyCallback");
        glfwSetMouseButtonCallback = GetFunction<d_glfwSetMouseButtonCallback>("glfwSetMouseButtonCallback");
        glfwSetCursorPosCallback = GetFunction<d_glfwSetCursorPosCallback>("glfwSetCursorPosCallback");
        
        if (glfwInit() == GLFW_FALSE)
            throw new RenderException("Failed to initialize GLFW.");
            
        glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
        glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
        glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
        glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, GLFW_TRUE);
        
        Loaded = true;
    }

    private static TDelegate GetFunction<TDelegate>(string name) where TDelegate : Delegate
    {
        if (!NativeLibrary.TryGetExport(_library, name, out IntPtr ptr))
            throw new EntryPointNotFoundException($"Could not find GLFW function '{name}'.");
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
    }
}
