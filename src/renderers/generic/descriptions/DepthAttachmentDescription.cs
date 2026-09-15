namespace odl3d.Renderer;

public sealed record DepthAttachmentDescription
{
    public LoadAction LoadAction { get; init; } = LoadAction.Clear;

    public StoreAction StoreAction { get; init; } = StoreAction.DontCare;

    public float ClearDepth { get; init; } = 1.0f;
}