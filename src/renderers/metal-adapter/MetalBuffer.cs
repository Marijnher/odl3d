using System;

namespace odl3d.Renderer.MetalAdapter;

public abstract class MetalBuffer
{
    public int Size { get; protected set; }
    public BufferUsage Usage { get; protected set; }
    public BufferType Type { get; protected set; }
    public bool Disposed { get; protected set; }

    public abstract Metal.Buffer Buffer { get; }

    public void Dispose()
    {
        if (Disposed) return;
         Buffer.Dispose();
         Disposed = true;
    }
}

public class MetalBuffer<T> : MetalBuffer, IBuffer<T> where T : unmanaged
{
    public override Metal.Buffer Buffer { get; }

    public MetalBuffer(Metal.Device device, BufferDescription description, T[]? initialData = null)
    {
        Size = description.Size;
        Usage = description.Usage;
        if (initialData != null && initialData.Length != Size) throw new RenderException("Provided data is not the same size as the buffer.");
        Buffer = device.CreateBuffer(initialData ?? new T[description.Size]);
    }
    
    public void SetData(T[] data)
    {
        if (data.Length != Size) throw new RenderException("Provided data is not the same size as the buffer.");
        Buffer.Update(data);
    }
}