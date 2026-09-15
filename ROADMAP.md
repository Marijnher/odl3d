You have a promising skeleton, but it is currently at the “type inventory” stage rather than a usable backend contract. The next work should focus first on making the generic layer internally coherent, then implementing one backend end-to-end—preferably Metal, because its explicit resource/pass model is driving the design.

## Current state

You now have:

- Resource interfaces:
  - `IGPUResource.cs`
  - `IBuffer.cs`
  - `ITexture.cs`
  - `IShaderModule.cs`
  - `IShaderPipeline.cs`
  - `IDepthStencilState.cs`
- Frame/surface/pass concepts:
  - `IRenderSurface.cs`
  - `IFrame.cs`
  - `IRenderPass.cs`
  - `IRenderPassDescription.cs`
- Device entry point:
  - `IRenderDevice.cs`
- Descriptions under `generic/types/`
- Capability enums under `generic/enums/`

The overall direction is correct. The immediate issue is that several interfaces and descriptions do not yet agree with one another.

---

# Phase 1: Fix the generic layer so it compiles cleanly

Do this before implementing any backend.

## 1. Fix namespace consistency

Everything currently uses:

```csharp
namespace odl3d.Renderer;
```

But the project’s existing code uses both `odl3d.Renderer` and `odl3d.Renderers`.

Pick one. I recommend:

```csharp
namespace odl3d.Renderer;
```

because the existing backend namespace is already `odl3d.Renderers`.

Alternatively, use:

```csharp
namespace odl3d.Rendering;
```

if you want to distinguish the generic API from concrete renderers.

Do not leave the generic contract in a namespace that differs only by singular/plural from the existing backend namespace without an intentional reason.

## 2. Fix current compile errors and typos

`VertexBufferLayoutDescription.cs` is missing a semicolon:

```csharp
public StepMode StepMode { get; init; } = StepMode.PerVertex;
```

`TextureUsage.cs` has:

```csharp
CopyDestionation
```

Rename it to:

```csharp
CopyDestination
```

Also, `TextureUsage.None` should be zero:

```csharp
None = 0
```

Current values start at `0x01`, which makes every usage technically include `None`.

`TextureFormat.cs` contains:

```csharp
RGBA16Flat
```

This should almost certainly be:

```csharp
RGBA16Float
```

## 3. Fix enum ownership

You already have existing texture enums in `src/textures/`:

- `TextureFilter`
- `MipmapFilter`
- `TextureWrap`
- `AnisotropicFilter`

Those are conceptually generic renderer enums, not merely CPU texture types. Move or re-export them into the generic rendering namespace so the new API does not depend on legacy texture ownership.

You should eventually have one canonical definition of each enum.

---

# Phase 2: Complete the missing resource contracts

## 4. Add `ISampler`

You have sampler implementation work in Metal, but no generic sampler contract yet.

Create:

```csharp
public interface ISampler : IGPUResource
{
}
```

A sampler does not need public properties unless you want introspection. Its configuration belongs in `SamplerDescription`.

Add:

```csharp
public sealed record SamplerDescription
{
    public TextureFilter MinFilter { get; init; } =
        TextureFilter.Nearest;

    public TextureFilter MagFilter { get; init; } =
        TextureFilter.Nearest;

    public MipmapFilter MipmapFilter { get; init; } =
        MipmapFilter.None;

    public TextureWrap AddressU { get; init; } =
        TextureWrap.Repeat;

    public TextureWrap AddressV { get; init; } =
        TextureWrap.Repeat;

    public TextureWrap AddressW { get; init; } =
        TextureWrap.Repeat;

    public AnisotropicFilter Anisotropy { get; init; } =
        AnisotropicFilter.None;

    public CompareFunction? Comparison { get; init; }
}
```

The existing Metal `SamplerState.cs` maps directly to this.

## 5. Expand `IRenderDevice`

Current `IRenderDevice.cs` is incomplete:

```csharp
public interface IRenderDevice : IGPUResource
{
    IRenderCapabilities Capabilities { get; }

    IRenderSurface CreateSurface(nint nativeWindow);

    IBuffer CreateBuffer(...);
    ITexture CreateTexture(...);
    IShaderModule CreateShaderModule(...);
    IShaderPipeline CreateShaderPipeline(...);
}
```

Recommended version:

