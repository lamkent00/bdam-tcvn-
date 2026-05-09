namespace BDAM.Core.Transactions;

public sealed class InMemoryCadExecutionScope : ICadExecutionScope
{
    private readonly List<InMemoryCadTransaction> transactions = [];

    public int DocumentLockCount { get; private set; }

    public IReadOnlyList<InMemoryCadTransaction> Transactions => transactions;

    public ICadDocumentLock BeginDocumentLock()
    {
        DocumentLockCount++;
        return new InMemoryCadDocumentLock();
    }

    public ICadTransaction BeginTransaction()
    {
        var transaction = new InMemoryCadTransaction();
        transactions.Add(transaction);
        return transaction;
    }

    private sealed class InMemoryCadDocumentLock : ICadDocumentLock
    {
        public void Dispose()
        {
        }
    }
}

public sealed class InMemoryCadTransaction : ICadTransaction
{
    public bool IsCommitted { get; private set; }

    public bool IsRolledBack { get; private set; }

    public bool IsCompleted => IsCommitted || IsRolledBack;

    public void Commit()
    {
        ThrowIfCompleted();
        IsCommitted = true;
    }

    public void Rollback()
    {
        if (!IsCompleted)
        {
            IsRolledBack = true;
        }
    }

    public void Dispose()
    {
        Rollback();
    }

    private void ThrowIfCompleted()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("Transaction is already completed.");
        }
    }
}
