using BDAM.Core.Commands;

namespace BDAM.Core.Tests;

public sealed class CommandRegistryTests
{
    [Fact]
    public void DefaultRegistryContainsAllLockedCommands()
    {
        var registry = new BdamCommandRegistry();

        Assert.All(BdamCommandIds.All, commandId => Assert.True(registry.TryGet(commandId, out _)));
    }

    [Fact]
    public void DefaultRegistryHasNoDuplicateCommandIds()
    {
        var registry = new BdamCommandRegistry();
        var ids = registry.All.Select(definition => definition.Id).ToArray();

        Assert.Equal(ids.Length, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
