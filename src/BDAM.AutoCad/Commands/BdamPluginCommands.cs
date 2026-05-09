using Autodesk.AutoCAD.Runtime;
using BDAM.AutoCad.Infrastructure;
using BDAM.Core.Commands;
using BDAM.Core.Configuration;
using BDAM.Core.Diagnostics;
using BDAM.Core.Layers;
using BDAM.Core.Transactions;

namespace BDAM.AutoCad.Commands;

public sealed class BdamPluginCommands
{
    private readonly BdamCommandRegistry registry = new();
    private readonly BdamPluginConfig config = BdamPluginConfig.CreateDefault();
    private readonly LayerStyleCatalog layerStyles = new();

    [CommandMethod(BdamCommandIds.DrawBeam)]
    public void BdamTcvn()
    {
        ExecuteStub(BdamCommandIds.DrawBeam);
    }

    [CommandMethod(BdamCommandIds.AnnotateSupplementaryRebar)]
    public void Gt()
    {
        ExecuteStub(BdamCommandIds.AnnotateSupplementaryRebar);
    }

    [CommandMethod(BdamCommandIds.CreateCadStatisticsTable)]
    public void Tktd()
    {
        ExecuteStub(BdamCommandIds.CreateCadStatisticsTable);
    }

    [CommandMethod(BdamCommandIds.ExportToExcel)]
    public void Xte()
    {
        ExecuteStub(BdamCommandIds.ExportToExcel);
    }

    private void ExecuteStub(string commandId)
    {
        var messenger = new InMemoryEditorMessenger();
        var executor = new CadCommandScopeExecutor(
            registry,
            config,
            layerStyles,
            new InMemoryCadExecutionScope(),
            messenger);

        executor.Execute(commandId, context =>
        {
            var writer = new NoOpLayerStyleWriter();
            var layerService = new LayerStyleService(context.LayerStyles, writer);
            var result = layerService.EnsureStandards();
            context.Messenger.WriteInfo($"{context.Command.Id} foundation stub is ready. Created standards: {result.CreatedAnything}.");
        });
    }
}
