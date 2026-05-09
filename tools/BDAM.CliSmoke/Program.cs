using BDAM.Core.Commands;
using BDAM.Core.Configuration;
using BDAM.Core.Diagnostics;
using BDAM.Core.Layers;
using BDAM.Core.Transactions;

var registry = new BdamCommandRegistry();
var config = BdamPluginConfig.CreateDefault();
var layerStyles = new LayerStyleCatalog();
var messenger = new InMemoryEditorMessenger();
var scope = new InMemoryCadExecutionScope();
var executor = new CadCommandScopeExecutor(registry, config, layerStyles, scope, messenger);

foreach (var commandId in BdamCommandIds.All)
{
    var result = executor.Execute(commandId, context =>
    {
        var writer = new InMemoryLayerStyleWriter();
        var service = new LayerStyleService(context.LayerStyles, writer);
        service.EnsureStandards();
    });

    Console.WriteLine($"{commandId}: {result.Status}");
}
