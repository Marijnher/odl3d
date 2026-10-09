using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Owns the GPU buffers that carry a scene's lights to the shaders. The light buffer grows with the number of lights, up to the most that one shader block can address on the current device.
/// </summary>
internal sealed class SceneLighting : IDisposable
{
    /// <summary>
    /// Uniform slot of the <see cref="LightingShaderData"/> block.
    /// </summary>
    public const int HeaderSlot = 2;

    /// <summary>
    /// Uniform slot of the light list block.
    /// </summary>
    public const int ListSlot = 3;

    private const int InitialCapacity = 16;

    private readonly IRenderDevice _renderer;
    private readonly IBuffer<LightingShaderData> _headerBuffer;
    private readonly List<Light> _active = new();
    private IBuffer<LightShaderData> _listBuffer;
    private LightShaderData[] _packed;
    private LightShaderData[] _uploaded;
    private int _uploadedCount = -1;
    private LightingShaderData _uploadedHeader;
    private bool _headerUploaded;
    private bool _warnedAboutOverflow;
    private bool _disposed;

    /// <summary>
    /// The most lights a single draw can address on this device.
    /// </summary>
    public int MaxLights { get; }

    public SceneLighting(IRenderDevice renderer)
    {
        _renderer = renderer;
        MaxLights = GetMaxLights(renderer);
        int capacity = Math.Min(InitialCapacity, MaxLights);
        _packed = new LightShaderData[capacity];
        _uploaded = new LightShaderData[capacity];
        _headerBuffer = renderer.CreateBuffer<LightingShaderData>(new BufferDescription { Size = 1, Usage = BufferUsage.Uniform });
        _listBuffer = CreateListBuffer(capacity);
    }

    /// <summary>
    /// Gets the most lights a shader block can hold on the given device. The default shaders are compiled with this same value.
    /// </summary>
    public static int GetMaxLights(IRenderDevice renderer) =>
        Math.Max(1, renderer.Capabilities.MaxUniformBlockSize / Unsafe.SizeOf<LightShaderData>());

    private IBuffer<LightShaderData> CreateListBuffer(int capacity) =>
        _renderer.CreateBuffer<LightShaderData>(new BufferDescription { Size = capacity, Usage = BufferUsage.Uniform });

    /// <summary>
    /// Uploads the enabled lights and binds the lighting buffers. Data identical to the previous call is not uploaded again.
    /// </summary>
    /// <param name="pass">The render pass to bind to.</param>
    /// <param name="lights">The lights of the scene.</param>
    /// <param name="ambient">The ambient color.</param>
    /// <param name="cameraPosition">The world-space camera position.</param>
    /// <param name="sceneOffset">The position of the scene, applied to positional lights.</param>
    public void Bind(IRenderPass pass, IReadOnlyList<Light> lights, Vector3 ambient, Vector3 cameraPosition, Vector3 sceneOffset)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        CollectActive(lights, cameraPosition, sceneOffset);
        int count = _active.Count;
        EnsureCapacity(count);

        for (int i = 0; i < count; i++)
            _packed[i] = _active[i].ToShaderData(sceneOffset);

        if (count != _uploadedCount || !AsBytes(_packed, count).SequenceEqual(AsBytes(_uploaded, count)))
        {
            if (count > 0) _listBuffer.SetData(_packed, 0, count);
            Array.Copy(_packed, _uploaded, count);
            _uploadedCount = count;
        }

        LightingShaderData header = new()
        {
            CameraPosition = new Vector4(cameraPosition, 1f),
            Ambient = new Vector4(ambient, 1f),
            LightCount = (uint) count
        };
        if (!_headerUploaded || !HeaderBytes(in header).SequenceEqual(HeaderBytes(in _uploadedHeader)))
        {
            _headerBuffer.SetData([header]);
            _uploadedHeader = header;
            _headerUploaded = true;
        }

        pass.SetUniformBuffer(_headerBuffer, HeaderSlot);
        pass.SetUniformBuffer(_listBuffer, ListSlot);
    }

    private static ReadOnlySpan<byte> HeaderBytes(in LightingShaderData header) =>
        MemoryMarshal.AsBytes(new ReadOnlySpan<LightingShaderData>(in header));

    private static ReadOnlySpan<byte> AsBytes(LightShaderData[] data, int count) =>
        MemoryMarshal.AsBytes(data.AsSpan(0, count));

    /// <summary>
    /// Gathers the enabled lights. When there are more than a draw can hold, the directional lights and the lights contributing most at the camera are kept.
    /// </summary>
    private void CollectActive(IReadOnlyList<Light> lights, Vector3 cameraPosition, Vector3 sceneOffset)
    {
        _active.Clear();
        foreach (Light light in lights)
            if (light.Enabled) _active.Add(light);
        if (_active.Count <= MaxLights) return;

        if (!_warnedAboutOverflow)
        {
            Console.WriteLine($"Warning: the scene has {_active.Count} active lights but this renderer can use {MaxLights} at once; the strongest lights near the camera are used.");
            _warnedAboutOverflow = true;
        }
        float[] scores = new float[_active.Count];
        for (int i = 0; i < scores.Length; i++)
            scores[i] = _active[i].GetPriority(cameraPosition, sceneOffset);
        Light[] sorted = _active.ToArray();
        Array.Sort(scores, sorted);
        _active.Clear();
        for (int i = sorted.Length - 1; i >= sorted.Length - MaxLights; i--)
            _active.Add(sorted[i]);
    }

    /// <summary>
    /// Grows the CPU arrays and the GPU buffer so they can hold the given number of lights.
    /// </summary>
    private void EnsureCapacity(int count)
    {
        if (count <= _packed.Length) return;
        int capacity = _packed.Length;
        while (capacity < count) capacity *= 2;
        capacity = Math.Min(capacity, MaxLights);

        Array.Resize(ref _packed, capacity);
        Array.Resize(ref _uploaded, capacity);
        IBuffer<LightShaderData> previous = _listBuffer;
        _listBuffer = CreateListBuffer(capacity);
        previous.Dispose();
        _uploadedCount = -1;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _headerBuffer.Dispose();
        _listBuffer.Dispose();
        _disposed = true;
    }
}
