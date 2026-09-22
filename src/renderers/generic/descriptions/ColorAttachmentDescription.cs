namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a color attachment in a rendering pipeline, including its load and store actions and clear color.
/// </summary>
public sealed record ColorAttachmentDescription
{
    /// <summary>
    /// The load action to be performed on the color attachment at the beginning of a rendering pass.
    /// </summary>
    public LoadAction LoadAction { get; init; } = LoadAction.Clear;

    /// <summary>
    /// The store action to be performed on the color attachment at the end of a rendering pass.
    /// </summary>
    public StoreAction StoreAction { get; init; } = StoreAction.Store;

    /// <summary>
    /// The clear color to be used if the load action is set to clear.
    /// </summary>
    public Color ClearColor { get; init; } = Color.Black;
}