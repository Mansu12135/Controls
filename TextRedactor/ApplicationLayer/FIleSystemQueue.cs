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
                string message = "";
                task.DoFeedBack(DoParseAndDo(task, ref message), message);
                CallStack.RemoveAt(0);
            }
        }

        private bool Do(Happened happened, bool isFolder, string path, ref string message)
        {
            switch (happened)
            {
                case Happened.Created:
                {
                    return isFolder
                        ? TransactionDirectory.CreateDirectory(path, ref message)
                        : TransactionFile.CreateFile(path, ref message);
                }
                case Happened.Deleted:
                    {
                        return isFolder
                        ? TransactionDirectory.RemoveDirectory(path, ref message)
                        : TransactionFile.DeleteFile(path, ref message);
                    }
            }
            return false;
        }

        private bool DoParseAndDo(FileSystemTask task, ref string message)
        {

            if (task.IsFolder == true)
            {
                var args = task.Args as ProjectArgs;
                if (args == null)
                {
                    message = "Invalid cast";
                    return false;
                }
                RenamedArgs renamedArgs = args.RenamedArgs;
                if (renamedArgs != null)
                {
                    if (!TransactionDirectory.MoveDirectory(renamedArgs.From, renamedArgs.To, ref message)) { return false; }
                }
                return Do(args.Happened, true, args.Project, ref message);
            }
            else if (task.IsFolder == false)
            {
                bool resposne = true;
                var args = task.Args as FileArgs;
                if (args == null)
                {
                    message = "Invalid cast";
                    return false;
                }
                RenamedArgs renamedArgs = args.RenamedArgs;
                if (renamedArgs != null)
                {
                    if (!TransactionFile.MoveFile(renamedArgs.From, renamedArgs.To, ref message)) { return false; }
                }
                foreach (string file in args.Files)
                {
                    resposne |= Do(args.Happened, true, file, ref message);
                }
                return resposne;
            }
            message = "Invalid parameter";
            return false;
        }

        public void Dispose()
        {
            Work = false;
        }
    }
}
