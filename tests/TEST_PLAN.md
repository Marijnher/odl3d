# odl3d Test Plan

## 1. Goals

1. Prove the **generic renderer API** (`src/renderer/generic`) has one well-defined behaviour, and that **every backend** (OpenGL, Metal, and future Vulkan/D3D12 per `architecture.puml`) implements exactly that behaviour.
2. Prove **backend-specific internals** (GL state caching, GLSL preprocessing, Metal buffer slots, ObjC bindings) are correct on their own terms.
3. Prove the **odl3d library layer** (scenes, drawables, meshes, textures, text, loaders, cameras, input) is correct independent of which backend is active.
4. Keep the bulk of tests **CPU-only, fast and deterministic**; reserve GPU tests for what can only be verified on a GPU.

### Answer to "should each backend be tested individually?"

Yes, but **not with separate copies of the same tests**. Use a single **conformance suite** written against `IRenderDevice` and run it once per backend (abstract xUnit test classes, one concrete subclass per backend). That is what enforces consistency across backends: the same assertion must pass on every backend. Separate per-backend projects only hold tests for **backend-specific internals** that have no generic equivalent.

## 2. Test tiers

| Tier | Name | Needs GPU? | Needs window? | Runs on | Speed |
|---|---|---|---|---|---|
| T0 | CPU unit (library + backend pure logic) | No | No | Every PR, all OSes | ms |
| T1 | Native interop (FreeType, GLFW init, Metal bindings) | No | No (GLFW init only) | Every PR, all OSes | ms |
| T2 | Backend conformance (generic API contract) | Yes | Hidden window / offscreen | Every PR (software GL) + nightly (HW) | s |
| T3 | Backend-specific GPU | Yes | Hidden window | Every PR (software GL) + nightly (HW) | s |
| T4 | Cross-backend image consistency (golden images) | Yes | Hidden window / offscreen | Every PR (software GL), nightly (all) | s |
| T5 | Library integration (scenes/drawables on a real device) | Yes | Hidden window | Every PR (software GL) + nightly | s |
| T6 | Non-functional: leaks, soak, perf, fuzz | Mixed | Mixed | Nightly / weekly | min |
| T7 | Manual exploratory (demo app) | Yes | Visible | Before release | - |

CPU-side vs GPU-side distinction:
- **CPU-side** tests verify *data handed to the GPU*: geometry, pixel buffers, struct layouts, matrices, shader source, enum mappings, resource bookkeeping. These are exact, deterministic, and run everywhere.
- **GPU-side** tests verify *what the GPU did with it*: pixels in a framebuffer, depth results, sampling, blending, errors raised by the driver/validation layer. These are only observable through **readback** and require tolerances.

Rule of thumb: if a bug can be caught by inspecting the bytes before upload, catch it in T0.

## 3. Prerequisites / testability gaps (must fix first)

Found while reviewing the code; GPU tiers are blocked or weakened without them.

