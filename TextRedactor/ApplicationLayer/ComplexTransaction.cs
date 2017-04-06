using System;
using System.Collections.Generic;
using System.Transactions;

namespace ApplicationLayer
{
    internal class ComplexTransaction
    {
        private List<TransactedOperation> Operations = new List<TransactedOperation>();

        public void AddOperation(Action mainAction, Action rollback)
        {
            Operations.Add(new TransactedOperation(mainAction, rollback, TransactionCompleted));
        }

        public void AddOperation(Action mainAction, List<string> paths)
        {
            Operations.Add(new RemovedOperation(mainAction, RemoveSnapshotManager.GetSnapshot(paths), TransactionCompleted));
        }

        private void TransactionCompleted(object sender, TransactionEventArgs transactionEventArgs)
        {
            if (transactionEventArgs.Transaction.TransactionInformation.Status == TransactionStatus.Aborted)
            {
                int index = Operations.FindIndex(
                    operation => operation.Inforamtion == transactionEventArgs.Transaction.TransactionInformation);
                for (int i = 0; i < index; i++)
                {
                    Operations[i].DoRollBack();
                }
            }
        }

        public void DoOperation()
        {
            Operations.ForEach(operation => operation.DoActionTransacted());

        }
    }
}
