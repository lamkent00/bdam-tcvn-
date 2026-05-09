using BDAM.Core.Layers;

namespace BDAM.AutoCad.Infrastructure;

public sealed class NoOpLayerStyleWriter : ILayerStyleWriter
{
    private readonly InMemoryLayerStyleWriter inner = new();

    public bool LayerExists(string layerName)
    {
        return inner.LayerExists(layerName);
    }

    public void CreateLayer(LayerStyleDefinition definition)
    {
        inner.CreateLayer(definition);
    }

    public bool TextStyleExists(string textStyleName)
    {
        return inner.TextStyleExists(textStyleName);
    }

    public void CreateTextStyle(string textStyleName)
    {
        inner.CreateTextStyle(textStyleName);
    }

    public bool DimensionStyleExists(string dimensionStyleName)
    {
        return inner.DimensionStyleExists(dimensionStyleName);
    }

    public void CreateDimensionStyle(string dimensionStyleName)
    {
        inner.CreateDimensionStyle(dimensionStyleName);
    }
}
