using System;

namespace odl3d.Renderers;

public static partial class MetalNew
{
    public class ObjCObject
    {
        public IntPtr Handle { get; protected set; }

        protected ObjCObject(IntPtr handle)
        {
            Handle = handle == IntPtr.Zero
                ? throw new ArgumentException("The Objective-C object cannot be null.", nameof(handle))
                : handle;
        }

        protected IntPtr GetRaw(string selectorName) => Send(Handle, GetSelector(selectorName));

        protected ObjCObject Get(string selectorName) => new ObjCObject(GetRaw(selectorName));

        protected T Get<T>(string selectorName) where T : ObjCObject
        {
            IntPtr objcHandle = GetRaw(selectorName);
            T? obj = (T?) Activator.CreateInstance(typeof(T), objcHandle);
            if (obj is null) throw new RenderException("Could not convert object.");
            return obj;
        }

        protected ulong GetUInt64(string selectorName) =>
            objc_msgSendUInt64(Handle, GetSelector(selectorName));

        protected string GetString(string selectorName)
        {
            NSString nsString = Get<NSString>(selectorName);
            return nsString.Value;
        }

        protected string? GetStringOrNull(string selectorName)
        {
            IntPtr result = GetRaw(selectorName);
            if (result == IntPtr.Zero) return null;
            return new NSString(result).Value;
        }

        protected IntPtr Send(string selectorName, IntPtr value) =>
            Send(Handle, GetSelector(selectorName), value);

        protected IntPtr SendString(string selectorName, string? value) =>
            Send(selectorName, value == null ? IntPtr.Zero : NSString.Create(value).Handle);

        protected static IntPtr Send(IntPtr receiver, IntPtr selector) =>
            objc_msgSend(receiver, selector);

        protected static IntPtr Send(IntPtr receiver, string selectorName) =>
            Send(receiver, GetSelector(selectorName));

        protected static IntPtr Send(IntPtr receiver, IntPtr selector, IntPtr arg)
            => objc_msgSendArg(receiver, selector, arg);
            
        protected static IntPtr Send(IntPtr receiver, string selectorName, IntPtr arg) =>
            Send(receiver, GetSelector(selectorName), arg);
    }
}