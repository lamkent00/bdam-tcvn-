namespace BDAM.Core.Diagnostics;

public interface IEditorMessenger
{
    void WriteInfo(string message);

    void WriteWarning(string message);

    void WriteError(string message, Exception? exception = null);
}
