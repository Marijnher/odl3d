namespace odl3d.Renderer;

public sealed record ColorAttachmentDescription
{
    public LoadAction LoadAction { get; init; } = LoadAction.Clear;

    public StoreAction StoreAction { get; init; } = StoreAction.Store;

    public Color ClearColor { get; init; } = Color.Black;
}