| # | Gap | Impact | Proposal |
|---|---|---|---|
| P1 | **No pixel readback** in the generic API (`IRenderFrame` only has `CreateRenderPass`/`Present`; `ITexture` only `Upload`; `TextureUsage.CopySource` exists but nothing uses it). | No GPU-side assertion is possible. | Add `IRenderFrame.ReadPixels(Rect) : byte[]` (RGBA8, row 0 = top) or `ITexture.Download()`. Can be `internal` + `InternalsVisibleTo` if not meant to be public. |
| P2 | **No offscreen render target**: `ColorAttachmentDescription` has no texture; passes can only target a window surface. | Every GPU test needs a window; GL requires one anyway, Metal does not. | Add `IRenderDevice.CreateOffscreenSurface(width, height, format)` returning an `IRenderSurface`. On GL back it with an FBO in a hidden GLFW window; on Metal with a `MTLTexture`. |
| P3 | No `InternalsVisibleTo` for tests. | Enum-mapping tables, `ShaderPreprocessor`, `GLVertexLayout`, `MetalBufferSlots` are not reachable. | Grant access to specific test assemblies, never a wildcard. Unit-test access is now enabled; add backend assembly grants when those projects are introduced. |
| P4 | `FontResolver` falls back to **system fonts**. | Text tests differ per machine. | Commit an OFL-licensed fixture font (e.g. DejaVu Sans / Noto Sans, regular + bold + italic) to `tests/fixtures/fonts` and register it via `AddSearchPath`. Never depend on system fonts in assertions. |
| P5 | Input is only fed by GLFW callbacks. | Input logic is untestable without a real window. | Expose an internal way to inject key/mouse events into `AbstractInputManager` (or test through `ProxyInputManager`). |
| P6 | Native libs (glfw, freetype) are copied by `odl3d.csproj`; test projects must copy them too. | `DllNotFoundException` in CI. | Shared `Directory.Build.targets` or reference the same items from test projects. |
| P7 | The former `tests/odl3d.Tests/` layout was redundant. | Confusing once multiple test projects exist. | Keep category directories directly under `tests/`; resolved by the section 4 layout. |
| P8 | xUnit 2 cannot skip dynamically. | Metal tests on Windows/Linux, or GL on a GPU-less runner, would fail instead of skip. | Move to xUnit v3 (`Assert.Skip`) or add `Xunit.SkippableFact`. |

## 4. Project structure

```
tests/
  unit/                              # T0 - library CPU logic (no native, no GPU)
    odl3d.Tests.Unit.csproj
  native/                            # T1 - FreeType / GLFW / Metal binding interop
    odl3d.Tests.Native.csproj
  rendering/
    conformance/                     # T2 + T4 - shared suites against IRenderDevice
      odl3d.Tests.Rendering.Conformance.csproj
    opengl/                          # backend conformance + GL-specific tests
      odl3d.Tests.Rendering.OpenGL.csproj
    metal/                           # backend conformance + Metal-specific tests
      odl3d.Tests.Rendering.Metal.csproj
  integration/                       # T5 - real-device library workflows
    odl3d.Tests.Integration.csproj
  benchmarks/                        # T6 - CPU benchmarks + GPU frame timing
    odl3d.Benchmarks.csproj
  fixtures/
    models/                          # tiny hand-written .obj/.mtl/.dae + references to assets/
    images/                          # png/jpg variants
    fonts/
    shaders/                         # valid/invalid GLSL + MSL
  golden/                            # optional initially; add reference PNGs when T4 scenes stabilize
```

Directory names describe test categories and are intentionally plain; project names live in the `.csproj` filenames. `golden/` is useful for repeatable cross-backend image regressions, but is not needed until T4 rendering tests and their canonical scenes are in place.

Conventions:
- Traits: `[Trait("Tier","T0".."T6")]`, `[Trait("Backend","OpenGL"|"Metal")]`, `[Trait("Requires","Gpu"|"Window"|"Font")]` so CI can filter (`dotnet test --filter "Tier=T0"`).
- **Conformance pattern:**
  ```csharp
  public abstract class BufferConformanceTests : IClassFixture<DeviceFixture> {
      protected abstract RenderTarget Target { get; }
      [Fact] public void SetData_out_of_range_throws() { ... }
  }
  [Trait("Backend","OpenGL")] public sealed class OpenGLBufferTests : BufferConformanceTests { protected override RenderTarget Target => RenderTarget.OpenGL; }
  [Trait("Backend","Metal")]  public sealed class MetalBufferTests  : BufferConformanceTests { protected override RenderTarget Target => RenderTarget.Metal; }
  ```
  `DeviceFixture` creates the device once per class, skips if unavailable (Metal off macOS, no GL context), and owns the hidden GLFW window.
- **GPU tests run serially** (`[CollectionDefinition(DisableParallelization = true)]`): GLFW must be used from the main thread and GL contexts are thread-affine. GPU tests are **synchronous** (no `await`, which may resume on another thread).
- Debug validation is **on** in all GPU tests: GL `KHR_debug` callback (fail test on `HIGH` severity), Metal `MTL_DEBUG_LAYER=1` (fail on validation messages).

