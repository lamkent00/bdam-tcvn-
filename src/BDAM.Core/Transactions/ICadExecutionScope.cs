namespace BDAM.Core.Transactions;

public interface ICadExecutionScope
{
    ICadDocumentLock BeginDocumentLock();

    ICadTransaction BeginTransaction();
}
