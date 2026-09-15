using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace odl3d.Renderers;

/// <summary>
/// Metal backend bootstrap and presentation integration.
///
/// Metal is loaded dynamically so this assembly remains buildable and loadable on
/// Windows and Linux. The renderer creates a CAMetalLayer for GLFW's Cocoa window
/// and owns the current drawable until <see cref="Present"/> is called.
/// </summary>
public sealed class MetalOld : IRenderer
{
    private IntPtr device;
    private IntPtr commandQueue;
    private IntPtr layer;
    private IntPtr drawable;
    private IntPtr commandBuffer;
    private IntPtr encoder;
    private IntPtr depthTexture;
    private int depthWidth;
    private int depthHeight;
    private bool initialized;
    private bool windowAttached;
    private bool depthTestEnabled;
    private bool depthWriteEnabled = true;
    private bool alphaBlendingEnabled;
    private bool wireframeEnabled;
    private int viewportX;
    private int viewportY;
    private int viewportWidth;
    private int viewportHeight;
    private uint nextHandle = 1;
    private readonly Dictionary<uint, IntPtr> resources = new();
    private readonly Dictionary<uint, (BufferTarget Target, int Size)> buffers = new();
    private readonly Dictionary<uint, TextureState> textures = new();
    private readonly Dictionary<uint, IntPtr> samplers = new();
    private readonly Dictionary<uint, List<(int Index, int Size, int Stride, int Offset)>> vertexLayouts = new();
    private readonly Dictionary<uint, uint> vaoIndexBuffers = new();
    private readonly Dictionary<uint, uint> vaoVertexBuffers = new();
    private readonly Dictionary<uint, string> shaders = new();
    private readonly Dictionary<uint, ShaderType> shaderTypes = new();
    private readonly Dictionary<uint, List<uint>> programs = new();
    private readonly Dictionary<uint, (IntPtr Vertex, IntPtr Fragment, IntPtr Pipeline)> pipelines = new();
    private readonly Dictionary<int, string> uniforms = new();
    private readonly Dictionary<int, object> uniformValues = new();
    private readonly Dictionary<string, object> namedUniformValues = new(StringComparer.Ordinal);
    private uint boundVao;
    private uint boundArrayBuffer;
    private uint boundIndexBuffer;
    private uint boundTexture;
    private uint activeProgram;
    private int nextUniform = 1;