```csharp
public interface IRenderDevice : IDisposable
{
    IRenderCapabilities Capabilities { get; }

    IRenderSurface CreateSurface(nint nativeWindow);

    IBuffer CreateBuffer(
        BufferDescription description,
        ReadOnlySpan<byte> initialData = default);

    ITexture CreateTexture(
        TextureDescription description,
        ReadOnlySpan<byte> initialData = default);

    ISampler CreateSampler(
        SamplerDescription description);

    IShaderModule CreateShaderModule(
        ShaderModuleDescription description);

    IShaderPipeline CreateShaderPipeline(
        ShaderPipelineDescription description);

    IDepthStencilState CreateDepthStencilState(
        DepthStencilDescription description);

    void UpdateBuffer(
        IBuffer buffer,
        ReadOnlySpan<byte> data,
        nuint destinationOffset = 0);

    void UploadTexture(
        ITexture texture,
        ReadOnlySpan<byte> data,
        int mipLevel = 0);

    void GenerateMipmaps(ITexture texture);
}
```

`IRenderDevice` should probably inherit `IDisposable` directly rather than `IGPUResource`. A device is not an ordinary GPU resource.

You can still make `IGPUResource` the base for buffers, textures, samplers, pipelines, and states.

## 6. Complete `ITexture`

Current `ITexture.cs` is acceptable as a minimum, but add usage and sample count:

```csharp
public interface ITexture : IGPUResource
{
    int Width { get; }
    int Height { get; }
    int Depth { get; }

    TextureFormat Format { get; }
    TextureUsage Usage { get; }

    int MipmapLevels { get; }
    int SampleCount { get; }
}
```

This matters because a swapchain texture, depth texture, sampled texture, and multisampled render target are all different use cases.

## 7. Improve `IBuffer`

Current `IBuffer.cs` is good but should expose access too:

```csharp
public interface IBuffer : IGPUResource
{
    nuint Size { get; }
    BufferUsage Usage { get; }
    BufferAccess Access { get; }
}
```

Do not put “vertex” or “index” into the native resource type. A Metal buffer can be used for either; the draw binding determines its role.

---

# Phase 3: Fix render-pass lifetime and command semantics

## 8. `IRenderPassDescription` should not be a GPU resource

Current `IRenderPassDescription.cs` is:

```csharp
public interface IRenderPassDescription : IGPUResource
{
}
```

That is the wrong ownership model.

A description is configuration data. It should be a record:

```csharp
public sealed record RenderPassDescription
{
    public required ColorAttachmentDescription Color { get; init; }
    public DepthAttachmentDescription? Depth { get; init; }
}
```

Remove `IRenderPassDescription` entirely unless you expect multiple polymorphic pass-description implementations.

If you retain the interface:

```csharp
public interface IRenderPassDescription
{
}
```

It should not inherit `IDisposable` or `IGPUResource`.

## 9. Expand `RenderPassDescription`

Current `RenderPassDescription.cs` only has:

```csharp
public required Color ClearColor { get; init; }
public bool ClearDepth { get; init; }
```

This is too tied to one color target and does not express load/store behavior.

Recommended:

```csharp
public sealed record RenderPassDescription
{
    public required ColorAttachmentDescription Color { get; init; }

    public DepthAttachmentDescription? Depth { get; init; }

    public IReadOnlyList<ColorAttachmentDescription>? AdditionalColors { get; init; }
}
```

```csharp
public sealed record ColorAttachmentDescription
{
    public LoadAction LoadAction { get; init; } =
        LoadAction.Clear;

    public StoreAction StoreAction { get; init; } =
        StoreAction.Store;

    public Color ClearColor { get; init; }
}
```

```csharp
public sealed record DepthAttachmentDescription
{
    public LoadAction LoadAction { get; init; } =
        LoadAction.Clear;

    public StoreAction StoreAction { get; init; } =
        StoreAction.DontCare;

    public float ClearDepth { get; init; } = 1.0f;
}
```

Add:

```csharp
public enum LoadAction
{
    DontCare,
    Load,
    Clear
}

public enum StoreAction
{
    DontCare,
    Store
}
```

This maps directly to Metal’s `MTLRenderPassDescriptor`, especially `RenderPassDepthAttachment.cs`.

## 10. Fix `IFrame`

Current `IFrame.cs` has:

```csharp
IRenderPass CreateRenderPass(IRenderPassDescription renderPassDescription);
void Present();
```

