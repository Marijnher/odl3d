# ODL3D - 3D Graphics Library

A .NET 3D graphics library with an abstract renderer stack and GLFW-based windowing for cross-platform game development and graphics applications.

## Overview

ODL3D is a C# graphics library designed to simplify 3D game development and graphics programming. It provides a unified API for handling 3D rendering, scene management, shaders, and asset management across Windows, Linux, and macOS.

## Key Features

- **3D mesh rendering** with OBJ model loading
- **Renderer-agnostic shader support**
- **3D camera and perspective** management
- **Sprite3D objects** for billboard rendering
- **Scene management** (2D and 3D scenes)
- **Texture mapping** and asset management
- **Cross-platform support** (Windows, Linux, macOS)

**Target Framework:** .NET 10.0

## Dependencies

- **GLFW3** - Window management and native window/context creation
- **Renderer backends** - Swap in the renderer implementation you need for your app

## Building

### Prerequisites
- .NET 10.0 SDK or later
- Platform-specific native libraries required by the selected renderer backend (for example GLFW3)

### Build Commands

```powershell
# Build odl3d
dotnet build odl3d/odl3d.csproj

# Release build
dotnet build odl3d/odl3d.csproj -c Release
```

## License

MIT License - See [LICENSE](odl2d/LICENSE) file for details.

Copyright © 2026 Marijn Herrebout

## Getting Started

1. Clone the repository
2. Ensure the native dependencies for your selected renderer/backend are installed and available (for example GLFW3)
3. Build the project:
   ```powershell
   dotnet build odl3d/odl3d.csproj
   ```
4. Run the demo or reference the library in your project

## Documentation

For detailed usage information, refer to the source files in the `src/` directory