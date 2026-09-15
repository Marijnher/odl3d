using System;
using System.Globalization;

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

        protected IntPtr GetRaw(string selectorName) =>
            objc_msgSend(Handle, GetSelector(selectorName));

        protected ObjCObject Get(string selectorName) =>
            new ObjCObject(objc_msgSend(Handle, GetSelector(selectorName)));

        protected T Get<T>(string selectorName) where T : ObjCObject =>
            Convert<T>(GetRaw(selectorName));

        protected ulong GetUInt64(string selectorName) =>
            objc_msgSendUInt64(Handle, GetSelector(selectorName));

        protected string GetString(string selectorName) =>
            Get<NSString>(selectorName).Value;

        protected string? GetStringOrNull(string selectorName)
        {
            IntPtr result = GetRaw(selectorName);
            if (result == IntPtr.Zero) return null;
            return new NSString(result).Value;
        }

        protected IntPtr Send(string selectorName, nuint value) =>
            objc_msgSendUInt64Arg(Handle, GetSelector(selectorName), value);

        protected IntPtr Send(string selectorName, nuint arg1, nuint arg2, nuint arg3) =>
            objc_msgSendDraw(Handle, GetSelector(selectorName), arg1, arg2, arg3);

        protected IntPtr Send(string selectorName, double red, double green, double blue, double alpha) =>
            objc_msgSendColor(Handle, GetSelector(selectorName), red, green, blue, alpha);

        protected static IntPtr SendRaw(IntPtr receiver, IntPtr selector, IntPtr arg) =>
            objc_msgSendArg(receiver, selector, arg);

        protected static IntPtr SendRaw(IntPtr receiver, IntPtr selector) =>
            objc_msgSend(receiver, selector);

        protected static IntPtr SendRaw(IntPtr receiver, string selectorName) =>
            objc_msgSend(receiver, GetSelector(selectorName));

        protected static IntPtr SendRaw(IntPtr receiver, string selectorName, IntPtr arg) =>
            objc_msgSendArg(receiver, GetSelector(selectorName), arg);

        protected IntPtr Send(string selectorName) =>
            objc_msgSend(Handle, GetSelector(selectorName));
        protected T Send<T>(string selectorName) where T : ObjCObject =>
            Convert<T>(Send(selectorName));

        protected IntPtr Send(string selectorName, IntPtr value) =>
            objc_msgSendArg(Handle, GetSelector(selectorName), value);
        protected T Send<T>(string selectorName, IntPtr value) where T : ObjCObject =>
            Convert<T>(Send(selectorName, value));

        protected IntPtr Send(string selectorName, ObjCObject obj) =>
            objc_msgSendArg(Handle, GetSelector(selectorName), obj.Handle);
        protected T Send<T>(string selectorName, ObjCObject obj) where T : ObjCObject =>
            Convert<T>(Send(selectorName, obj));

        protected IntPtr Send(string selectorName, string? value) =>
            objc_msgSendArg(Handle, GetSelector(selectorName), value == null ? IntPtr.Zero : NSString.Create(value).Handle);
        protected T Send<T>(string selectorName, string? value) where T : ObjCObject =>
            Convert<T>(Send(selectorName, value));

        protected T Convert<T>(IntPtr objc) where T : ObjCObject 
        {
            T? obj = (T?) Activator.CreateInstance(typeof(T), objc);
            if (obj is null) throw new RenderException("Could not convert object.");
            return obj;
        }
    }
}