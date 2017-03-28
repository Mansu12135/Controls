using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ApplicationLayer
{
    internal class FileSystemQueue : IDisposable
    {
        private List<FileSystemTask> CallStack;
        private Thread QueueThread;
        private bool Work = true;

        public FileSystemQueue()
        {
            CallStack = new List<FileSystemTask>();
            QueueThread = new Thread(StartWork);
            QueueThread.Start();
        }

        internal void AddTask(string path, EventArgs args, Action<bool, string> callback, Priority priority = Priority.Normal)
        {
            CallStack.Add(new FileSystemTask(path,args, callback, Thread.CurrentThread, priority));
        }

        private void StartWork()
        {
            while (Work || CallStack.Any())
            {
                if (!CallStack.Any()) { continue; }
                FileSystemTask task = CallStack[0];

                task.DoFeedBack(true, "");
                CallStack.RemoveAt(0);
            }
        }

        public void Dispose()
        {
            Work = false;
        }
    }
}
