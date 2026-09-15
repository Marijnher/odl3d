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
    private delegate IntPtr d_objc_msgSendUInt64(IntPtr receiver, IntPtr selector, nuint value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendPtr(IntPtr receiver, IntPtr selector, IntPtr arg);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendPtrUInt64(IntPtr receiver, IntPtr selector, IntPtr ptr1, nuint value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendFourDoubles(IntPtr receiver, IntPtr selector,
        double red, double green, double blue, double alpha);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void d_objc_msgSendThreeUInt64(IntPtr receiver, IntPtr selector,
        nuint primitiveType, nuint vertexStart, nuint vertexCount);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendPtrOutPtr(IntPtr receiver, IntPtr selector,
        IntPtr source, out IntPtr error);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendPtrPtrOutPtr(IntPtr receiver, IntPtr selector,
        IntPtr source, IntPtr options, out IntPtr error);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendPtrUInt64UInt64(
        IntPtr receiver, IntPtr selector, IntPtr ptr, nuint nuint1, nuint nuint2);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendThreeUInt64PtrUInt64(IntPtr receiver, IntPtr selector,
        nuint primitiveType, nuint indexCount, nuint indexType, IntPtr indexBuffer, nuint indexBufferOffset);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_objc_msgSendThreeUInt64Byte(IntPtr receiver, IntPtr selector,
        nuint pixelFormat, nuint width, nuint height, byte mipmapped);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void d_objc_msgSendRegion(IntPtr receiver, IntPtr selector,
        Region region, nuint level, IntPtr bytes, nuint bytesPerRow);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void d_objc_msgSendPtrUInt64Void(IntPtr receiver, IntPtr selector,
        IntPtr value, nuint index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_MTLCreateSystemDefaultDevice();

#pragma warning disable CS8618
    private static d_sel_registerName sel_registerName;
    private static d_objc_getClass obcj_getClass;
    private static d_objc_msgSend objc_msgSend;
    private static d_objc_msgSendUInt64 objc_msgSendUInt64;
    private static d_objc_msgSendPtr objc_msgSendPtr;
    private static d_objc_msgSendPtrUInt64 objc_msgSendPtrUInt64;
    private static d_objc_msgSendFourDoubles objc_msgSendFourDoubles;
    private static d_objc_msgSendThreeUInt64 objc_msgSendThreeUInt64;
    private static d_objc_msgSendPtrOutPtr objc_msgSendPtrOutPtr;
    private static d_objc_msgSendPtrPtrOutPtr objc_msgSendPtrPtrOutPtr;
    private static d_objc_msgSendPtrUInt64UInt64 objc_msgSendPtrUInt64UInt64;
    private static d_objc_msgSendThreeUInt64PtrUInt64 objc_msgSendThreeUInt64PtrUInt64;
    private static d_objc_msgSendThreeUInt64Byte objc_msgSendThreeUInt64Byte;
    private static d_objc_msgSendRegion objc_msgSendRegion;
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
        objc_msgSendPtr = GetFunction<d_objc_msgSendPtr>(_objc, "objc_msgSend");
        objc_msgSendPtrUInt64 = GetFunction<d_objc_msgSendPtrUInt64>(_objc, "objc_msgSend");
        objc_msgSendFourDoubles = GetFunction<d_objc_msgSendFourDoubles>(_objc, "objc_msgSend");
        objc_msgSendThreeUInt64 = GetFunction<d_objc_msgSendThreeUInt64>(_objc, "objc_msgSend");
        objc_msgSendPtrOutPtr = GetFunction<d_objc_msgSendPtrOutPtr>(_objc, "objc_msgSend");
        objc_msgSendPtrPtrOutPtr = GetFunction<d_objc_msgSendPtrPtrOutPtr>(_objc, "objc_msgSend");
        objc_msgSendPtrUInt64UInt64 = GetFunction<d_objc_msgSendPtrUInt64UInt64>(_objc, "objc_msgSend");
        objc_msgSendThreeUInt64PtrUInt64 = GetFunction<d_objc_msgSendThreeUInt64PtrUInt64>(_objc, "objc_msgSend");
        objc_msgSendThreeUInt64Byte = GetFunction<d_objc_msgSendThreeUInt64Byte>(_objc, "objc_msgSend");
        objc_msgSendRegion = GetFunction<d_objc_msgSendRegion>(_objc, "objc_msgSend");
        MTLCreateSystemDefaultDevice = GetFunction<d_MTLCreateSystemDefaultDevice>(_metal, "MTLCreateSystemDefaultDevice");

        Loaded = true;
    }

    private static void ThrowIfError(IntPtr result, IntPtr error, string message)
    {
        if (result != IntPtr.Zero) return;
        if (error == IntPtr.Zero) throw new RenderException($"{message}: Unknown Metal exception.");
        NSException exc = new NSException(error);
        throw new RenderException($"{message}: {exc.LocalizedDescription}");
    }

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

    [StructLayout(LayoutKind.Sequential)]
    private struct Region
    {
        public nuint X, Y, Z, Width, Height, Depth;
    }
}
