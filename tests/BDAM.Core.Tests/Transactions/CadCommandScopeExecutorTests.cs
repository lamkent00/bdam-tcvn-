using BDAM.Core.Commands;
using BDAM.Core.Configuration;
using BDAM.Core.Diagnostics;
using BDAM.Core.Layers;
using BDAM.Core.Transactions;

namespace BDAM.Core.Tests.Transactions;

public sealed class CadCommandScopeExecutorTests
{
    [Fact]
    public void ExecuteCommitsTransactionOnSuccess()
    {
        var scope = new InMemoryCadExecutionScope();
        var executor = CreateExecutor(scope);

        var result = executor.Execute(BdamCommandIds.DrawBeam, _ => { });

        Assert.Equal(CommandExecutionStatus.Succeeded, result.Status);
        Assert.Single(scope.Transactions);
        Assert.True(scope.Transactions[0].IsCommitted);
        Assert.Equal(1, scope.DocumentLockCount);
    }

    [Fact]
    public void ExecuteRollsBackTransactionOnError()
    {
        var scope = new InMemoryCadExecutionScope();
        var executor = CreateExecutor(scope);

        var result = executor.Execute(BdamCommandIds.DrawBeam, _ => throw new InvalidOperationException("boom"));

        Assert.Equal(CommandExecutionStatus.Failed, result.Status);
        Assert.Single(scope.Transactions);
        Assert.True(scope.Transactions[0].IsRolledBack);
        Assert.Equal(1, scope.DocumentLockCount);
    }

    private static CadCommandScopeExecutor CreateExecutor(InMemoryCadExecutionScope scope)
    {
        return new CadCommandScopeExecutor(
            new BdamCommandRegistry(),
            BdamPluginConfig.CreateDefault(),
            new LayerStyleCatalog(),
            scope,
            new InMemoryEditorMessenger());
    }
}
