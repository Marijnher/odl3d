# odl3d - Object Display Layer 3D

A .NET 3D graphics library with an abstract renderer stack and GLFW-based windowing for cross-platform game development and graphics applications.

## Overview

odl3d is a C# graphics library designed to simplify 3D game development and graphics programming. It provides a unified API for handling 3D rendering, scene management, shaders, and asset management across Windows, Linux, and macOS.

## Architecture

Applications create a graphics application and window, construct drawables independently, explicitly add them to 2D or 3D scenes, and render those scenes through the application's renderer.

![odl3d Architecture](docs/architecture.png)

Source: [docs/architecture.puml](docs/architecture.puml).

### Renderer API

The renderer API provides a backend-independent device, frame lifecycle, and GPU resource contracts for OpenGL and Metal.

![Renderer Architecture](docs/renderer-api.png)

Source: [docs/renderer-api.puml](docs/renderer-api.puml).

## Dependencies

- **GLFW3** - Window management
- **Freetype** - Font rendering

## Project Layout

- `odl3d.csproj` builds the reusable core library from `src/`.
- `demo/odl3d.Demo.csproj` builds the interactive sample and copies its assets.
- `tests/unit/odl3d.Tests.Unit.csproj` contains T0 CPU-only unit tests.
- `tests/native/odl3d.Tests.Native.csproj` contains T1 native interop tests.
- `tests/TEST_PLAN.md` describes planned rendering, integration, and performance coverage.
- `external/decodl` is the upstream decoder project, pinned as a Git submodule.

Default GLSL and MSL sources are kept as editable raw-string constants in `src/shaders/DefaultShaders.cs`; they are compiled directly from source and do not depend on demo files or the current working directory.

## Building

Clone with the decoder submodule, or initialize it after cloning:

```sh
git clone --recurse-submodules https://github.com/Marin-MK/odl3d.git
# Or, from an existing checkout:
git submodule update --init --recursive
```

Build the library and demo:

```sh
dotnet build odl3d.sln
dotnet run --project demo/odl3d.Demo.csproj
```

## Testing

For T0/T1 test commands, native prerequisites, and readable line/branch coverage reports, see [TESTING.md](TESTING.md).

## Example usage

```csharp
using var app = new GraphicsApplication(RenderTarget.OpenGL);
using var window = app.CreateWindow(800, 600, "My application");
window.Camera = new MoveableCamera(window) { Position = new Vector3(1, 0.5f, 2) };
var scene = window.CreateScene3D();

var texture = TextureBuilder.CreateCheckerboard(64, 64, Color.Magenta, new Color(0, 255, 255));
var sprite = new Sprite3D(texture);
scene.Add(sprite);

while (!window.ShouldClose)
{
   window.Update(0);
   window.Render();
}
```

## Getting Started

1. Ensure the native dependencies for your selected renderer/backend are available in `bin`:
   * Windows: `freetype6.dll` and `glfw3.dll`
   * macOS: `libfreetype.6.dylib` and `libglfw.3.dylib`
   * Linux: `libfreetype.6.so` and `libglfw.3.so`
3. Build the project:
   ```powershell
   dotnet build
   ```
4. Run the demo or reference the library in your project

## Documentation

For detailed usage information, refer to the source files in the `src/` directory and additional documentation files in the `docs/` directory.

## License

MIT License - See [LICENSE](LICENSE) file for details.

Copyright © 2026 Marijn Herrebout