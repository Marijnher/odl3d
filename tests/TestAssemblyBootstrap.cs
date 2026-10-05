using System.Reflection;
using System.Runtime.Loader;

namespace odl3d.Tests;

public abstract class TestBase
{
    static TestBase()
    {
        AssemblyLoadContext.Default.Resolving += ResolveOdl3d;
    }

    private static Assembly? ResolveOdl3d(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        if (!string.Equals(assemblyName.Name, "odl3d", StringComparison.OrdinalIgnoreCase))
            return null;

        string? testAssemblyPath = typeof(TestBase).Assembly.Location;
        if (string.IsNullOrEmpty(testAssemblyPath))
            return null;

        string? testDirectory = Path.GetDirectoryName(testAssemblyPath);
        if (testDirectory == null)
            return null;

        string libraryPath = Path.Combine(testDirectory, "odl3d.dll");
        return File.Exists(libraryPath)
            ? context.LoadFromAssemblyPath(libraryPath)
            : null;
    }
}