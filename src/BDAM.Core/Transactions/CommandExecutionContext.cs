using BDAM.Core.Commands;
using BDAM.Core.Configuration;
using BDAM.Core.Diagnostics;
using BDAM.Core.Layers;

namespace BDAM.Core.Transactions;

public sealed class CommandExecutionContext
{
    public CommandExecutionContext(
        BdamCommandDefinition command,
        BdamPluginConfig config,
        LayerStyleCatalog layerStyles,
        IEditorMessenger messenger)
    {
        Command = command;
        Config = config;
        LayerStyles = layerStyles;
        Messenger = messenger;
    }

    public BdamCommandDefinition Command { get; }

    public BdamPluginConfig Config { get; }

    public LayerStyleCatalog LayerStyles { get; }

    public IEditorMessenger Messenger { get; }
}
