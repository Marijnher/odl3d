using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class CommandQueue : ObjCObject
    {
        public string? Label
        {
            get => GetStringOrNull("label");
            set => Send("setLabel:" , value);
        }

        public CommandQueue(IntPtr handle, bool ownsNativeObject = false) : base(handle, ownsNativeObject) { }

        public CommandBuffer CreateCommandBuffer() => Get<CommandBuffer>("commandBuffer");
    }
}