## 5. T0 - CPU unit tests (`tests/unit`)

Initial implementation in `tests/unit` covers color/rectangle/matrix/timer behavior, camera math, mesh builder indexing and counts, shader-data offsets and default depth conventions, sampler defaults, texture pixels/builders/lifecycle, Object3D transforms and resource ownership, and culture-independent OBJ loading. The remaining checklist below is still planned work; in particular, deterministic text tests need the fixture font from P4, and input/backend-internal coverage needs the corresponding test seams and projects.

### 5.1 Utilities
- `Color`: constants have correct bytes; equality; conversion to `Vector4` (0..1) round-trips.
- `Rect`: `Contains` on edges (decide and lock inclusive/exclusive), zero/negative sizes, `Intersects` symmetric, touching edges.
- `MatrixConversion`: round-trips; **column/row-major convention** matches what GLSL (`mat4` column-major) and MSL (`float4x4` column-major) expect.
- `Timer`: monotonic, delta non-negative (inject a clock if possible).

### 5.2 Cameras
- `Front` is unit length for a grid of yaw/pitch; `Front/Up/Right` are orthonormal; `Left == -Right`, etc.
- Pitch clamped (no flip at +-90 deg / gimbal).
- `GetViewMatrix()` transforms `Position` to the origin and `Position + Front` onto -Z.
- `GetProjectionMatrix()`: point at `NearPlane` -> NDC z = 0, at `FarPlane` -> z = 1 (System.Numerics is [0,1] depth); aspect ratio respected; resize updates aspect.
- **Depth-convention test:** the GLSL default vertex shader remaps z with `z = z*2 - w` (`DefaultShaders.cs`) while MSL does not. Assert the remap is present in GLSL and absent in MSL, so both backends clip at the same distances. Document that custom GL shaders must do the same.
- `MoveableCamera.Update(dt)`: movement scales linearly with `dt`, diagonal movement not faster, no movement when input disabled (needs P5).

### 5.3 Meshes
- `MeshBuilder` factories (`CreateQuad/Plane/Cube/Sphere`, `Mesh.CreateTextPlane`):
  - Vertex/index counts match formulas (e.g. cube 24 verts / 36 indices; sphere as a function of segments/rings).
  - Every index `< VertexCount`; index count % 3 == 0; no degenerate triangles.
  - **Winding:** geometric face normal agrees with the declared `FrontFace` and with the vertex normals (dot > 0) - wrong winding only shows up as missing faces when culling.
  - Normals unit length; UVs in [0,1]; closed meshes are watertight (each edge shared by exactly 2 triangles).
  - Bounding box matches the requested size.
- `Build(hasNormals)`: every vertex contains position, UV, and normal fields; unspecified normals are zero and `HasNormals` reports whether meaningful normals were supplied.
- `Mesh.QuadFlippedV` vs `Mesh.Quad`: only V differs (`v' = 1 - v`), positions/indices identical (regression).
- `Mesh`: `Dispose` idempotent; `EnsureUploaded` after dispose throws.

### 5.4 Vertex and shader-data layout (high value)
GPU layout mismatches fail silently (garbage rendering), so lock them down on the CPU:
- `Vertex` is a sequential unmanaged struct with a 32-byte position/UV/normal layout. Verify its field offsets match the fixed vertex stride and the `VertexAttributeDescription.Offset`s used by `ShaderPipeline.CreateDefault`.
- `ObjectShaderData` == 256 bytes, `SceneShaderData` == 128 bytes; every field offset matches the **std140** block in the GLSL default shader and the struct in the MSL default shader.
- 256 bytes is also the uniform-offset alignment needed by GL (`GL_UNIFORM_BUFFER_OFFSET_ALIGNMENT`) and Metal (constant-buffer offsets on macOS); assert size % 256 == 0.
- `VertexLayoutDescription` checks: attributes do not overlap, fit within the stride, have unique indices, and `VertexFormat` byte sizes are correct.