Change it to:

```csharp
public interface IFrame : IDisposable
{
    ITexture ColorTexture { get; }

    IRenderPass BeginRenderPass(
        RenderPassDescription description);

    void Present();
}
```

I would not make `IFrame` an `IGPUResource`. It represents an acquired frame/drawable and has frame-scoped lifetime semantics.

For Metal:

- `AcquireFrame()` obtains a drawable.
- `BeginRenderPass()` creates a command buffer and encoder.
- `Present()` presents the drawable and commits the command buffer.
- Disposing without presenting should safely abandon or finish the frame.

You should document whether `Present()` is mandatory.

## 11. Fix `IRenderPass`

Current `IRenderPass.cs` is close but missing sampler binding and explicit end semantics.

Recommended:

```csharp
public interface IRenderPass : IDisposable
{
    void SetViewport(Viewport viewport);
    void SetScissor(ScissorRect scissor);

    void SetPipeline(IShaderPipeline pipeline);
    void SetDepthStencilState(IDepthStencilState state);

    void SetVertexBuffer(
        int slot,
        IBuffer buffer,
        nuint offset = 0);

    void SetIndexBuffer(
        IBuffer buffer,
        IndexFormat format,
        nuint offset = 0);

    void SetUniformBuffer(
        int slot,
        IBuffer buffer,
        nuint offset = 0);

    void SetTexture(
        int slot,
        ITexture texture);

    void SetSampler(
        int slot,
        ISampler sampler);

    void Draw(
        int vertexStart,
        int vertexCount);

    void DrawIndexed(
        int indexStart,
        int indexCount,
        int baseVertex = 0);

    void End();
}
```

Rename:

```csharp
SetShaderPipeline
```

to:

```csharp
SetPipeline
```

The word “shader” is incomplete. A pipeline also contains:

- vertex layout,
- rasterizer state,
- blend state,
- depth format,
- color format.

The current Metal `CommandEncoder.cs` already follows this explicit binding model.

---

# Phase 4: Complete pipeline descriptions

## 12. Add a real sampler contract to pipeline/resource binding

Your pipeline description currently contains:

- shaders
- vertex layout
- color format
- depth format
- rasterizer
- blend
- depth stencil

That is good, but ensure the pipeline is immutable and complete.

Current `ShaderPipelineDescription.cs`:

```csharp
public required TextureFormat? DepthFormat { get; init; }
```

This should be optional rather than required nullable:

```csharp
public TextureFormat? DepthFormat { get; init; }
```

Add:

```csharp
public int SampleCount { get; init; } = 1;
```

Potentially add:

```csharp
public PrimitiveTopology PrimitiveTopology { get; init; } =
    PrimitiveTopology.TriangleList;
```

```csharp
public enum PrimitiveTopology
{
    PointList,
    LineList,
    LineStrip,
    TriangleList,
    TriangleStrip
}
```

That replaces hard-coded `GL_TRIANGLES` behavior and maps directly to Metal’s `PrimitiveType`.

## 13. Expand `IDepthStencilState`

Current `IDepthStencilState.cs` is empty.

At minimum:

```csharp
public interface IDepthStencilState : IGPUResource
{
    DepthStencilDescription Description { get; }
}
```

The state itself should still be created from:

```csharp
IDepthStencilState CreateDepthStencilState(
    DepthStencilDescription description);
```

For a first implementation, this is enough:

```csharp
public sealed record DepthStencilDescription
{
    public bool DepthTestEnabled { get; init; }
    public bool DepthWriteEnabled { get; init; }
    public CompareFunction DepthCompare { get; init; } =
        CompareFunction.LessEqual;
}
```

Later add stencil:

```csharp
public bool StencilEnabled { get; init; }
public StencilFaceDescription Front { get; init; }
public StencilFaceDescription Back { get; init; }
public byte StencilReadMask { get; init; } = 0xFF;
public byte StencilWriteMask { get; init; } = 0xFF;
```

Do not add full stencil complexity until the engine needs it.

---

# Phase 5: Improve vertex-layout contracts

## 14. Fix `VertexBufferLayoutDescription`

Current `VertexBufferLayoutDescription.cs` should be:

```csharp
public sealed record VertexBufferLayoutDescription
{
    public required int Slot { get; init; }
    public required int Stride { get; init; }

    public StepMode StepMode { get; init; } =
        StepMode.PerVertex;
}
```

