using BDAM.Core.Commands;
using BDAM.Core.Configuration;
using BDAM.Core.Diagnostics;
using BDAM.Core.Layers;

namespace BDAM.Core.Transactions;

public sealed class CadCommandScopeExecutor
{
    private readonly BdamCommandRegistry commandRegistry;
    private readonly BdamPluginConfig config;
    private readonly LayerStyleCatalog layerStyles;
    private readonly ICadExecutionScope scope;
    private readonly IEditorMessenger messenger;

    public CadCommandScopeExecutor(
        BdamCommandRegistry commandRegistry,
        BdamPluginConfig config,
        LayerStyleCatalog layerStyles,
        ICadExecutionScope scope,
        IEditorMessenger messenger)
    {
        this.commandRegistry = commandRegistry;
        this.config = config;
        this.layerStyles = layerStyles;
        this.scope = scope;
        this.messenger = messenger;
    }

    public CommandExecutionResult Execute(string commandId, Action<CommandExecutionContext> handler)
    {
        var command = commandRegistry.GetRequired(commandId);
        var configErrors = config.Validate();
        if (configErrors.Count > 0)
        {
            var message = $"Invalid BDAM configuration: {string.Join("; ", configErrors)}";
            messenger.WriteError(message);
            return new CommandExecutionResult(command.Id, CommandExecutionStatus.Failed, message);
        }

        messenger.WriteInfo($"Starting command {command.Id}.");

        try
        {
            using var documentLock = scope.BeginDocumentLock();
            using var transaction = scope.BeginTransaction();
            var context = new CommandExecutionContext(command, config, layerStyles, messenger);
            handler(context);
            transaction.Commit();
            var message = $"Command {command.Id} completed.";
            messenger.WriteInfo(message);
            return new CommandExecutionResult(command.Id, CommandExecutionStatus.Succeeded, message);
        }
        catch (OperationCanceledException exception)
        {
            var message = $"Command {command.Id} cancelled.";
            messenger.WriteWarning(message);
            return new CommandExecutionResult(command.Id, CommandExecutionStatus.Cancelled, message, exception);
        }
        catch (Exception exception)
        {
            var message = $"Command {command.Id} failed and was rolled back.";
            messenger.WriteError(message, exception);
            return new CommandExecutionResult(command.Id, CommandExecutionStatus.Failed, message, exception);
        }
    }
}
