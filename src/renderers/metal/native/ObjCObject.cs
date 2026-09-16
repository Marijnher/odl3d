using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

public static partial class Metal
{
    public class ObjCObject : IDisposable
    {
        public IntPtr Handle { get; protected set; }
        private readonly bool ownsNativeObject;
        public bool Disposed { get; protected set; }

        public ObjCObject(IntPtr handle, bool ownsNativeObject = false)
        {
            Handle = handle == IntPtr.Zero
                ? throw new ArgumentException("The Objective-C object cannot be null.", nameof(handle))
                : handle;
            this.ownsNativeObject = ownsNativeObject;
        }

        public void Dispose()
        {
            if (!ownsNativeObject || Handle == IntPtr.Zero) return;
            Send("release");
            Handle = IntPtr.Zero;
            Disposed = true;
            GC.SuppressFinalize(this);
        }

        ~ObjCObject() => Dispose();

        protected static IntPtr Class(string name) => obcj_getClass(name);

        protected IntPtr GetRaw(string selectorName) =>
            objc_msgSend(Handle, GetSelector(selectorName));

        protected ObjCObject Get(string selectorName) =>
            new ObjCObject(objc_msgSend(Handle, GetSelector(selectorName)));

        protected T Get<T>(string selectorName) where T : ObjCObject =>
            Convert<T>(GetRaw(selectorName));

        protected CGSize GetSize(string selectorName) =>
            objc_msgSendRetSize(Handle, GetSelector(selectorName));

        protected uint GetUInt32(string selectorName) =>
            objc_msgSendRetUInt32(Handle, GetSelector(selectorName));

        protected ulong GetUInt64(string selectorName) =>
            objc_msgSendRetUInt64(Handle, GetSelector(selectorName));

        protected double GetDouble(string selectorName) =>
            objc_msgSendRetDouble(Handle, GetSelector(selectorName));

        protected string GetString(string selectorName) =>
            Get<NSString>(selectorName).Value;

        protected NSRect GetRect(string selectorName) =>
            objc_msgSendRetNSRect(Handle, GetSelector(selectorName));

        protected string? GetStringOrNull(string selectorName)
        {
            IntPtr result = GetRaw(selectorName);
            if (result == IntPtr.Zero) return null;
            return new NSString(result).Value;
        }

        protected static IntPtr SendRaw(IntPtr receiver, IntPtr selector, IntPtr arg) =>
            objc_msgSendPtr(receiver, selector, arg);

        protected static IntPtr SendRaw(IntPtr receiver, IntPtr selector) =>
            objc_msgSend(receiver, selector);

        protected static IntPtr SendRaw(IntPtr receiver, string selectorName) =>
            objc_msgSend(receiver, GetSelector(selectorName));

        protected static IntPtr SendRaw(IntPtr receiver, string selectorName, IntPtr arg) =>
            objc_msgSendPtr(receiver, GetSelector(selectorName), arg);

        protected static IntPtr SendRaw(IntPtr receiver, string selectorName, nuint arg1, nuint arg2, nuint arg3, byte byte1) =>
            objc_msgSendThreeUInt64Byte(receiver, GetSelector(selectorName), arg1, arg2, arg3, byte1);

        protected IntPtr Send(string selectorName) =>
            objc_msgSend(Handle, GetSelector(selectorName));
        protected T Send<T>(string selectorName) where T : ObjCObject =>
            Convert<T>(Send(selectorName));

        protected IntPtr Send(string selectorName, IntPtr value) =>
            objc_msgSendPtr(Handle, GetSelector(selectorName), value);
        protected T Send<T>(string selectorName, IntPtr value) where T : ObjCObject =>
            Convert<T>(Send(selectorName, value));

        protected IntPtr Send(string selectorName, nuint value) =>
            objc_msgSendUInt64(Handle, GetSelector(selectorName), value);
        protected T Send<T>(string selectorName, nuint value) where T : ObjCObject =>
            Convert<T>(Send(selectorName, value));

        protected IntPtr Send(string selectorName, nuint arg1, nuint arg2, nuint arg3) =>
            objc_msgSendThreeUInt64(Handle, GetSelector(selectorName), arg1, arg2, arg3);
        protected T Send<T>(string selectorName, nuint arg1, nuint arg2, nuint arg3) where T : ObjCObject =>
            Convert<T>(Send(selectorName, arg1, arg2, arg3));

        protected IntPtr Send(string selectorName, double red, double green, double blue, double alpha) =>
            objc_msgSendFourDoubles(Handle, GetSelector(selectorName), red, green, blue, alpha);
        protected T Send<T>(string selectorName, double red, double green, double blue, double alpha) where T : ObjCObject =>
            Convert<T>(Send(selectorName, red, green, blue, alpha));

        protected IntPtr Send(string selectorName, double value) =>
            objc_msgSendDouble(Handle, GetSelector(selectorName), value);
        protected T Send<T>(string selectorName, double value) where T : ObjCObject =>
            Convert<T>(Send(selectorName, value));

