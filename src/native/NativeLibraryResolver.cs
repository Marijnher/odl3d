using System;
using System.Runtime.InteropServices;

namespace odl3d;

internal static class NativeLibraryResolver
{
    internal static IntPtr LoadFromBinOrSystem(string[] candidates, string failureMessage)
    {
        foreach (string candidate in candidates)
        {
            if (NativeLibrary.TryLoad("bin/" + candidate, out IntPtr library)) return library;
        }

        foreach (string candidate in candidates)
        {
            if (NativeLibrary.TryLoad(candidate, out IntPtr library)) return library;
        }

        throw new DllNotFoundException(failureMessage);
    }

    internal static TDelegate GetFunction<TDelegate>(IntPtr library, string name) where TDelegate : Delegate
    {
        if (!NativeLibrary.TryGetExport(library, name, out IntPtr address))
            throw new EntryPointNotFoundException($"Could not find native function '{name}'.");
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
    }
}