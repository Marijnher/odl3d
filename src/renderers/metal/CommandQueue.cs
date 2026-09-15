using System;

namespace odl3d.Renderers;

public static partial class MetalNew
{
    public sealed class CommandQueue : ObjCObject
    {
        public string? Label
        {
            get => GetStringOrNull("label");
            set => SendString("setLabel:" , value);
        }
        public CommandBuffer CommandBuffer => Get<CommandBuffer>("commandBuffer");

        public CommandQueue(IntPtr handle) : base(handle) { }
    }
}