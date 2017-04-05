using System;
using System.Transactions;

namespace ApplicationLayer
{
    internal class TransactedOperation
    {
        private Action MainAction { get; }

        private Action RollBackAction { get; }

        private TransactionCompletedEventHandler Handler { get; }

        internal TransactionInformation Inforamtion { get; private set; }

        public TransactedOperation(Action doAction, Action rollBackAction, TransactionCompletedEventHandler handler)
        {
            MainAction = doAction;
            RollBackAction = rollBackAction;
            Handler = handler;
        }

        private void AttachEventHandler()
        {
            Transaction.Current.TransactionCompleted += Handler;
        }

        private void DettachEventHandler()
        {
            Transaction.Current.TransactionCompleted -= Handler;
        }

        public void DoActionTransacted()
        {
            using (var transaction = new TransactionScope())
            {
                Inforamtion = Transaction.Current.TransactionInformation;
                AttachEventHandler();
                try
                {
                    MainAction.Invoke();
                }
                catch (Exception e)
                {
                    Transaction.Current.Rollback();
                }
                transaction.Complete();
            }
        }

        public void DoRollBack()
        {
            using (var transaction = new TransactionScope())
            {
                AttachEventHandler();
                try
                {
                    RollBackAction.Invoke();
                }
                catch (Exception e)
                {
                    Transaction.Current.Rollback();
                }
                transaction.Complete();
            }
        }
    }
}
