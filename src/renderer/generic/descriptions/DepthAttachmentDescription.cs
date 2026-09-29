namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a depth attachment in a rendering pipeline, including its load and store actions and clear depth value.
/// </summary>
public sealed record DepthAttachmentDescription
{
    /// <summary>
    /// The load action to be performed on the depth attachment at the beginning of a rendering pass.
    /// </summary>
    public LoadAction LoadAction { get; init; } = LoadAction.Clear;

    /// <summary>
    /// The store action to be performed on the depth attachment at the end of a rendering pass.
    /// </summary>
    public StoreAction StoreAction { get; init; } = StoreAction.DontCare;

    /// <summary>
    /// The clear depth value to be used if the load action is set to clear.
    /// </summary>
    public float ClearDepth { get; init; } = 1.0f;
}