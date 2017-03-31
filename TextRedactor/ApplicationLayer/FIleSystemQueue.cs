using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ApplicationLayer
{
    internal class FileSystemQueue : IDisposable
    {
        private List<FileSystemTask> CallStack;
        private IFileSystemControl Control;
        private Thread QueueThread;
        private bool Work = true;

        public FileSystemQueue(IFileSystemControl control)
        {
            CallStack = new List<FileSystemTask>();
            Control = control;
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
                if (CallStack.Count == 0) { continue; }
                FileSystemTask task = CallStack.Find(item => item.Priority == Priority.High);
                // FileSystemTask task = CallStack.GroupBy(item => item.Priority).First(item => item.Key == Priority.High).ToList().First();
                string message = "";
                task.DoFeedBack(DoParseAndDo(task, ref message), message);
                CallStack.Remove(task);
            }
        }

        private void DoByPriority()
        {
            var list = CallStack.GroupBy(item => item.Priority);
            for (int i = 0; i < list.Count(); i++)
            {
                
            }
        }

        private bool Do(Happened happened, bool isFolder, string path, ref string message)
        {
            switch (happened)
            {
                case Happened.Created:
                {
                    return isFolder
                        ? BrowseSystem.CreateProject(path, Control, ref message)
                        : BrowseSystem.CreateFile(Path.GetFileNameWithoutExtension(path), Path.GetDirectoryName(path), Control, ref message);
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
                return Do(args.Happened, true, task.Path, ref message);
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
