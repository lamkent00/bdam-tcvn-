using BDAM.Core.Layers;

namespace BDAM.Core.Tests.Layers;

public sealed class LayerStyleServiceTests
{
    [Fact]
    public void EnsureStandardsCreatesLayersAndStylesOnlyOnce()
    {
        var writer = new InMemoryLayerStyleWriter();
        var service = new LayerStyleService(new LayerStyleCatalog(), writer);

        var first = service.EnsureStandards();
        var second = service.EnsureStandards();

        Assert.True(first.CreatedAnything);
        Assert.False(second.CreatedAnything);
        Assert.Equal(Enum.GetValues<CadLayerPurpose>().Length, writer.Layers.Count);
        Assert.Single(writer.TextStyles);
        Assert.Single(writer.DimensionStyles);
    }
}
