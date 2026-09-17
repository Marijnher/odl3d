using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public sealed class VertexLayoutDescriptorArray : ObjCObject
    {
        public VertexLayoutDescriptorArray(IntPtr handle) : base(handle) { }

        public VertexLayoutDescriptor Get(int index) =>
            Send<VertexLayoutDescriptor>("objectAtIndexedSubscript:", index);

        public VertexLayoutDescriptor this[int index] => Get(index);
    }

    public sealed class VertexLayoutDescriptor : ObjCObject
    {
        public uint Stride
        {
            get => GetUInt32("stride");
            set => Send("setStride:", value);
        }

        public StepMode StepFunction
        {
            get => GetUInt32("stepFunction") switch
            {
                1 => StepMode.PerVertex,
                2 => StepMode.PerInstance,
                uint other => throw new RenderException($"Unsupported step function: {other}")
            };
            set => Send("setStepFunction:", value switch
            {
                StepMode.PerVertex => 1,
                StepMode.PerInstance => 2,
                _ => throw new RenderException($"Unsupported step function: {value}")
            });
        }

        public VertexLayoutDescriptor(IntPtr handle) : base(handle) { }
    }
}