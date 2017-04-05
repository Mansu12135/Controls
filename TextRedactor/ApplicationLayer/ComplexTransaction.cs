using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ApplicationLayer
{
    internal class ComplexTransaction
    {
        private List<TransactedOperation> Operations = new List<TransactedOperation>();

        public void AddOperation(Action doAction, Action rollback)
        {
            Operations.Add(new TransactedOperation(doAction, rollback, TransactionCompleted));
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
