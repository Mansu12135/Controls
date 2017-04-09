using System.Transactions;

namespace ApplicationLayer
{
    internal static class TransactionActionHelper
    {
        public static bool DoActionWithCheckOnTransaction(CheckExpression action, ref string message)
        {
            if (Transaction.Current == null)
            {
                using (var transaction = new TransactionScope())
                {
                    if (!action.Invoke(ref message))
                    {
                        Transaction.Current.Rollback();
                        return false;
                    }
                    transaction.Complete();
                }
                return true;
            }
            return action.Invoke(ref message);
        }

        public static bool CheckConditions(CheckExpression checkExpression, ref string message)
        {
            if (!checkExpression.Invoke(ref message))
            {
                if (Transaction.Current != null)
                {
                    Transaction.Current.Rollback();
                }
                return false;
            }
            return true;
        }

        public delegate bool CheckExpression(ref string message);
    }
}
