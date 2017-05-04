using System;
using System.Transactions;

namespace ApplicationLayer
{
    internal class TransactedOperation
    {
        private Action MainAction { get;  set; }

        private Action RollBackAction { get; set; }

        private TransactionCompletedEventHandler Handler { get; set; }

        internal TransactionInformation Inforamtion { get; private set; }

        public TransactedOperation(Action doAction, Action rollBackAction, TransactionCompletedEventHandler handler)
        {
            MainAction = doAction;
            RollBackAction = rollBackAction;
            Handler = handler;
        }

        private void AttachEventHandler()
        {
            DettachEventHandler();
            Transaction.Current.TransactionCompleted += Handler;
        }

        private void DettachEventHandler()
        {
            Transaction.Current.TransactionCompleted -= Handler;
        }

        public bool DoActionTransacted()
        {
            bool isSuccess = false;
            TransactionScope transaction = null;
            try
            {
                transaction = new TransactionScope();
                Inforamtion = Transaction.Current.TransactionInformation;
                AttachEventHandler();
                MainAction.Invoke();
            }
            catch (Exception e)
            {
                Transaction.Current.Rollback();
            }
            finally
            {
                if (Transaction.Current != null &&
                    Transaction.Current.TransactionInformation.Status != TransactionStatus.Aborted)
                {
                    transaction.Complete();
                    transaction.Dispose();
                    transaction = null;
                    isSuccess = true;
                }
            }
            return isSuccess;
    }

        public void DoRollBack()
        {
            using (var transaction = new TransactionScope())
            {
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