### 5.5 Textures
- `TextureBuilder`: gradient endpoints exact, circle inside/outside/edge, `SetPixel` out-of-bounds behaviour, **row 0 = top** convention.
- `Texture`: `Pixels.Length == w*h*4`; invalid dimensions throw `TextureException`; `HasPartialAlpha` correct for opaque / binary-alpha / soft-alpha images, and **invalidated when pixels change**; `FilterMode`/`WrapMode` defaults; `Dispose` idempotent; `OnDisposed` raised exactly once; `Upload` after dispose throws.
- `Texture.FromImage` (decodl): fixtures for PNG RGBA/RGB/gray/gray+alpha/palette/16-bit/interlaced, and JPEG; check pixel values at known coordinates and orientation; missing/corrupt file -> `TextureException` (not a raw decoder exception).
- `Sampler`: defaults; every filter/wrap combination maps to a `SamplerDescription`.

### 5.6 Loaders
- `ObjLoader` (small hand-written fixtures): triangles; quads/n-gons (triangulation); negative indices; `v`, `v/vt`, `v//vn`, `v/vt/vn`; missing `vt`; `o`/`g` groups; `usemtl` switching inside a group; comments, blank lines, CRLF, trailing whitespace; **parsing under a `de-DE` culture** (decimal comma bug); malformed line -> defined exception with line number.
- `MtlLoader`: `Kd/Ka/Ks/Ns/d`, `map_Kd` relative to the `.mtl` file, paths with spaces, missing material file.
- `DaeLoader`: tiny fixtures for node transform hierarchy, `up_axis` (Y_UP vs Z_UP), `<triangles>` vs `<polylist>`, multiple UV sets, missing images.
- **Snapshot tests on real assets** (`assets/center`, `gym`, `oras_center`, `cube.obj`): vertex/index counts, bounding box, material names, texture paths. Catches loader regressions without a GPU.
- Security: texture paths in MTL/DAE that escape the model directory (`..\..\`) are rejected or contained.

### 5.7 Text (CPU side)
- `FontResolver`: custom search paths before system paths; case-insensitive; extension optional; recursive; unknown font -> defined exception.
- `Font.Get`: same arguments -> same instance; different size -> different instance; `Dispose` removes it from the cache; metrics sane for the fixture font (`Ascender > 0`, `Descender <= 0`, `LineHeight >= Ascender - Descender`).
- Kerning: known pair (e.g. "AV") is negative for the fixture font; `MeasureLine` equals the sum of advances plus kerning.
- Synthetic bold is wider than regular; synthetic italic has a sheared bounding box.
- `GlyphAtlas`: glyph rectangles never overlap (property-based test with FsCheck over random glyph sets); padding prevents bleeding; `GetOrAdd` is idempotent; `Grow` keeps existing glyph pixels and **updates their UVs**.
- `OutlineDecomposer`: contour counts ('O' = 2, 'i' = 2, 'B' = 3); outer vs hole orientation; curve flattening within tolerance.
- `PolygonTessellator` (property-based): sum of triangle areas == polygon area minus holes; all triangles have the same winding; no triangle centroid outside the polygon; handles collinear points, duplicate vertices, holes touching the outer contour; empty input.
- `TextGeometry.Build`: extruded mesh is watertight; normals point outward; empty string, whitespace-only text, newlines, all `TextAlign` values, underline/strikethrough geometry; `Depth = 0`.
- `Text2D` uses `Mesh.QuadFlippedV` and `Text3D` uses `Mesh.Quad` (regression).

### 5.8 Scenes, objects, drawables (CPU logic)
- `Object3D.GetModelMatrix()`: translate * rotate * scale order; rotation in degrees and axis order locked by a test; scale applied before rotation.
- `Scene.Add/Remove`: sets/clears `Object.Scene`; adding twice and removing a non-member behave as specified.
- Pass partitioning: opaque objects go to `RenderPass.Opaque` and partial-alpha objects to `RenderPass.Transparent`. **Scene2D draws both passes** (regression: anti-aliased 2D text was invisible). Transparent objects sorted back-to-front if sorting is implemented.
- `Scene2D`: ortho projection maps pixel (0,0) to top-left and (W,H) to bottom-right; `Sprite2D` pixel position and size.
- `Sprite2D.RegisterMousePressInside`: hit test at edges and outside (needs P5).
- Ownership: `AutoDisposeTexture/AutoDisposeMesh` dispose only owned resources; a texture shared by two objects survives disposing one of them.

### 5.9 Input
- Key/mouse state transitions (down, repeat, up); handlers called once per event; `ProxyInputManager` forwards only while enabled; `InputException` when input is disabled (needs P5).

### 5.10 Renderer front-end and backend CPU-side internals
These live in the backend test projects but need no GPU:
- `RenderFactory.Create`: invalid enum -> `RenderException`; default is Metal on macOS and OpenGL otherwise; `Metal` on Windows/Linux -> `RenderException` (not `DllNotFoundException`/`EntryPointNotFoundException`).
- **Enum mapping exhaustiveness:** for each backend, every value of every generic enum (`BlendFactor`, `BlendOperation`, `CompareFunction`, `CullMode`, `FrontFace`, `PrimitiveType`, `IndexType`, `TextureFormat`, `TextureFilter`, `MipmapFilter`, `WrapMode`, `LoadAction`, `StoreAction`, `VertexFormat`, ...) maps to a native value, or throws a documented "unsupported" `RenderException`. Never a silent 0/default. Iterate over `Enum.GetValues<T>()` so new enum members fail the test until mapped.
- `DefaultShaders`: source is returned for every `RenderTarget`; uniform block / buffer names and binding indices agree with `BufferSlots`.
- **Offline shader compilation** (no GPU): compile default and fixture GLSL with `glslangValidator` (`#version 330 core`), and MSL with `xcrun -sdk macosx metal` on macOS. Optional: SPIR-V reflection to verify block sizes/offsets against section 5.4.
- `ShaderPreprocessor` (GL): `#version` stays first; defines injected; line numbers preserved (`#line`) so compiler errors point to the right line; idempotent.
- `GLVertexLayout` cache key: equal layouts share an entry, different layouts do not.
- `MetalBufferSlots`: **vertex-buffer slots and uniform-buffer slots never collide.** Metal uses one buffer argument table for both, which is a classic bug source.

## 6. T1 - Native interop tests
- FreeType: init/done; load a host font face; check metrics, kerning, outline extraction and rasterized glyph data. **Run on Windows, Linux and macOS**: protects the LLP64 vs LP64 `FT_Long` struct-layout fix (`src/native/FT.cs`). Load/unload many faces with no crash. A licensed repository fixture font remains desirable for stable metric assertions.
- GLFW: `glfwInit` succeeds; version >= 3.3; hidden-window creation where a display exists (Xvfb on Linux). GLFW and FreeType first load local `bin/` libraries, then use OS library resolution. Skipped on hosted macOS CI: GLFW's Cocoa backend requires the main thread and a logged-in window-server session, neither of which hosted macOS runners reliably provide, and `glfwInit`/`glfwCreateWindow` hang rather than fail fast without one.
- Metal bindings (macOS): the native framework/runtime load and platform guard are tested; selector and device-presence assertions still require a macOS runner.

Coverage policy: T0 and T1 runs collect Cobertura line and branch data using `tests/coverage.runsettings`. Tier-owned code should reach 100% line coverage; all boolean/conditional branches should be exercised where platform behavior permits. Platform-specific native branches are verified by the Windows/Linux/macOS CI matrix, and new code must add tests for each new branch before merge.

## 7. T2 - Backend conformance suite (generic API contract)

Written once against `IRenderDevice`, run per backend. Every test asserts **behaviour and exception type**, so backends cannot diverge. Pixel checks use readback (P1) on a small offscreen target (e.g. 64x64, `SampleCount = 1`, VSync off).

### 7.1 Device and capabilities
- `Name` non-empty; `RenderTarget` correct.
- Capabilities within spec minimums (e.g. `MaxTextureSize >= 4096` or the GL 3.3 minimum you choose to require, `MaxTextureSlots >= 16`, `MaxVertexBufferSlots >= 1`).
- Features reported as unsupported throw a consistent `RenderException` when used (e.g. wireframe when `SupportsWireframe == false`; Metal supports fill mode lines, GL ES does not).

### 7.2 Resource lifecycle (every resource type)
- Create -> dispose -> dispose again (no throw).
- Use after dispose -> `ObjectDisposedException` on **every** backend.
- Dispose device with live resources: defined behaviour (throw, or release everything) applied the same way on every backend.

### 7.3 Buffers
- With/without initial data; `SetData` full, partial with offset; `offset + count > Size` -> `ArgumentOutOfRangeException`; zero-size buffer; large buffer (64 MB).
- Content verified by drawing (vertex buffer -> pixel position, uniform buffer -> pixel color).
- Updating a uniform buffer **every frame for N frames** with readback each frame: catches CPU/GPU races (Metal in-flight frames, GL buffer orphaning).
- Multiple objects using different 256-byte offsets of one uniform buffer produce distinct colors.

### 7.4 Textures and samplers
- Every `TextureFormat`: create, upload, sample, read back (or a documented `RenderException`).
- 1x1, NPOT, odd widths (e.g. 3x3 RGB, a **GL unpack-alignment** trap), `MaxTextureSize`, `MaxTextureSize + 1` -> exception; upload with the wrong byte count -> exception; mip levels.
- **Orientation:** 2x2 texture with four distinct corner colors drawn full-screen; read back and assert each corner. Catches GL bottom-left vs Metal top-left origin bugs.
- Filtering: magnify a 2x2 texture; Nearest gives hard edges, Linear gives the expected midpoint value (+- tolerance).
- Wrap: UVs in [-1, 2] with Repeat / Clamp / Mirror give the expected pattern.
- Anisotropy above `MaxAnisotropy` is clamped, not an error.
- sRGB vs linear formats: expected encoded values.

### 7.5 Shaders and pipelines
- Valid module compiles; invalid source -> `ShaderException` whose message contains the compiler log and line number; wrong language for the backend (GLSL on Metal, MSL on GL) -> `ShaderException`; missing entry point -> `ShaderException`.
- Vertex layout that does not match shader inputs -> defined error.
- Cull mode x front face: draw one CW and one CCW triangle and assert which are visible for all combinations.
- Blend: each `BlendFactor`/`BlendOperation` against analytically computed expected colors (straight vs premultiplied alpha).
- Depth: each `CompareFunction`; depth write off; two overlapping quads drawn in both orders give the same result with depth on.
- Near/far clipping: geometry just inside/outside near and far planes is clipped identically on all backends (validates the GLSL z remap).

### 7.6 Render passes, surfaces, frames
- `LoadAction.Clear` gives exactly the clear color; `Load` preserves previous contents; `DontCare` is not asserted.
- `SetViewport`/`SetScissor`: `Rect` origin convention is the **same** on all backends (top-left). Draw a full-screen quad with a scissor and assert the lit rectangle.
- `Draw`, `DrawIndexed` (16- and 32-bit indices, first index, base vertex), every `PrimitiveType`.
- Misuse produces the same exception type on every backend: draw without a pipeline, draw after `End()`, `End()` twice, `Present()` twice, render pass from a disposed frame.
- Surface: `Resize` to new size updates `Width/Height` and the next frame's size; resize to 0x0 (minimized) does not crash; disposing an unpresented frame is safe.
- Multiple render passes in one frame (opaque then transparent) with `Load`.

## 8. T3 - Backend-specific GPU tests

### OpenGL (`odl3d.Tests.Rendering.OpenGL`)
- Context is 3.3+ core profile; required extensions present.
- `KHR_debug` callback: zero errors across the whole conformance suite.
- **State cache coherence** (`GLVertexState`, bound textures/samplers/programs): after binding A, B, A, the driver state (`glGet*`) matches the cache; no state leaks between passes/pipelines.
- VAO-per-layout cache: switching layouts rebinds correctly; deleting a buffer referenced by a cached VAO.
- Uniform-block binding points and texture units match `BufferSlots`.
- `GL_UNPACK_ALIGNMENT` handled for RGB and odd widths.
- macOS GL 4.1 (deprecated but present): conformance suite runs too, if supported.
- Driver matrix (nightly): Mesa llvmpipe, NVIDIA, AMD, Intel.

### Metal (`odl3d.Tests.Rendering.Metal`)
- Validation layer enabled: zero validation messages across the suite.
- Command buffers complete with `MTLCommandBufferStatusCompleted`; errors reported as `RenderException`.
- **Frames in flight:** CPU never writes a buffer region the GPU is still reading (per-frame uniform test from 7.3 under load).
- Autorelease pools: object count / memory stays flat over 10k frames.
- Storage modes: `Managed` buffers on Intel/AMD Macs call `didModifyRange`; `Shared` on Apple Silicon.
- Drawable pixel format (BGRA8) vs RGBA textures: no channel swap in readback.
- Drawable acquisition on a hidden/occluded window does not deadlock (timeout).
- Hardware matrix (nightly): Apple Silicon and, if available, Intel/AMD Mac.

## 9. T4 - Cross-backend image consistency (golden images)

- A fixed set of **canonical scenes**, built only through the public odl3d API: clear; single colored quad; textured quad (orientation); depth-overlap; alpha blend stack; cube with culling; sphere with normals; Sprite2D/Text2D overlay; Text3D; `cube.obj`; one DAE asset; wireframe.
- Determinism: fixed resolution (256x256), fixed camera, no time-based animation, fixture font only, MSAA off, VSync off.
- **One golden image per scene, shared by all backends.** A per-backend override is allowed only with a written reason (e.g. documented rasterization difference).
- Comparison metric: per-pixel channel delta <= 2 for >= 99.5% of pixels and max delta <= 16 (tune per scene); optionally FLIP/SSIM for textured/AA scenes. Edges and filtered regions are the only places allowed to differ.
- On macOS, render with **both Metal and OpenGL in the same run and diff them directly**: the strongest consistency check available.
- On failure write `actual.png`, `expected.png`, `diff.png` as CI artifacts. Regenerate with `ODL3D_UPDATE_GOLDENS=1`; golden changes need human review in the PR.
- Goldens are produced on the reference CI configuration (Mesa llvmpipe); hardware runs use the same goldens with tolerance.

## 10. T5 - Library integration (on a real device)
Backend-parameterized like T2.
- `GraphicsApplication`: create/dispose; dispose order (windows before device); second window on OpenGL -> defined exception (documented single-window limitation).
- `Window`: create, resize (camera aspect, `Scene2D.Viewport`, surface size updated), close, `Update/Render` loop for N frames.
- `Mesh`/`Texture` lazy upload happens once (`EnsureUploaded`), re-upload after modifying pixels.
- `ShaderPipeline.CreateDefault` with and without normals; `Wireframe` toggle.
- Scene3D with `Model` loaded from `assets/` renders (non-empty and matches golden).
- Scene2D: `Sprite2D` from PNG upright; **anti-aliased `Text2D` visible** (transparent-pass regression); text upright (flip regression).
- `Text3D`, `TextBillboard`, `Plane3D`, `Sprite3D` smoke + golden.
- `GlyphAtlas.Grow` mid-frame: previously drawn text is still correct in the next frame.
- Missing texture in a model: fallback behaviour, no crash.
- Demo smoke test: each demo scene (`DemoScene`, `ModelScene`, `UIScene`) runs 300 frames on a hidden window without exceptions or validation errors.

## 11. T6 - Non-functional
- **Leaks:** create/dispose each resource type 10k times; GL object names / Metal `currentAllocatedSize` / process memory stay flat. Steady-state frame loop: `GC.GetAllocatedBytesForCurrentThread()` per frame stays near zero.
- **Soak:** 100k frames with resizes and add/remove of objects.
- **Performance (BenchmarkDotNet, CPU):** `PolygonTessellator`, `GlyphAtlas`, `TextGeometry`, loaders on real assets, `MeshBuilder.CreateSphere`, `Texture.HasPartialAlpha`. Track trends and alert on >10% regression; no hard pass/fail thresholds on shared runners.
- **GPU frame time** (nightly on hardware only): GPU timestamp queries for canonical scenes; trend tracking.
- **Fuzzing** (SharpFuzz or mutation-based): OBJ, MTL, DAE, PNG/JPEG decoders, font files. Only documented exceptions are allowed; no crashes, hangs or unbounded allocations.
- **Threading:** calling GPU APIs from a non-render thread gives a defined error, not a driver crash.

## 12. CI matrix

| Runner | T0 | T1 | OpenGL T2-T5 | Metal T2-T5 | Notes |
|---|---|---|---|---|---|
| windows-latest | yes | yes | yes (Mesa llvmpipe `opengl32.dll` next to test binaries) | skip | |
| ubuntu-latest | yes | yes | yes (`xvfb-run`, `LIBGL_ALWAYS_SOFTWARE=1`) | skip | Reference config for goldens |
| macos-14 (arm64) | yes | yes | yes (GL 4.1) | yes if a Metal device is exposed, otherwise skip with reason | Hosted VMs may have limited Metal; use self-hosted if needed |
| Self-hosted GPU pool (nightly) | - | - | NVIDIA / AMD / Intel | Apple Silicon | Also T6 perf/soak |

- PR gate: T0 + T1 everywhere; T2-T5 on software GL and on Metal if available.
- Nightly: everything, including hardware GPUs, fuzzing, soak, benchmarks.
- T0 + T1 require 100% line coverage for tier-owned source files across the supported OS matrix; branch coverage is reported and each reachable branch must have an assertion. Platform-specific lines (for example Windows `FT_Long` layout and macOS Cocoa GLFW bindings) are combined by `.github/workflows/tests.yml`. Coverage collection is configured in `tests/coverage.runsettings`; run a project with `dotnet test <project.csproj> --settings tests/coverage.runsettings --collect:"XPlat Code Coverage"`.
- GPU tiers are judged by the API-surface checklist in sections 7-8 instead of line coverage.

## 13. Regression register
Each bug fix adds a test at the lowest tier that reproduces it. Known items to lock in now:
| Bug | Test |
|---|---|
| Scene2D skipped the transparent pass | 5.8 pass partitioning + 10 Text2D visible |
| Scene2D vertical flip / `QuadFlippedV` | 5.3 + 5.7 + 7.4 orientation + 10 text upright |
| FreeType `FT_Long` size on Windows (LLP64) | 6 FreeType on all three OSes |
| `Font` cache eviction on dispose | 5.7 |
| GL vs Metal depth range (`z*2-w` remap) | 5.2 + 7.5 near/far clipping |

## 14. Rollout order
1. **Phase 1 (CPU):** establish `tests/unit`, then add struct-layout tests, mesh/texture/loader/text/camera tests, enum exhaustiveness, and offline shader compilation. Resolve P6/P7 where applicable. Highest value per effort.
2. **Phase 2 (testability):** readback + offscreen surface (P1, P2), `InternalsVisibleTo` (P3), fixture font (P4), input injection (P5), dynamic skip (P8).
3. **Phase 3 (GPU):** conformance suite on OpenGL/llvmpipe in CI, then Metal on macOS; GL-specific and Metal-specific tests.
4. **Phase 4:** golden-image consistency (including direct Metal vs GL diff on macOS), integration tests, demo smoke.
5. **Phase 5:** leaks, soak, benchmarks, fuzzing, hardware GPU nightly.
