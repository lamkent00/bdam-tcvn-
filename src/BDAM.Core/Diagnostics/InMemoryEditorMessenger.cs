namespace BDAM.Core.Diagnostics;

public sealed class InMemoryEditorMessenger : IEditorMessenger
{
    private readonly List<string> messages = [];

    public IReadOnlyList<string> Messages => messages;

    public void WriteInfo(string message)
    {
        messages.Add($"INFO: {message}");
    }

    public void WriteWarning(string message)
    {
        messages.Add($"WARN: {message}");
    }

    public void WriteError(string message, Exception? exception = null)
    {
        var suffix = exception is null ? string.Empty : $" ({exception.GetType().Name}: {exception.Message})";
        messages.Add($"ERROR: {message}{suffix}");
    }
}
