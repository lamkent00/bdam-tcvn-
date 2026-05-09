namespace BDAM.Core.Layers;

public sealed class LayerStyleCatalog
{
    private readonly IReadOnlyDictionary<CadLayerPurpose, LayerStyleDefinition> definitions;

    public LayerStyleCatalog(IEnumerable<LayerStyleDefinition>? definitions = null)
    {
        var source = definitions?.ToArray() ?? CreateDefaultDefinitions();
        this.definitions = source.ToDictionary(definition => definition.Purpose);
    }

    public IReadOnlyCollection<LayerStyleDefinition> All => definitions.Values.OrderBy(definition => definition.Purpose).ToArray();

    public LayerStyleDefinition GetRequired(CadLayerPurpose purpose)
    {
        if (!definitions.TryGetValue(purpose, out var definition))
        {
            throw new KeyNotFoundException($"Layer style for purpose '{purpose}' is not registered.");
        }

        return definition;
    }

    private static LayerStyleDefinition[] CreateDefaultDefinitions()
    {
        return
        [
            new(CadLayerPurpose.BeamOutline, "BDAM_BEAM", 8, "Continuous"),
            new(CadLayerPurpose.RebarMain, "BDAM_REBAR_MAIN", 1, "Continuous"),
            new(CadLayerPurpose.RebarExtra, "BDAM_REBAR_EXTRA", 30, "Continuous"),
            new(CadLayerPurpose.Stirrup, "BDAM_STIRRUP", 3, "Continuous"),
            new(CadLayerPurpose.Leader, "BDAM_LEADER", 2, "Continuous", TextStyleName: "BDAM_TEXT"),
            new(CadLayerPurpose.Text, "BDAM_TEXT", 7, "Continuous", TextStyleName: "BDAM_TEXT"),
            new(CadLayerPurpose.Dimension, "BDAM_DIM", 4, "Continuous", DimensionStyleName: "BDAM_DIM"),
            new(CadLayerPurpose.StatisticsTable, "BDAM_TABLE", 5, "Continuous", TextStyleName: "BDAM_TEXT"),
        ];
    }
}
