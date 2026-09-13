# odl3d - 3D Graphics Library

A .NET 3D graphics library with an abstract renderer stack and GLFW-based windowing for cross-platform game development and graphics applications.

## Overview

odl3d is a C# graphics library designed to simplify 3D game development and graphics programming. It provides a unified API for handling 3D rendering, scene management, shaders, and asset management across Windows, Linux, and macOS.

## Architecture

Applications create a window and renderer, add drawable objects to 2D or 3D scenes, and render those scenes through the active backend.

![architecture.png](architecture.png)

## Dependencies

- **GLFW3** - Window management and native window/context creation
- **Freetype** - Font rendering

## Building

### Prerequisites
- GLFW3
- Freetype

### Build Commands

```powershell
# Build odl3d
dotnet build

# Release build
dotnet build -c Release
```

## License

MIT License - See [LICENSE](odl2d/LICENSE) file for details.

Copyright © 2026 Marijn Herrebout

## Getting Started

1. Clone the repository
2. Ensure the native dependencies for your selected renderer/backend are installed and available (for example GLFW3)
3. Build the project:
   ```powershell
   dotnet build
   ```
4. Run the demo or reference the library in your project

## Documentation

For detailed usage information, refer to the source files in the `src/` directory