        protected IntPtr Send(string selectorName, ObjCObject obj) =>
            objc_msgSendPtr(Handle, GetSelector(selectorName), obj.Handle);
        protected T Send<T>(string selectorName, ObjCObject obj) where T : ObjCObject =>
            Convert<T>(Send(selectorName, obj));

        protected IntPtr Send(string selectorName, string? value) =>
            objc_msgSendPtr(Handle, GetSelector(selectorName), value == null ? IntPtr.Zero : NSString.Create(value).Handle);
        protected T Send<T>(string selectorName, string? value) where T : ObjCObject =>
            Convert<T>(Send(selectorName, value));

        protected IntPtr Send(string selectorName, IntPtr arg1, out IntPtr arg2) =>
            objc_msgSendPtrOutPtr(Handle, GetSelector(selectorName), arg1, out arg2);
        protected T Send<T>(string selectorName, IntPtr arg1, out IntPtr arg2) where T : ObjCObject =>
            Convert<T>(Send(selectorName, arg1, out arg2));

        protected IntPtr Send(string selectorName, IntPtr arg1, IntPtr arg2, out IntPtr arg3) =>
            objc_msgSendPtrPtrOutPtr(Handle, GetSelector(selectorName), arg1, arg2, out arg3);
        protected T Send<T>(string selectorName, IntPtr arg1, IntPtr arg2, out IntPtr arg3) where T : ObjCObject =>
            Convert<T>(Send(selectorName, arg1, arg2, out arg3));

        protected IntPtr Send(string selectorName, IntPtr arg1, nuint arg2, nuint arg3) =>
            objc_msgSendPtrUInt64UInt64(Handle, GetSelector(selectorName), arg1, arg2, arg3);
        protected T Send<T>(string selectorName, IntPtr arg1, nuint arg2, nuint arg3) where T : ObjCObject =>
            Convert<T>(Send(selectorName, arg1, arg2, arg3));

        protected IntPtr Send(string selectorName, IntPtr bytes, nuint bytesPerRow, nuint width, nuint height) =>
            objc_msgSendRegion(Handle, GetSelector(selectorName),
                new() { Width = width, Height = height, Depth = 1 }, 0, bytes, bytesPerRow);
        protected T Send<T>(string selectorName, IntPtr bytes, nuint bytesPerRow, nuint width, nuint height) where T : ObjCObject =>
            Convert<T>(Send(selectorName, bytes, bytesPerRow, width, height));

        protected IntPtr Send(string selectorName, nuint primitiveType, nuint indexCount, nuint indexType, IntPtr indexBuffer, nuint indexBufferOffset) =>
            objc_msgSendThreeUInt64PtrUInt64(Handle, GetSelector(selectorName), primitiveType, indexCount, indexType, indexBuffer, indexBufferOffset);
        protected T Send<T>(string selectorName, nuint primitiveType, nuint indexCount, nuint indexType, IntPtr indexBuffer, nuint indexBufferOffset) where T : ObjCObject =>
            Convert<T>(Send(selectorName, primitiveType, indexCount, indexType, indexBuffer, indexBufferOffset));

        protected IntPtr Send(string selectorName, IntPtr ptr1, nuint nuint1) =>
            objc_msgSendPtrUInt64(Handle, GetSelector(selectorName), ptr1, nuint1);
        protected T Send<T>(string selectorName, IntPtr ptr1, nuint nuint1) where T : ObjCObject =>
            Convert<T>(Send(selectorName, ptr1, nuint1));

        protected IntPtr Send(string selectorName, MTLViewport viewport) =>
            objc_msgSendViewport(Handle, GetSelector(selectorName), viewport);
        protected T Send<T>(string selectorName, MTLViewport viewport) where T : ObjCObject =>
            Convert<T>(Send(selectorName, viewport));

        protected IntPtr Send(string selectorName, MTLScissorRect scissorRect) =>
            objc_msgSendScissorRect(Handle, GetSelector(selectorName), scissorRect);
        protected T Send<T>(string selectorName, MTLScissorRect scissorRect) where T : ObjCObject =>
            Convert<T>(Send(selectorName, scissorRect));

        protected IntPtr Send(string selectorName, NSRect rect) =>
            objc_msgSendNSRect(Handle, GetSelector(selectorName), rect);
        protected T Send<T>(string selectorName, NSRect rect) where T : ObjCObject =>
            Convert<T>(Send(selectorName, rect));

        protected IntPtr Send(string selectorName, CGSize size) =>
            objc_msgSendSize(Handle, GetSelector(selectorName), size);
        protected T Send<T>(string selectorName, CGSize size) where T : ObjCObject =>
            Convert<T>(Send(selectorName, size));

        protected T Convert<T>(IntPtr objc) where T : ObjCObject 
        {
            T? obj = (T?) Activator.CreateInstance(
                typeof(T),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                args: [objc],
                culture: null);
            if (obj is null) throw new RenderException("Could not convert object.");
            return obj;
        }
    }
}