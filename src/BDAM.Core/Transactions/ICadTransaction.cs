namespace BDAM.Core.Transactions;

public interface ICadTransaction : IDisposable
{
    bool IsCompleted { get; }

    void Commit();

    void Rollback();
}
