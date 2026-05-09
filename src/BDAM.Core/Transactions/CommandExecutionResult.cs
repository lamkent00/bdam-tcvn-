namespace BDAM.Core.Transactions;

public sealed record CommandExecutionResult(
    string CommandId,
    CommandExecutionStatus Status,
    string Message,
    Exception? Exception = null)
{
    public bool IsSuccess => Status == CommandExecutionStatus.Succeeded;
}
