namespace odl3d.Renderer;

/// <summary>
/// Represents the different blend factors that can be used in blending operations.
/// </summary>
public enum BlendFactor
{
    /// <summary>
    /// Represents a blend factor of zero, meaning no contribution from the source or destination color.
    /// </summary>
    Zero,
    /// <summary>
    /// Represents a blend factor of one, meaning full contribution from the source or destination color.
    /// </summary>
    One,
    /// <summary>
    /// Represents a blend factor based on the source color.
    /// </summary>
    SourceColor,
    /// <summary>
    /// Represents a blend factor of one minus the source color.
    /// </summary>
    OneMinusSourceColor,
    /// <summary>
    /// Represents a blend factor based on the destination color.
    /// </summary>
    DestinationColor,
    /// <summary>
    /// Represents a blend factor of one minus the destination color.
    /// </summary>
    OneMinusDestinationColor,

    /// <summary>
    /// Represents a blend factor based on the source alpha.
    /// </summary>
    SourceAlpha,
    /// <summary>
    /// Represents a blend factor of one minus the source alpha.
    /// </summary>
    OneMinusSourceAlpha,
    /// <summary>
    /// Represents a blend factor based on the destination alpha.
    /// </summary>
    DestinationAlpha,
    /// <summary>
    /// Represents a blend factor of one minus the destination alpha.
    /// </summary>
    OneMinusDestinationAlpha,
    
    /// <summary>
    /// Represents a blend factor based on the blend color.
    /// </summary>
    BlendColor,
    /// <summary>
    /// Represents a blend factor of one minus the blend color.
    /// </summary>
    OneMinusBlendColor,
    /// <summary>
    /// Represents a blend factor based on the blend alpha.
    /// </summary>
    BlendAlpha,
    /// <summary>
    /// Represents a blend factor of one minus the blend alpha.
    /// </summary>
    OneMinusBlendAlpha,
    /// <summary>
    /// Represents a blend factor based on the source alpha saturated.
    /// </summary>
    SourceAlphaSaturated,

    /// <summary>
    /// Represents a blend factor based on the source1 color.
    /// </summary>
    Source1Color,
    /// <summary>
    /// Represents a blend factor of one minus the source1 color.
    /// </summary>
    OneMinusSource1Color,
    /// <summary>
    /// Represents a blend factor based on the source1 alpha.
    /// </summary>
    Source1Alpha,
    /// <summary>
    /// Represents a blend factor of one minus the source1 alpha.
    /// </summary>
    OneMinusSource1Alpha
}