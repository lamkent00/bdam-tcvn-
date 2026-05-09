using BDAM.Core.Configuration;

namespace BDAM.Core.Tests.Configuration;

public sealed class BdamPluginConfigTests
{
    [Fact]
    public void DefaultsMatchLockedBusinessValues()
    {
        var config = BdamPluginConfig.CreateDefault();

        Assert.Equal(4, config.SupportZoneDivisor);
        Assert.Equal(50, config.StirrupEdgeDistanceMm);
        Assert.Equal(11700, config.MaxStockLengthMm);
        Assert.Equal(40, config.LapSpliceDiameterMultiplier);
    }

    [Fact]
    public void DefaultConfigurationIsValid()
    {
        var config = BdamPluginConfig.CreateDefault();

        Assert.Empty(config.Validate());
    }
}