Add validation:

- `Slot >= 0`
- `Stride > 0`
- attribute offsets are within stride
- each attribute references an existing buffer slot
- no duplicate locations
- formats are supported by the selected backend

## 15. Expand vertex formats

Current `VertexFormat.cs` should cover the most common cross-backend formats:

```csharp
public enum VertexFormat
{
    Float,
    Float2,
    Float3,
    Float4,

    Half2,
    Half4,

    UInt,
    UInt2,
    UInt3,
    UInt4,

    SByte4Normalized,
    UByte4Normalized,
    Short2Normalized,
    Short4Normalized,
    UShort2Normalized,
    UShort4Normalized
}
```

Do not expose backend-specific formats unless they are capability-gated.

---

# Phase 6: Complete shader contracts

## 16. Improve shader module creation

Current `ShaderModuleDescription.cs` is a good start:

```csharp
public required ShaderStage Stage;
public required string Source;
public string? EntryPoint;
public ShaderLanguage ShaderLanguage;
```

Add optional source metadata:

```csharp
public string? DebugName { get; init; }
public IReadOnlyDictionary<string, string>? Defines { get; init; }
```

Do not assume every backend supports every language.

For Metal:

```csharp
new ShaderModuleDescription
{
    Stage = ShaderStage.Vertex,
    Source = metalSource,
    EntryPoint = "vertex_main",
    ShaderLanguage = ShaderLanguage.Metal
}
```

Your enum currently calls the value `Default`; add explicit names:

```csharp
public enum ShaderLanguage
{
    Default,
    Glsl,
    Msl,
    Hlsl,
    SpirV
}
```

Use `Msl` or `Metal`, but choose one naming convention and stick with it.

## 17. Expand `IShaderModule`

Current `IShaderModule.cs` is adequate:

```csharp
public interface IShaderModule : IGPUResource
{
    ShaderStage Stage { get; }
    string EntryPoint { get; }
}
```

Add optional diagnostics:

```csharp
public interface IShaderModule : IGPUResource
{
    ShaderStage Stage { get; }
    string EntryPoint { get; }
    string? DebugName { get; }
}
```

Compilation failures should throw a detailed exception during `CreateShaderModule`, not return a partially valid object.

## 18. Expand `IShaderPipeline`

Current `IShaderPipeline.cs` is empty.

At minimum:

```csharp
public interface IShaderPipeline : IGPUResource
{
    TextureFormat ColorFormat { get; }
    TextureFormat? DepthFormat { get; }
    int SampleCount { get; }
}
```

Optionally expose the original description:

```csharp
GraphicsPipelineDescription Description { get; }
```

The Metal implementation will wrap `MTLRenderPipelineState`, while OpenGL will wrap a linked program plus VAO/layout state.

---

# Phase 7: Surface and capability contracts

## 19. Improve `IRenderSurface`

Current `IRenderSurface.cs` is reasonable:

```csharp
int Width { get; }
int Height { get; }
bool VSync { get; set; }
void Resize(...);
IFrame AcquireFrame();
```

Add:

```csharp
TextureFormat ColorFormat { get; }
TextureFormat? DepthFormat { get; }
int SampleCount { get; }
```

Also consider:

```csharp
bool IsValid { get; }
```

`AcquireFrame()` may fail because:

- the window is minimized;
- the drawable is temporarily unavailable;
- the swapchain is out of date;
- the surface was disposed.

Define whether it returns `null` or throws. I recommend:

```csharp
IFrame? AcquireFrame();
```

for temporarily unavailable frames.

This maps well to Metal’s `nextDrawable`, which can return no drawable.

## 20. Improve capabilities

Current `IRenderCapabilities.cs` has useful fields but naming is inconsistent:

```csharp
SupportAnisotropicFiltering
SupportsAnisotropicFiltering
```

Use `Supports` consistently.

Add:

```csharp
bool SupportsTextureArrays { get; }
bool SupportsMultisampling { get; }
bool SupportsDepth24 { get; }
bool SupportsDepth32Float { get; }
bool SupportsComputeShaders { get; }
bool SupportsInstancing { get; }
bool SupportsSamplerObjects { get; }
bool SupportsMipmapGeneration { get; }
```

And limits:

