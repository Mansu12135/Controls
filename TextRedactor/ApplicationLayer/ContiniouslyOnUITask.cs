using System;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationLayer
{
    public class ContiniouslyOnUITask : Task
    {
        private Action UITask;
        //private Task ContiniouslyTask;
        public ContiniouslyOnUITask(Action task, Action UItask) : base(task)
        {
            UITask = UItask;
        }

        public new void Start()
        {
            ((Task)this).Start();
           /* ContiniouslyTask =*/ ContinueWith((obj)=>{ Application.Current.Dispatcher.Invoke(UITask); });
        }

        //public new void Wait()
        //{
        //    ((Task)this).Wait();
        //    ContiniouslyTask.Wait();
        //}
    }
}