    public void ConfigureWindow()
    {
        GLFW.glfwWindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);
    }

    public void SetVSync(bool enabled)
    {
        if (layer != IntPtr.Zero)
            Send(layer, "setDisplaySyncEnabled:", enabled);
    }

    private sealed class TextureState
    {
        public TextureFilter MinFilter = TextureFilter.Nearest;
        public MipmapFilter Mipmap = MipmapFilter.None;
        public TextureFilter MagFilter = TextureFilter.Nearest;
        public TextureWrap WrapH = TextureWrap.Repeat;
        public TextureWrap WrapV = TextureWrap.Repeat;
        public AnisotropicFilter Anisotropic = AnisotropicFilter.None;
        public int Width;
        public int Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MetalUniforms
    {
        public Matrix4x4 Mvp;
        public Matrix4x4 Model;
        public Vector4 Color;
        public Vector4 TextureColor;
        public int UseTexture;
        public int Lit;
        public int FloatsPerVertex;
    }

    private static IntPtr msgSend;
    private static IntPtr selRegister;
    private static IntPtr objcGetClass;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr CreateDevice();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendObject(IntPtr receiver, IntPtr selector);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendObjectArg(IntPtr receiver, IntPtr selector, IntPtr value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendObjectResult(IntPtr receiver, IntPtr selector, IntPtr value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendBoolArg(IntPtr receiver, IntPtr selector, byte value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendUIntArg(IntPtr receiver, IntPtr selector, nuint value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendUIntResult(IntPtr receiver, IntPtr selector, nuint value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendClearColor(IntPtr receiver, IntPtr selector, double red, double green, double blue, double alpha);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendDoubleArg(IntPtr receiver, IntPtr selector, double value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendBufferCreate(IntPtr receiver, IntPtr selector, nuint length, nuint options);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendBytesCreate(IntPtr receiver, IntPtr selector, IntPtr bytes, nuint length, nuint options);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendSizeResult(IntPtr receiver, IntPtr selector, nuint value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nuint SendNoArgUInt(IntPtr receiver, IntPtr selector);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendSource(IntPtr receiver, IntPtr selector, IntPtr source, IntPtr options, out IntPtr error);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendPipeline(IntPtr receiver, IntPtr selector, IntPtr descriptor, out IntPtr error);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendBufferAt(IntPtr receiver, IntPtr selector, IntPtr buffer, nuint offset, nuint index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendBytesAt(IntPtr receiver, IntPtr selector, IntPtr bytes, nuint length, nuint index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendPipelineState(IntPtr receiver, IntPtr selector, IntPtr pipeline);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendIntArg(IntPtr receiver, IntPtr selector, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendIndexedDraw(IntPtr receiver, IntPtr selector, nuint primitiveType, nuint indexCount, nuint indexType, IntPtr indexBuffer, nuint indexBufferOffset);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendClassString(IntPtr receiver, IntPtr selector, IntPtr utf8);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendTextureDescriptor(IntPtr receiver, IntPtr selector, IntPtr descriptor);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendTexture2DDescriptor(IntPtr receiver, IntPtr selector, nuint pixelFormat, nuint width, nuint height, byte mipmapped);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendRegion(IntPtr receiver, IntPtr selector, Region region, nuint level, IntPtr bytes, nuint bytesPerRow);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendSamplerTexture(IntPtr receiver, IntPtr selector, IntPtr sampler, nuint index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendSamplerDescriptor(IntPtr receiver, IntPtr selector);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendRange(IntPtr receiver, IntPtr selector, nuint value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendFloatArg(IntPtr receiver, IntPtr selector, float value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendTextureAt(IntPtr receiver, IntPtr selector, IntPtr texture, nuint index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendViewport(IntPtr receiver, IntPtr selector, Viewport viewport);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SendSizeStruct(IntPtr receiver, IntPtr selector, Size size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SendDepthState(IntPtr receiver, IntPtr selector, IntPtr descriptor);

    [StructLayout(LayoutKind.Sequential)]
    private struct Viewport
    {
        public double OriginX;
        public double OriginY;
        public double Width;
        public double Height;
        public double ZNear;
        public double ZFar;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Size
    {
        public double Width;
        public double Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Region
    {
        // MTLRegion is composed of NSUInteger fields, which are 64-bit on macOS.
        // Using uint here shifts the following arguments in the objc_msgSend ABI.
        public nuint OriginX;
        public nuint OriginY;
        public nuint OriginZ;
        public nuint SizeWidth;
        public nuint SizeHeight;
        public nuint SizeDepth;
    }

    private static IntPtr Sel(string name)
    {
        IntPtr pointer = Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return Marshal.GetDelegateForFunctionPointer<SelRegister>(selRegister)(pointer);
        }
        finally
        {
            // sel_registerName copies the string synchronously.
            Marshal.FreeCoTaskMem(pointer);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SelRegister(IntPtr name);
    private static IntPtr Class(string name)
    {
        IntPtr pointer = Marshal.StringToCoTaskMemUTF8(name);
        try
        {
            return Marshal.GetDelegateForFunctionPointer<GetClass>(objcGetClass)(pointer);
        }
        finally
        {
            Marshal.FreeCoTaskMem(pointer);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetClass(IntPtr name);

    private static void InitializeObjectiveCRuntime()
    {
        IntPtr objc = NativeLibrary.Load("/usr/lib/libobjc.A.dylib");
        msgSend = NativeLibrary.GetExport(objc, "objc_msgSend");
        selRegister = NativeLibrary.GetExport(objc, "sel_registerName");
        objcGetClass = NativeLibrary.GetExport(objc, "objc_getClass");
    }

    private static IntPtr Send(IntPtr receiver, string selector) =>
        Marshal.GetDelegateForFunctionPointer<SendObject>(msgSend)(receiver, Sel(selector));

    private static void Send(IntPtr receiver, string selector, IntPtr value) =>
        Marshal.GetDelegateForFunctionPointer<SendObjectArg>(msgSend)(receiver, Sel(selector), value);

    private static void Send(IntPtr receiver, string selector, bool value) =>
        Marshal.GetDelegateForFunctionPointer<SendBoolArg>(msgSend)(receiver, Sel(selector), value ? (byte) 1 : (byte) 0);

    private static void Send(IntPtr receiver, string selector, nuint value) =>
        Marshal.GetDelegateForFunctionPointer<SendUIntArg>(msgSend)(receiver, Sel(selector), value);

    private static IntPtr SendResult(IntPtr receiver, string selector, nuint value) =>
        Marshal.GetDelegateForFunctionPointer<SendUIntResult>(msgSend)(receiver, Sel(selector), value);

    private static IntPtr SendObjectResultValue(IntPtr receiver, string selector, IntPtr value) =>
        Marshal.GetDelegateForFunctionPointer<SendObjectResult>(msgSend)(receiver, Sel(selector), value);

    private static IntPtr SendBuffer(IntPtr receiver, string selector, nuint length, nuint options) =>
        Marshal.GetDelegateForFunctionPointer<SendBufferCreate>(msgSend)(receiver, Sel(selector), length, options);

    private static IntPtr SendBytes(IntPtr receiver, string selector, IntPtr bytes, nuint length, nuint options) =>
        Marshal.GetDelegateForFunctionPointer<SendBytesCreate>(msgSend)(receiver, Sel(selector), bytes, length, options);

    private static IntPtr SendSize(IntPtr receiver, string selector, nuint value) =>
        Marshal.GetDelegateForFunctionPointer<SendSizeResult>(msgSend)(receiver, Sel(selector), value);

    private static nuint SendNoArgUIntValue(IntPtr receiver, string selector) =>
        Marshal.GetDelegateForFunctionPointer<SendNoArgUInt>(msgSend)(receiver, Sel(selector));

    private static void SendRangeValue(IntPtr receiver, string selector, nuint value) =>
        Marshal.GetDelegateForFunctionPointer<SendRange>(msgSend)(receiver, Sel(selector), value);

    private static void SendFloatValue(IntPtr receiver, string selector, float value) =>
        Marshal.GetDelegateForFunctionPointer<SendFloatArg>(msgSend)(receiver, Sel(selector), value);

    private static void SendDoubleValue(IntPtr receiver, string selector, double value) =>
        Marshal.GetDelegateForFunctionPointer<SendDoubleArg>(msgSend)(receiver, Sel(selector), value);

    private static void SendRegionValue(IntPtr receiver, string selector, Region region, nuint level, IntPtr bytes, nuint bytesPerRow) =>
        Marshal.GetDelegateForFunctionPointer<SendRegion>(msgSend)(receiver, Sel(selector), region, level, bytes, bytesPerRow);

    private static void SendSamplerTextureValue(IntPtr receiver, string selector, IntPtr sampler, nuint index) =>
        Marshal.GetDelegateForFunctionPointer<SendSamplerTexture>(msgSend)(receiver, Sel(selector), sampler, index);

    private static IntPtr NewTexture(IntPtr receiver, IntPtr descriptor) =>
        Marshal.GetDelegateForFunctionPointer<SendTextureDescriptor>(msgSend)(
            receiver, Sel("newTextureWithDescriptor:"), descriptor);

    private static IntPtr NewTexture2DDescriptor(IntPtr receiver, nuint pixelFormat, nuint width, nuint height, bool mipmapped)
    {
        // Build the descriptor through its properties instead of the convenience
        // class method. This makes the depth value explicit on all macOS/AGX
        // combinations and avoids creating a zero-depth texture.
        IntPtr descriptor = Send(receiver, "alloc");
        descriptor = Send(descriptor, "init");
        Send(descriptor, "setTextureType:", (nuint)2); // MTLTextureType2D
        Send(descriptor, "setPixelFormat:", pixelFormat);
        Send(descriptor, "setWidth:", width);
        Send(descriptor, "setHeight:", height);
        Send(descriptor, "setDepth:", (nuint)1);
        Send(descriptor, "setMipmapLevelCount:", (nuint)(mipmapped ? 1 : 1));
        return descriptor;
    }

    private static void SendTextureAtIndex(IntPtr receiver, string selector, IntPtr texture, nuint index) =>
        Marshal.GetDelegateForFunctionPointer<SendTextureAt>(msgSend)(receiver, Sel(selector), texture, index);

    private static void SendViewportValue(IntPtr receiver, string selector, Viewport viewport) =>
        Marshal.GetDelegateForFunctionPointer<SendViewport>(msgSend)(receiver, Sel(selector), viewport);

    private static void SendSizeValue(IntPtr receiver, string selector, Size size) =>
        Marshal.GetDelegateForFunctionPointer<SendSizeStruct>(msgSend)(receiver, Sel(selector), size);

    private static IntPtr SendSourceValue(IntPtr receiver, string selector, IntPtr source, IntPtr options, out IntPtr error) =>
        Marshal.GetDelegateForFunctionPointer<SendSource>(msgSend)(receiver, Sel(selector), source, options, out error);

    private static IntPtr SendPipelineValue(IntPtr receiver, string selector, IntPtr descriptor, out IntPtr error) =>
        Marshal.GetDelegateForFunctionPointer<SendPipeline>(msgSend)(receiver, Sel(selector), descriptor, out error);

    private static void SendBufferAtIndex(IntPtr receiver, string selector, IntPtr buffer, nuint offset, nuint index) =>
        Marshal.GetDelegateForFunctionPointer<SendBufferAt>(msgSend)(receiver, Sel(selector), buffer, offset, index);

    private static void SendBytesAtIndex(IntPtr receiver, string selector, IntPtr bytes, nuint length, nuint index) =>
        Marshal.GetDelegateForFunctionPointer<SendBytesAt>(msgSend)(receiver, Sel(selector), bytes, length, index);

    private static void SendPipelineStateValue(IntPtr receiver, string selector, IntPtr pipeline) =>
        Marshal.GetDelegateForFunctionPointer<SendPipelineState>(msgSend)(receiver, Sel(selector), pipeline);

    private static void SendInt(IntPtr receiver, string selector, int value) =>
        Marshal.GetDelegateForFunctionPointer<SendIntArg>(msgSend)(receiver, Sel(selector), value);

    private static void SendIndexed(IntPtr receiver, string selector, nuint primitiveType, nuint indexCount, nuint indexType, IntPtr indexBuffer, nuint indexBufferOffset) =>
        Marshal.GetDelegateForFunctionPointer<SendIndexedDraw>(msgSend)(receiver, Sel(selector), primitiveType, indexCount, indexType, indexBuffer, indexBufferOffset);

    private static IntPtr NewNSString(string value)
    {
        IntPtr bytes = Marshal.StringToCoTaskMemUTF8(value);
        try
        {
            return Marshal.GetDelegateForFunctionPointer<SendClassString>(msgSend)(
                Class("NSString"), Sel("stringWithUTF8String:"), bytes);
        }
        finally
        {
            Marshal.FreeCoTaskMem(bytes);
        }
    }

    private static string NativeString(IntPtr value)
    {
        if (value == IntPtr.Zero) return string.Empty;
        IntPtr utf8 = Send(value, "UTF8String");
        return utf8 == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(utf8) ?? string.Empty;
    }

    private uint NewHandle(IntPtr resource)
    {
        uint handle = nextHandle++;
        resources.Add(handle, resource);
        return handle;
    }

    private IntPtr Resource(uint handle)
    {
        if (handle == 0 || !resources.TryGetValue(handle, out IntPtr resource))
            throw new RenderException($"Metal resource handle {handle} is invalid.");
        return resource;
    }

    private void DeleteHandle(uint handle)
    {
        if (resources.Remove(handle, out IntPtr resource) && resource != IntPtr.Zero)
            Send(resource, "release");
    }

    private void RequireWindowAndFrame()
    {
        RequireWindow();
        if (encoder == IntPtr.Zero)
            throw new RenderException("Metal drawing requires an active frame. Call Window.Render before drawing.");
    }

    public void Initialize()
    {
        if (initialized) return;
        if (!OperatingSystem.IsMacOS())
            throw new RenderException("Metal is only available on macOS.");

        try
        {
            InitializeObjectiveCRuntime();
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            throw new RenderException($"The Objective-C runtime required by Metal could not be loaded: {exception.Message}");
        }

        IntPtr framework;
        if (!NativeLibrary.TryLoad("/System/Library/Frameworks/Metal.framework/Metal", out framework))
            throw new RenderException("Metal.framework could not be loaded.");

        if (!NativeLibrary.TryGetExport(framework, "MTLCreateSystemDefaultDevice", out IntPtr export))
            throw new RenderException("Metal.framework does not export MTLCreateSystemDefaultDevice.");

        device = Marshal.GetDelegateForFunctionPointer<CreateDevice>(export)();
        if (device == IntPtr.Zero)
            throw new RenderException("No compatible Metal device was found.");

        commandQueue = Send(device, "newCommandQueue");
        if (commandQueue == IntPtr.Zero)
            throw new RenderException("Metal could not create a command queue.");

        initialized = true;
    }

    /// <summary>Associates the renderer with a GLFW Cocoa window and creates its Metal layer.</summary>
    public void AttachWindow(IntPtr glfwWindow)
    {
        RequireInitialized();
        if (!OperatingSystem.IsMacOS())
            throw new RenderException("A Metal window is only available on macOS.");
        if (glfwWindow == IntPtr.Zero)
            throw new RenderException("Cannot attach Metal to a null GLFW window.");

        IntPtr cocoaWindow = GLFW.glfwGetCocoaWindow(glfwWindow);
        if (cocoaWindow == IntPtr.Zero)
            throw new RenderException("GLFW did not provide an NSWindow for the Metal renderer.");

        IntPtr contentView = Send(cocoaWindow, "contentView");
        layer = Send(Class("CAMetalLayer"), "layer");
        if (layer == IntPtr.Zero)
            throw new RenderException("Could not create CAMetalLayer.");

        Send(layer, "setDevice:", device);
        Send(layer, "setPixelFormat:", (nuint) 80); // MTLPixelFormatBGRA8Unorm
        Send(layer, "setDisplaySyncEnabled:", false);
        Send(layer, "setPresentsWithTransaction:", false);
        Send(layer, "setMaximumDrawableCount:", (nuint)3);
        Send(contentView, "setWantsLayer:", true);
        Send(contentView, "setLayer:", layer);
        windowAttached = true;
    }

    public void SetDrawableSize(int width, int height)
    {
        RequireWindow();
        if (width <= 0 || height <= 0) return;
        SendSizeValue(layer, "setDrawableSize:", new Size { Width = width, Height = height });
    }

    /// <summary>Begins a frame by acquiring the current drawable and clearing it.</summary>
    public void BeginFrame(Color color)
    {
        RequireWindow();
        if (encoder != IntPtr.Zero) return;

        drawable = Send(layer, "nextDrawable");
        if (drawable == IntPtr.Zero)
            throw new RenderException("Metal did not provide a drawable for the current frame.");

        commandBuffer = Send(commandQueue, "commandBuffer");
        IntPtr descriptor = Send(Class("MTLRenderPassDescriptor"), "renderPassDescriptor");
        IntPtr attachments = Send(descriptor, "colorAttachments");
        IntPtr attachment = SendResult(attachments, "objectAtIndexedSubscript:", 0);
        IntPtr texture = Send(drawable, "texture");
        Send(attachment, "setTexture:", texture);
        Send(attachment, "setLoadAction:", (nuint) 2); // MTLLoadActionClear
        Send(attachment, "setStoreAction:", (nuint) 1); // MTLStoreActionStore
        int drawableWidth = (int)SendNoArgUIntValue(texture, "width");
        int drawableHeight = (int)SendNoArgUIntValue(texture, "height");
        if (depthTexture == IntPtr.Zero || drawableWidth != depthWidth || drawableHeight != depthHeight)
        {
            if (depthTexture != IntPtr.Zero) Send(depthTexture, "release");
            IntPtr depthDescriptor = NewTexture2DDescriptor(Class("MTLTextureDescriptor"), 252,
                (nuint)Math.Max(1, drawableWidth), (nuint)Math.Max(1, drawableHeight), false);
            Send(depthDescriptor, "setUsage:", (nuint)4); // MTLTextureUsageRenderTarget
            depthTexture = NewTexture(device, depthDescriptor);
            Send(depthDescriptor, "release");
            depthWidth = drawableWidth;
            depthHeight = drawableHeight;
        }
        IntPtr depthAttachment = Send(descriptor, "depthAttachment");
        Send(depthAttachment, "setTexture:", depthTexture);
        Send(depthAttachment, "setLoadAction:", (nuint)2);
        Send(depthAttachment, "setStoreAction:", (nuint)0);
        SendDoubleValue(depthAttachment, "setClearDepth:", 1.0);
        Marshal.GetDelegateForFunctionPointer<SendClearColor>(msgSend)(
            attachment,
            Sel("setClearColor:"),
            color.R / 255d,
            color.G / 255d,
            color.B / 255d,
            color.A / 255d);
        encoder = SendObjectResultValue(commandBuffer, "renderCommandEncoderWithDescriptor:", descriptor);
        if (encoder == IntPtr.Zero)
            throw new RenderException("Metal could not create a render command encoder.");
        if (viewportWidth > 0 && viewportHeight > 0)
            SetViewport(viewportX, viewportY, viewportWidth, viewportHeight);
    }

    /// <summary>Ends the current command encoder, presents its drawable, and commits the command buffer.</summary>
    public void Present()
    {
        RequireWindow();
        if (encoder == IntPtr.Zero) return;
        Send(encoder, "endEncoding");
        Send(commandBuffer, "presentDrawable:", drawable);
        Send(commandBuffer, "commit");
        encoder = IntPtr.Zero;
        commandBuffer = IntPtr.Zero;
        drawable = IntPtr.Zero;
    }

    public void Present(IntPtr window) => Present();

    public void Dispose()
    {
        PresentIfPossible();
        if (depthTexture != IntPtr.Zero) Send(depthTexture, "release");
        foreach (IntPtr sampler in samplers.Values)
            if (sampler != IntPtr.Zero) Send(sampler, "release");
        foreach (uint handle in new List<uint>(resources.Keys))
            DeleteHandle(handle);
        foreach (var pipeline in pipelines.Values)
        {
            if (pipeline.Pipeline != IntPtr.Zero) Send(pipeline.Pipeline, "release");
            if (pipeline.Vertex != IntPtr.Zero) Send(pipeline.Vertex, "release");
            if (pipeline.Fragment != IntPtr.Zero) Send(pipeline.Fragment, "release");
        }
        pipelines.Clear();
        if (commandQueue != IntPtr.Zero) Send(commandQueue, "release");
        if (layer != IntPtr.Zero) Send(layer, "release");
        device = commandQueue = layer = IntPtr.Zero;
        depthTexture = IntPtr.Zero;
        depthWidth = depthHeight = 0;
        initialized = windowAttached = false;
    }

    private void PresentIfPossible()
    {
        if (windowAttached && encoder != IntPtr.Zero) Present();
    }

    private void RequireInitialized()
    {
        if (!initialized) throw new RenderException("Metal renderer has not been initialized.");
    }

    private void RequireWindow()
    {
        RequireInitialized();
        if (!windowAttached) throw new RenderException("Metal renderer is not attached to a GLFW window.");
    }

    public uint CreateVertexArray()
    {
        RequireInitialized();
        vertexLayouts[nextHandle] = new List<(int, int, int, int)>();
        return NewHandle(IntPtr.Zero);
    }

    public void DeleteVertexArray(VertexArray vao)
    {
        vertexLayouts.Remove(vao.Handle);
        vaoIndexBuffers.Remove(vao.Handle);
        vaoVertexBuffers.Remove(vao.Handle);
        DeleteHandle(vao.Handle);
    }

    public void BindVertexArray(VertexArray? vao)
    {
        if (vao == null)
        {
            if (boundVao != 0)
            {
                vaoIndexBuffers[boundVao] = boundIndexBuffer;
                vaoVertexBuffers[boundVao] = boundArrayBuffer;
            }
            boundVao = 0;
            return;
        }

        boundVao = vao.Handle;
        vaoIndexBuffers.TryGetValue(boundVao, out boundIndexBuffer);
        vaoVertexBuffers.TryGetValue(boundVao, out boundArrayBuffer);
    }

    public uint CreateBuffer()
    {
        RequireInitialized();
        return NewHandle(IntPtr.Zero);
    }

    public void DeleteBuffer(Buffer buffer)
    {
        buffers.Remove(buffer.Handle);
        DeleteHandle(buffer.Handle);
    }

    public void BindBuffer(BufferTarget target, uint buffer)
    {
        Resource(buffer);
        if (target == BufferTarget.ArrayBuffer) boundArrayBuffer = buffer;
        else if (target == BufferTarget.ElementBuffer) boundIndexBuffer = buffer;
        else throw new ArgumentOutOfRangeException(nameof(target));
        buffers[buffer] = (target, buffers.TryGetValue(buffer, out var old) ? old.Size : 0);
    }

    public void SetBufferData(BufferTarget target, uint buffer, float[] data, BufferHint hint)
    {
        byte[] bytes = new byte[data.Length * sizeof(float)];
        System.Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
        UploadBuffer(target, buffer, bytes);
    }

    public void SetBufferData(BufferTarget target, uint buffer, uint[] data, BufferHint hint)
    {
        byte[] bytes = new byte[data.Length * sizeof(uint)];
        System.Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
        UploadBuffer(target, buffer, bytes);
    }

    private void UploadBuffer(BufferTarget target, uint handle, byte[] data)
    {
        RequireInitialized();
        BindBuffer(target, handle);
        IntPtr old = Resource(handle);
        if (old != IntPtr.Zero) Send(old, "release");
        IntPtr buffer = SendBuffer(device, "newBufferWithLength:options:", (nuint)data.Length, 0);
        if (buffer == IntPtr.Zero) throw new RenderException("Metal could not allocate a buffer.");
        IntPtr contents = Send(buffer, "contents");
        Marshal.Copy(data, 0, contents, data.Length);
        resources[handle] = buffer;
        buffers[handle] = (target, data.Length);
    }

    public void EnableVertexAttribute(int index)
    {
        if (boundVao == 0) throw new RenderException("No Metal vertex array is bound.");
        if (!vertexLayouts.TryGetValue(boundVao, out _)) throw new RenderException("Invalid Metal vertex array.");
    }

    public void AddVertexAttribute(int index, int size, int stride, int offset)
    {
        if (boundVao == 0 || !vertexLayouts.TryGetValue(boundVao, out var layout))
            throw new RenderException("No Metal vertex array is bound.");
        layout.Add((index, size, stride, offset));
    }

    public uint CreateTexture()
    {
        RequireInitialized();
        uint handle = NewHandle(IntPtr.Zero);
        textures[handle] = new TextureState();
        return handle;
    }

    public void DeleteTexture(Texture texture)
    {
        if (samplers.Remove(texture.Handle, out IntPtr sampler) && sampler != IntPtr.Zero)
            Send(sampler, "release");
        textures.Remove(texture.Handle);
        DeleteHandle(texture.Handle);
        if (boundTexture == texture.Handle) boundTexture = 0;
    }
    public void BindTexture(Texture? texture) => boundTexture = texture?.Handle ?? 0;
    public void SetTextureMinFilter(TextureFilter filterMode, MipmapFilter mipmapFilter)
    {
        RequireTextureState(out TextureState state);
        state.MinFilter = filterMode;
        state.Mipmap = mipmapFilter;
        RefreshSampler();
    }
    public void SetTextureMagFilter(TextureFilter filterMode)
    {
        RequireTextureState(out TextureState state);
        state.MagFilter = filterMode;
        RefreshSampler();
    }
    public void SetTextureWrapModeH(TextureWrap wrapModeH)
    {
        RequireTextureState(out TextureState state);
        state.WrapH = wrapModeH;
        RefreshSampler();
    }
    public void SetTextureWrapModeV(TextureWrap wrapModeV)
    {
        RequireTextureState(out TextureState state);
        state.WrapV = wrapModeV;
        RefreshSampler();
    }
    public void SetTextureAnisotropicFilter(AnisotropicFilter anisotropicFilter)
    {
        RequireTextureState(out TextureState state);
        state.Anisotropic = anisotropicFilter;
        RefreshSampler();
    }

    public void UploadTexture(Texture texture)
    {
        RequireInitialized();
        if (boundTexture == 0) throw new RenderException("No Metal texture is bound.");
        if (!textures.TryGetValue(boundTexture, out TextureState? state))
            throw new RenderException("Invalid Metal texture handle.");

        IntPtr descriptor = NewTexture2DDescriptor(Class("MTLTextureDescriptor"), 70,
            (nuint)texture.Width, (nuint)texture.Height, state.Mipmap != MipmapFilter.None);
        SendSize(descriptor, "setWidth:", (nuint)texture.Width);
        SendSize(descriptor, "setHeight:", (nuint)texture.Height);
        Send(descriptor, "setMipmapLevelCount:", (nuint)(state.Mipmap == MipmapFilter.None ? 1 : MipmapLevels(texture.Width, texture.Height)));
        Send(descriptor, "setUsage:", (nuint)1); // MTLTextureUsageShaderRead
        IntPtr metalTexture = NewTexture(device, descriptor);
        Send(descriptor, "release");
        if (metalTexture == IntPtr.Zero)
            throw new RenderException("Metal could not allocate the texture.");

        if (Resource(boundTexture) != IntPtr.Zero)
            Send(Resource(boundTexture), "release");
        resources[boundTexture] = metalTexture;
        state.Width = texture.Width;
        state.Height = texture.Height;
        IntPtr pixels = Marshal.AllocHGlobal(texture.Pixels.Length);
        try
        {
            Marshal.Copy(texture.Pixels, 0, pixels, texture.Pixels.Length);
            SendRegionValue(metalTexture, "replaceRegion:mipmapLevel:withBytes:bytesPerRow:",
                new Region { SizeWidth = (nuint)texture.Width, SizeHeight = (nuint)texture.Height, SizeDepth = 1 },
                0, pixels, (nuint)(texture.Width * 4));
        }
        finally
        {
            Marshal.FreeHGlobal(pixels);
        }
        RefreshSampler();
    }

    public void GenerateMipmaps()
    {
        RequireTextureState(out TextureState state);
        if (state.Mipmap == MipmapFilter.None || boundTexture == 0) return;
        IntPtr texture = Resource(boundTexture);
        IntPtr buffer = Send(commandQueue, "commandBuffer");
        IntPtr blit = Send(buffer, "blitCommandEncoder");
        Send(blit, "generateMipmapsForTexture:", texture);
        Send(blit, "endEncoding");
        Send(buffer, "commit");
        Send(buffer, "waitUntilCompleted");
    }

    private void RequireTextureState(out TextureState state)
    {
        RequireInitialized();
        if (boundTexture == 0 || !textures.TryGetValue(boundTexture, out state!))
            throw new RenderException("No valid Metal texture is bound.");
    }

    private void RefreshSampler()
    {
        if (boundTexture == 0 || !textures.TryGetValue(boundTexture, out TextureState? state) ||
            Resource(boundTexture) == IntPtr.Zero)
            return;
        if (samplers.Remove(boundTexture, out IntPtr oldSampler) && oldSampler != IntPtr.Zero)
            Send(oldSampler, "release");

        IntPtr descriptor = Send(Class("MTLSamplerDescriptor"), "new");
        Send(descriptor, "setMinFilter:", (nuint)(state.MinFilter == TextureFilter.Linear ? 1 : 0));
        Send(descriptor, "setMagFilter:", (nuint)(state.MagFilter == TextureFilter.Linear ? 1 : 0));
        Send(descriptor, "setMipFilter:", (nuint)state.Mipmap);
        Send(descriptor, "setSAddressMode:", (nuint)AddressMode(state.WrapH));
        Send(descriptor, "setTAddressMode:", (nuint)AddressMode(state.WrapV));
        SendFloatValue(descriptor, "setMaxAnisotropy:", Math.Max(1, (int)state.Anisotropic));
        IntPtr sampler = SendObjectResultValue(device, "newSamplerStateWithDescriptor:", descriptor);
        Send(descriptor, "release");
        if (sampler == IntPtr.Zero)
            throw new RenderException("Metal could not create a texture sampler.");
        samplers[boundTexture] = sampler;
    }

    private static int AddressMode(TextureWrap wrap) => wrap switch
    {
        TextureWrap.Repeat => 2,
        TextureWrap.Mirror => 3,
        TextureWrap.Clamp => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(wrap))
    };

    private static int MipmapLevels(int width, int height)
    {
        int levels = 1;
        while (width > 1 || height > 1)
        {
            width = Math.Max(1, width / 2);
            height = Math.Max(1, height / 2);
            levels++;
        }
        return levels;
    }

    public uint CreateShader(ShaderType shaderType)
    {
        RequireInitialized();
        uint handle = NewHandle(IntPtr.Zero);
        shaderTypes[handle] = shaderType;
        return handle;
    }

    public void DeleteShader(Shader shader)
    {
        shaders.Remove(shader.Handle);
        shaderTypes.Remove(shader.Handle);
        DeleteHandle(shader.Handle);
    }
    public void SetShaderSource(Shader shader, string source) => shaders[shader.Handle] = source;
    public bool CompileShader(Shader shader)
    {
        if (!shaders.TryGetValue(shader.Handle, out string? source) || string.IsNullOrWhiteSpace(source))
            return false;
        return source.Contains("vertex_main", StringComparison.Ordinal) ||
               source.Contains("fragment_main", StringComparison.Ordinal);
    }

    public string GetShaderLog(Shader shader) => shaders.ContainsKey(shader.Handle)
        ? "Metal shader source is compiled when the program is linked."
        : "No source was supplied.";

    public uint CreateShaderProgram()
    {
        RequireInitialized();
        uint handle = NewHandle(IntPtr.Zero);
        programs[handle] = new List<uint>();
        return handle;
    }

    public void DeleteShaderProgram(ShaderProgram program)
    {
        if (pipelines.Remove(program.Handle, out var pipeline))
        {
            if (pipeline.Pipeline != IntPtr.Zero) Send(pipeline.Pipeline, "release");
            if (pipeline.Vertex != IntPtr.Zero) Send(pipeline.Vertex, "release");
            if (pipeline.Fragment != IntPtr.Zero) Send(pipeline.Fragment, "release");
        }
        programs.Remove(program.Handle);
        DeleteHandle(program.Handle);
    }

    public void AttachShader(ShaderProgram program, Shader shader)
    {
        if (!programs.TryGetValue(program.Handle, out var attached))
            throw new RenderException("Invalid Metal shader program.");
        attached.Add(shader.Handle);
    }

    public bool LinkShaderProgram(ShaderProgram program)
    {
        if (!programs.TryGetValue(program.Handle, out var attached) || attached.Count < 2)
            return false;
        uint vertexHandle = attached.Find(handle => shaderTypes.GetValueOrDefault(handle) == ShaderType.Vertex);
        uint fragmentHandle = attached.Find(handle => shaderTypes.GetValueOrDefault(handle) == ShaderType.Fragment);
        if (vertexHandle == 0 || fragmentHandle == 0 ||
            !shaders.TryGetValue(vertexHandle, out string? vertexSource) ||
            !shaders.TryGetValue(fragmentHandle, out string? fragmentSource))
            return false;

        IntPtr library = SendSourceValue(device, "newLibraryWithSource:options:error:", NewNSString(vertexSource), IntPtr.Zero, out IntPtr libraryError);
        if (library == IntPtr.Zero)
            throw new ShaderException($"Metal vertex library compilation failed: {NativeString(Send(libraryError, "localizedDescription"))}");
        IntPtr fragmentLibrary = SendSourceValue(device, "newLibraryWithSource:options:error:", NewNSString(fragmentSource), IntPtr.Zero, out IntPtr fragmentError);
        if (fragmentLibrary == IntPtr.Zero)
            throw new ShaderException($"Metal fragment library compilation failed: {NativeString(Send(fragmentError, "localizedDescription"))}");

        IntPtr vertexFunction = SendObjectResultValue(library, "newFunctionWithName:", NewNSString("vertex_main"));
        IntPtr fragmentFunction = SendObjectResultValue(fragmentLibrary, "newFunctionWithName:", NewNSString("fragment_main"));
        IntPtr descriptor = Send(Class("MTLRenderPipelineDescriptor"), "alloc");
        descriptor = Send(descriptor, "init");
        Send(descriptor, "setVertexFunction:", vertexFunction);
        Send(descriptor, "setFragmentFunction:", fragmentFunction);
        IntPtr colors = Send(descriptor, "colorAttachments");
        IntPtr color = SendResult(colors, "objectAtIndexedSubscript:", 0);
        Send(color, "setPixelFormat:", (nuint)80); // MTLPixelFormatBGRA8Unorm
        Send(descriptor, "setDepthAttachmentPixelFormat:", (nuint)252); // MTLPixelFormatDepth32Float
        Send(color, "setBlendingEnabled:", alphaBlendingEnabled);
        if (alphaBlendingEnabled)
        {
            Send(color, "setRgbBlendOperation:", (nuint)0); // add
            Send(color, "setAlphaBlendOperation:", (nuint)0); // add
            Send(color, "setSourceRGBBlendFactor:", (nuint)4); // source alpha
            Send(color, "setDestinationRGBBlendFactor:", (nuint)5); // one minus source alpha
            Send(color, "setSourceAlphaBlendFactor:", (nuint)1); // one
            Send(color, "setDestinationAlphaBlendFactor:", (nuint)5); // one minus source alpha
        }

        IntPtr pipeline = SendPipelineValue(device, "newRenderPipelineStateWithDescriptor:error:", descriptor, out IntPtr pipelineError);
        if (pipeline == IntPtr.Zero)
            throw new ShaderException($"Metal pipeline creation failed: {NativeString(Send(pipelineError, "localizedDescription"))}");
        pipelines[program.Handle] = (vertexFunction, fragmentFunction, pipeline);
        Send(library, "release");
        Send(fragmentLibrary, "release");
        Send(descriptor, "release");
        return true;
    }

    public string GetShaderProgramLog(ShaderProgram program) => pipelines.ContainsKey(program.Handle)
        ? string.Empty
        : "Metal pipeline has not been created.";

    public void UseShaderProgram(ShaderProgram program)
    {
        if (!pipelines.ContainsKey(program.Handle))
            throw new RenderException("The Metal shader program is not linked.");
        activeProgram = program.Handle;
    }

    public int GetUniformLocation(ShaderProgram program, string name)
    {
        if (activeProgram != program.Handle && !programs.ContainsKey(program.Handle))
            throw new RenderException("Invalid Metal shader program.");
        int location = nextUniform++;
        uniforms[location] = name;
        return location;
    }

    public void SetUniformMatrix(int location, Matrix4x4 data)
    {
        uniformValues[location] = data;
        if (uniforms.TryGetValue(location, out string? name)) namedUniformValues[name] = data;
    }

    public void SetUniformInt(int location, int data)
    {
        uniformValues[location] = data;
        if (uniforms.TryGetValue(location, out string? name)) namedUniformValues[name] = data;
    }

    public void SetUniformColor(int location, Color color)
    {
        uniformValues[location] = color;
        if (uniforms.TryGetValue(location, out string? name)) namedUniformValues[name] = color;
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        RequireWindow();
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        viewportX = x;
        viewportY = y;
        viewportWidth = width;
        viewportHeight = height;
        if (encoder != IntPtr.Zero)
            SendViewportValue(encoder, "setViewport:", new Viewport
            {
                OriginX = x, OriginY = y, Width = width, Height = height, ZNear = 0, ZFar = 1
            });
    }

    public void SetEnableDepthTest(bool enable)
    {
        RequireWindow();
        depthTestEnabled = enable;
    }

    public void SetDepthMask(bool enable)
    {
        RequireWindow();
        depthWriteEnabled = enable;
    }

    public void SetAlphaBlending(bool enable)
    {
        RequireWindow();
        alphaBlendingEnabled = enable;
    }

    public void SetWireFrame(bool enable)
    {
        RequireWindow();
        wireframeEnabled = enable;
    }
    public void ClearColor(Color color) => BeginFrame(color);
    public void ClearColorBuffer() { }
    public void ClearDepthBuffer() => RequireWindow();

    public void DrawElements(int count)
    {
        RequireWindowAndFrame();
        if (activeProgram == 0 || boundVao == 0 || boundIndexBuffer == 0)
            throw new RenderException("Metal draw requires a shader program, vertex array, and index buffer.");
        if (!buffers.TryGetValue(boundIndexBuffer, out var index) || index.Size < count * sizeof(uint))
            throw new RenderException("Metal index buffer is smaller than the requested draw count.");
        if (!pipelines.TryGetValue(activeProgram, out var pipeline))
            throw new RenderException("Metal shader pipeline is not available.");

        SendPipelineStateValue(encoder, "setRenderPipelineState:", pipeline.Pipeline);
        Send(encoder, "setTriangleFillMode:", (nuint)(wireframeEnabled ? 1 : 0));
        IntPtr depthDescriptor = Send(Class("MTLDepthStencilDescriptor"), "new");
        Send(depthDescriptor, "setDepthCompareFunction:", (nuint)(depthTestEnabled ? 1 : 0));
        Send(depthDescriptor, "setDepthWriteEnabled:", depthWriteEnabled);
        IntPtr depthState = SendObjectResultValue(device, "newDepthStencilStateWithDescriptor:", depthDescriptor);
        Send(depthDescriptor, "release");
        if (depthState == IntPtr.Zero)
            throw new RenderException("Metal could not create a depth-stencil state.");
        Send(encoder, "setDepthStencilState:", depthState);
        Send(depthState, "release");
        SendBufferAtIndex(encoder, "setVertexBuffer:offset:atIndex:", Resource(boundArrayBuffer), 0, 0);

        MetalUniforms uniforms = new()
        {
            Mvp = GetUniform<Matrix4x4>("uMVP"),
            Model = GetUniform<Matrix4x4>("uModel"),
            Color = ToVector(GetUniformOrDefault("uColor", new Color(255, 255, 255, 255))),
            TextureColor = ToVector(GetUniformOrDefault("texColor", new Color(255, 255, 255, 255))),
            UseTexture = GetUniformOrDefault("uUseTexture", 0),
            Lit = GetUniformOrDefault("uLit", 0),
            FloatsPerVertex = GetVertexFloatsPerVertex()
        };
        IntPtr uniformMemory = Marshal.AllocHGlobal(Marshal.SizeOf<MetalUniforms>());
        try
        {
            Marshal.StructureToPtr(uniforms, uniformMemory, false);
            SendBytesAtIndex(encoder, "setVertexBytes:length:atIndex:", uniformMemory,
                (nuint)Marshal.SizeOf<MetalUniforms>(), 1);
            SendBytesAtIndex(encoder, "setFragmentBytes:length:atIndex:", uniformMemory,
                (nuint)Marshal.SizeOf<MetalUniforms>(), 1);
        }
        finally
        {
            Marshal.FreeHGlobal(uniformMemory);
        }

        if (GetUniformOrDefault("uUseTexture", 0) != 0)
        {
            if (boundTexture == 0 || Resource(boundTexture) == IntPtr.Zero ||
                !samplers.TryGetValue(boundTexture, out IntPtr sampler))
                throw new RenderException("A textured draw requires an uploaded texture and sampler.");
            SendTextureAtIndex(encoder, "setFragmentTexture:atIndex:", Resource(boundTexture), 0);
            SendSamplerTextureValue(encoder, "setFragmentSamplerState:atIndex:", sampler, 0);
        }

        SendIndexed(encoder, "drawIndexedPrimitives:indexCount:indexType:indexBuffer:indexBufferOffset:",
            3, (nuint)count, 1, Resource(boundIndexBuffer), 0);
    }

    private T GetUniform<T>(string name) where T : struct
    {
        if (namedUniformValues.TryGetValue(name, out object? value) && value is T typed)
            return typed;
        return default;
    }

    private int GetUniformOrDefault(string name, int fallback) =>
        namedUniformValues.TryGetValue(name, out object? value) && value is int integer ? integer : fallback;

    private Color GetUniformOrDefault(string name, Color fallback) =>
        namedUniformValues.TryGetValue(name, out object? value) && value is Color color ? color : fallback;

    private static Vector4 ToVector(Color color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    private int GetVertexFloatsPerVertex()
    {
        if (boundVao != 0 && vertexLayouts.TryGetValue(boundVao, out var layout))
            return layout.Count > 2 ? 8 : 5;
        return 5;
    }
}
