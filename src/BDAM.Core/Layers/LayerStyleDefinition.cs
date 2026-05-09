namespace BDAM.Core.Layers;

public sealed record LayerStyleDefinition(
    CadLayerPurpose Purpose,
    string LayerName,
    short ColorIndex,
    string LineType,
    string? TextStyleName = null,
    string? DimensionStyleName = null);