```csharp
int MaxTextureSize { get; }
int MaxAnisotropy { get; }
int MaxVertexBufferSlots { get; }
int MaxUniformBufferSlots { get; }
int MaxTextureSlots { get; }
int MaxSamplerSlots { get; }
```

Capabilities should describe actual hardware/backend support, not merely API-level features.

---

# Phase 8: Define ownership and disposal rules

## 21. Separate resource lifetime from frame lifetime

Use `IGPUResource` for:

- buffers
- textures
- samplers
- shader modules
- pipelines
- depth-stencil states

Do not use it for:

- `IRenderPassDescription`
- `IFrame`
- perhaps `IRenderPass`

Recommended base:

```csharp
public interface IGPUResource : IDisposable
{
    bool IsDisposed { get; }
}
```

Rename `Disposed` to `IsDisposed` for normal .NET property conventions.

For frame/pass interfaces:

```csharp
public interface IFrame : IDisposable
{
    bool IsPresented { get; }
    ...
}
```

```csharp
public interface IRenderPass : IDisposable
{
    bool IsEnded { get; }
    void End();
}
```

Document invalid lifecycle calls:

- Cannot use a disposed resource.
- Cannot begin two passes on one frame.
- Cannot call `Present()` twice.
- Disposing a pass should end it if safe.
- Disposing an unpresented frame should release/abandon it.

---

# Phase 9: Build the Metal adapter

Once the contracts are stable, create a separate adapter layer.

Recommended structure:

```text
src/renderers/metal/
    native binding classes

src/renderers/metal-adapter/
    MetalRenderDevice.cs
    MetalRenderSurface.cs
    MetalFrame.cs
    MetalRenderPass.cs
    MetalBufferResource.cs
    MetalTextureResource.cs
    MetalSamplerResource.cs
    MetalShaderModule.cs
    MetalShaderPipeline.cs
    MetalDepthStencilState.cs
```

Do not make the raw `Metal.Device` implement `IRenderDevice` directly.

The raw binding should continue exposing Metal concepts:

```csharp
Metal.Device
Metal.CommandQueue
Metal.CommandEncoder
Metal.Texture
Metal.SamplerState
```

The adapter translates generic descriptions into those classes.

## Metal mapping

| Generic concept | Metal implementation |
|---|---|
| `IRenderDevice` | `MTLDevice` wrapper |
| `IRenderSurface` | `CAMetalLayer` wrapper |
| `IFrame` | `CAMetalDrawable` + command buffer |
| `IRenderPass` | `MTLRenderCommandEncoder` |
| `IBuffer` | `MTLBuffer` |
| `ITexture` | `MTLTexture` |
| `ISampler` | `MTLSamplerState` |
| `IShaderModule` | `MTLLibrary`/`MTLFunction` |
| `IShaderPipeline` | `MTLRenderPipelineState` |
| `IDepthStencilState` | `MTLDepthStencilState` |

---

# Phase 10: Implement OpenGL adapter

The OpenGL adapter should hide the current state machine behind the new explicit API.

## OpenGL mapping

| Generic concept | OpenGL implementation |
|---|---|
| `IBuffer` | buffer object handle |
| `ITexture` | texture object handle |
| `ISampler` | sampler object, or texture parameters if unavailable |
| `IShaderModule` | compiled shader handle |
| `IShaderPipeline` | linked program + vertex layout |
| `IRenderPass` | current framebuffer/context |
| `SetVertexBuffer` | bind VBO/VAO |
| `SetIndexBuffer` | bind element buffer |
| `SetPipeline` | `glUseProgram` + state setup |
| `SetSampler` | texture unit/sampler binding |
| `DrawIndexed` | `glDrawElements` |

This is where the old OpenGL-specific methods belong—not in the generic contract.

---

# Phase 11: Integrate with existing engine classes

The existing engine still uses the legacy `IRenderer.cs`, currently renamed to `IRendererOld` in parts of the repository.

You need to choose a migration strategy.

## Recommended transition

1. Keep `IRendererOld` temporarily.
2. Add `IRenderDevice` as the new API.
3. Implement Metal against `IRenderDevice`.
4. Implement OpenGL against `IRenderDevice`.
5. Update `Window` to own an `IRenderSurface`.
6. Update `Mesh` to create generic buffers/layouts.
7. Update `Texture` to become CPU-side image data.
8. Update `ShaderProgram` to create a generic pipeline.
9. Remove `RenderFactory.Renderer` once callers no longer use the old API.
10. Delete `IRendererOld` only after all engine code migrates.

