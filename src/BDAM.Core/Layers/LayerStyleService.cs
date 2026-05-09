namespace BDAM.Core.Layers;

public sealed class LayerStyleService
{
    private readonly LayerStyleCatalog catalog;
    private readonly ILayerStyleWriter writer;

    public LayerStyleService(LayerStyleCatalog catalog, ILayerStyleWriter writer)
    {
        this.catalog = catalog;
        this.writer = writer;
    }

    public LayerStyleEnsureResult EnsureStandards()
    {
        var createdLayers = new List<string>();
        var createdTextStyles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var createdDimensionStyles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in catalog.All)
        {
            if (!writer.LayerExists(definition.LayerName))
            {
                writer.CreateLayer(definition);
                createdLayers.Add(definition.LayerName);
            }

            if (definition.TextStyleName is not null && !writer.TextStyleExists(definition.TextStyleName))
            {
                writer.CreateTextStyle(definition.TextStyleName);
                createdTextStyles.Add(definition.TextStyleName);
            }

            if (definition.DimensionStyleName is not null && !writer.DimensionStyleExists(definition.DimensionStyleName))
            {
                writer.CreateDimensionStyle(definition.DimensionStyleName);
                createdDimensionStyles.Add(definition.DimensionStyleName);
            }
        }

        return new LayerStyleEnsureResult(
            createdLayers,
            createdTextStyles.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            createdDimensionStyles.Order(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    public LayerStyleDefinition GetDefinition(CadLayerPurpose purpose)
    {
        return catalog.GetRequired(purpose);
    }
}
