namespace BDAM.Core.Layers;

public interface ILayerStyleWriter
{
    bool LayerExists(string layerName);

    void CreateLayer(LayerStyleDefinition definition);

    bool TextStyleExists(string textStyleName);

    void CreateTextStyle(string textStyleName);

    bool DimensionStyleExists(string dimensionStyleName);

    void CreateDimensionStyle(string dimensionStyleName);
}
