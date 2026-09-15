using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class Device : ObjCObject
    {
        public Device(IntPtr handle) : base(handle) { }

        public string? Name => GetString("name");
        public string Description => GetString("description");
        public ulong RegistryID => GetUInt64("registryID");

        public CommandQueue NewCommandQueue() => Get<CommandQueue>("newCommandQueue");

        public RenderPassDescriptor NewRenderPassDescriptor() => RenderPassDescriptor.Create();
    }
}