Do not try to make the generic API and old API share every method. The old API is intentionally stateful; the new API should not be.

---

# Phase 12: Validation and tests

Before adding advanced features, test the contract at the abstraction level.

## Required tests

### Resource tests

- Create/dispose buffer.
- Upload buffer data.
- Reject updates larger than the buffer.
- Create sampled texture.
- Create depth texture.
- Create mipmapped texture.
- Generate mipmaps.
- Create sampler with each wrap mode.
- Create sampler with each filter mode.
- Dispose resources multiple times safely or reject consistently.

### Pipeline tests

- Create vertex shader module.
- Create fragment shader module.
- Reject unsupported shader language.
- Reject missing entry point where required.
- Reject pipeline with mismatched color/depth formats.
- Reject invalid vertex layout.
- Reject unsupported topology or vertex format.

### Render-pass tests

- Acquire frame.
- Begin pass.
- Bind pipeline/resources.
- Draw.
- End pass.
- Present.
- Verify invalid lifecycle order produces clear errors.

### Capability tests

- Query Metal capabilities.
- Query OpenGL capabilities.
- Ensure unsupported features are reported rather than silently ignored.

---

# Important design decisions still needed

## 1. Should `IFrame.ColorTexture` exist?

Yes, but perhaps rename it:

```csharp
ITexture ColorAttachment { get; }
```

`ColorTexture` implies a normal sampled texture. `ColorAttachment` better describes a swapchain image.

## 2. Should render passes own command buffers?

For Metal, yes internally. The generic API does not need to expose command buffers initially.

Later, if you add asynchronous submission or compute:

```csharp
ICommandBuffer CreateCommandBuffer();
```

For now, `IFrame.BeginRenderPass()` is simpler.

## 3. Should `IRenderPass` inherit `IGPUResource`?

No. It is a short-lived encoder, not a persistent GPU resource.

Use:

```csharp
public interface IRenderPass : IDisposable
```

## 4. Should `IDepthStencilState` be separate from the pipeline?

Metal supports a separate depth-stencil state, while depth format belongs to the pipeline. Keeping it separate gives you flexibility:

```csharp
pipeline = device.CreateShaderPipeline(...);
depthState = device.CreateDepthStencilState(...);
pass.SetPipeline(pipeline);
pass.SetDepthStencilState(depthState);
```

## 5. Should samplers be separate from textures?

Yes. The current Metal implementation already demonstrates why:

```csharp
pass.SetTexture(0, texture);
pass.SetSampler(0, sampler);
```

One texture can be sampled with multiple policies.

---

# Suggested execution order

## Immediate

1. Fix namespace inconsistencies.
2. Fix compile errors and enum typos.
3. Add `ISampler`.
4. Add `SamplerDescription`.
5. Complete `IRenderDevice`.
6. Remove `IGPUResource` from descriptions.
7. Add load/store attachment descriptions.
8. Add `SetSampler` and `End()` to `IRenderPass`.
9. Rename `SetShaderPipeline` to `SetPipeline`.
10. Add missing resource metadata.

## Next

11. Implement `MetalRenderDevice`.
12. Implement `MetalRenderSurface`.
13. Implement `MetalFrame`.
14. Implement `MetalRenderPass`.
15. Wrap existing Metal resources in generic resource classes.
16. Run the current demo through the generic interfaces.

## Then

17. Implement the OpenGL adapter.
18. Migrate `Window`.
19. Migrate `Mesh` and vertex layouts.
20. Migrate CPU texture upload.
21. Migrate shader/pipeline creation.
22. Remove old renderer dependencies.

## Later

23. Add stencil.
24. Add multisampling.
25. Add texture arrays and 3D textures.
26. Add instancing.
27. Add indirect drawing.
28. Add compute support.
29. Add command buffers and asynchronous submission.
30. Add capability-driven feature validation.

The most important next milestone is not adding more enums. It is getting a complete path working:

```text
IRenderDevice
    -> IRenderSurface
    -> IFrame
    -> IRenderPass
    -> IShaderPipeline
    -> IBuffer / ITexture / ISampler
    -> DrawIndexed
    -> Present
```

Once that path works on Metal and OpenGL, the contract will have proven that it is genuinely backend-agnostic rather than just a collection of plausible types.