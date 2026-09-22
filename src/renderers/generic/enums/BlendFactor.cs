namespace odl3d.Renderer;

public enum BlendFactor
{
    Zero,
    One,
    SourceColor,
    OneMinusSourceColor,
    DestinationColor,
    OneMinusDestinationColor,

    SourceAlpha,
    OneMinusSourceAlpha,
    DestinationAlpha,
    OneMinusDestinationAlpha,
    
    BlendColor,
    OneMinusBlendColor,
    BlendAlpha,
    OneMinusBlendAlpha,
    SourceAlphaSaturated,

    Source1Color,
    OneMinusSource1Color,
    Source1Alpha,
    OneMinusSource1Alpha
}