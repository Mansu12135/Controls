using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace ApplicationLayer
{
    internal class FileSystemTask: IDisposable
    {
        private Action<bool, string> CallBack;
        private Dispatcher ThreadDispatcher;

        internal string Path;

        internal bool? IsFolder;

        internal EventArgs Args;

        internal Priority Priority { get; private set; }

        public FileSystemTask(string path, EventArgs args, Action<bool, string> callBack, Thread currentThread, Priority priority = Priority.Normal)
        {
            Path = path;
            Args = args;
            Priority = priority;
            CallBack = callBack;
            ThreadDispatcher = Dispatcher.FromThread(currentThread);
            if (args is ProjectArgs)
            {
                IsFolder = true;
                return;
            }
            if (args is FileArgs)
            {
                IsFolder = false;
                return;
            }
        }

        internal void DoFeedBack(bool rezult, string message)
        {
            ThreadDispatcher.Invoke(() => CallBack.Invoke(rezult, message));
            Dispose();
        }

        public void Dispose()
        {
            CallBack = null;
            ThreadDispatcher = null;
            Path = null;
            IsFolder = null;
            Args = null;
            Priority = 0;
        }
    }

    public enum Priority
    {
        High,
        Normal,
        Low
    }
}
