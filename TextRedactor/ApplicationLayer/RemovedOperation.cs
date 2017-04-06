using System;
using System.Collections.Generic;
using System.IO;
using System.Transactions;

namespace ApplicationLayer
{
    internal class RemovedOperation : TransactedOperation
    {
        private static Dictionary<string, byte[]> RollBackCollection;

        public RemovedOperation(Action doAction, Dictionary<string, byte[]> deletedObject, TransactionCompletedEventHandler handler) : base(doAction, () =>
        {
            foreach (KeyValuePair<string, byte[]> action in RollBackCollection)
            {
                if (action.Value == null)
                {
                    Directory.CreateDirectory(action.Key);
                    continue;
                }
                using (var stream = File.Create(action.Key, action.Value.Length))
                {
                    stream.Write(action.Value, 0, action.Value.Length);
                }
            }
        }, handler)
        {
            RollBackCollection = deletedObject;
        }
    }
}
