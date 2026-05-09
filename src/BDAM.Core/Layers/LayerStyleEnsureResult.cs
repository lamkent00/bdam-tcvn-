namespace BDAM.Core.Layers;

public sealed record LayerStyleEnsureResult(
    IReadOnlyList<string> CreatedLayers,
    IReadOnlyList<string> CreatedTextStyles,
    IReadOnlyList<string> CreatedDimensionStyles)
{
    public bool CreatedAnything => CreatedLayers.Count > 0 || CreatedTextStyles.Count > 0 || CreatedDimensionStyles.Count > 0;
}
