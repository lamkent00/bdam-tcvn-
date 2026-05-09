namespace BDAM.Core.Layers;

public sealed class InMemoryLayerStyleWriter : ILayerStyleWriter
{
    private readonly Dictionary<string, LayerStyleDefinition> layers = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> textStyles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> dimensionStyles = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<LayerStyleDefinition> Layers => layers.Values.ToArray();

    public IReadOnlyCollection<string> TextStyles => textStyles.ToArray();

    public IReadOnlyCollection<string> DimensionStyles => dimensionStyles.ToArray();

    public bool LayerExists(string layerName)
    {
        return layers.ContainsKey(layerName);
    }

    public void CreateLayer(LayerStyleDefinition definition)
    {
        layers.Add(definition.LayerName, definition);
    }

    public bool TextStyleExists(string textStyleName)
    {
        return textStyles.Contains(textStyleName);
    }

    public void CreateTextStyle(string textStyleName)
    {
        textStyles.Add(textStyleName);
    }

    public bool DimensionStyleExists(string dimensionStyleName)
    {
        return dimensionStyles.Contains(dimensionStyleName);
    }

    public void CreateDimensionStyle(string dimensionStyleName)
    {
        dimensionStyles.Add(dimensionStyleName);
    }
}
