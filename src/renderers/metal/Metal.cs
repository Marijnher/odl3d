using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace odl3d.Renderers;

public static partial class Metal
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sel_registerName([MarshalAs(UnmanagedType.LPStr)] string name);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_getClass([MarshalAs(UnmanagedType.LPStr)] string name);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSend(IntPtr receiver, IntPtr selector);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ulong d_objc_msgSendUInt64(IntPtr receiver, IntPtr selector);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendUInt64Arg(IntPtr receiver, IntPtr selector, nuint value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendArg(IntPtr receiver, IntPtr selector, IntPtr arg);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendColor(IntPtr receiver, IntPtr selector,
        double red, double green, double blue, double alpha);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendDraw(IntPtr receiver, IntPtr selector,
        nuint primitiveType, nuint vertexStart, nuint vertexCount);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_MTLCreateSystemDefaultDevice();

#pragma warning disable CS8618
    private static d_sel_registerName sel_registerName;
    private static d_objc_getClass obcj_getClass;
    private static d_objc_msgSend objc_msgSend;
    private static d_objc_msgSendUInt64 objc_msgSendUInt64;
    private static d_objc_msgSendUInt64Arg objc_msgSendUInt64Arg;
    private static d_objc_msgSendArg objc_msgSendArg;
    private static d_objc_msgSendColor objc_msgSendColor;
    private static d_objc_msgSendDraw objc_msgSendDraw;
    private static d_MTLCreateSystemDefaultDevice MTLCreateSystemDefaultDevice;
#pragma warning restore CS8618

    private static IntPtr _objc;
    private static IntPtr _metal;
    private static readonly Dictionary<string, IntPtr> _selectors = new();

    public static bool Loaded { get; private set; }

    public static void Load()
    {
        if (Loaded) return;
        if (!OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("Metal is only available on macOS.");

        if (!NativeLibrary.TryLoad("/usr/lib/libobjc.A.dylib", out _objc))
            throw new DllNotFoundException("Could not load the Objective-C runtime.");
        if (!NativeLibrary.TryLoad("/System/Library/Frameworks/Metal.framework/Metal", out _metal))
            throw new DllNotFoundException("Could not load the Metal framework.");

        sel_registerName = GetFunction<d_sel_registerName>(_objc, "sel_registerName");
        obcj_getClass = GetFunction<d_objc_getClass>(_objc, "objc_getClass");
        objc_msgSend = GetFunction<d_objc_msgSend>(_objc, "objc_msgSend");
        objc_msgSendUInt64 = GetFunction<d_objc_msgSendUInt64>(_objc, "objc_msgSend");
        objc_msgSendUInt64Arg = GetFunction<d_objc_msgSendUInt64Arg>(_objc, "objc_msgSend");
        objc_msgSendArg = GetFunction<d_objc_msgSendArg>(_objc, "objc_msgSend");
        objc_msgSendColor = GetFunction<d_objc_msgSendColor>(_objc, "objc_msgSend");
        objc_msgSendDraw = GetFunction<d_objc_msgSendDraw>(_objc, "objc_msgSend");
        MTLCreateSystemDefaultDevice = GetFunction<d_MTLCreateSystemDefaultDevice>(_metal, "MTLCreateSystemDefaultDevice");

        Loaded = true;
    }

    internal static IntPtr Class(string name) => obcj_getClass(name);

    private static IntPtr GetSelector(string name)
    {
        if (!_selectors.TryGetValue(name, out IntPtr selector))
        {
            selector = sel_registerName(name);
            _selectors.Add(name, selector);
        }
        return selector;
    }

    private static TDelegate GetFunction<TDelegate>(IntPtr library, string name) where TDelegate : Delegate
    {
        if (!NativeLibrary.TryGetExport(library, name, out IntPtr ptr))
            throw new EntryPointNotFoundException($"Could not find native function '{name}'.");
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
    }

    public static Device GetDefaultDevice()
    {
        IntPtr device = MTLCreateSystemDefaultDevice();
        if (device == IntPtr.Zero)
            throw new InvalidOperationException("Metal did not provide a system default device.");
        return new Device(device);
    }
}
