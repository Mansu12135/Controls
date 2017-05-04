using System;
using System.Collections.Generic;

namespace ApplicationLayer
{
    public interface IFileSystemControl
    {
        FileSystemWorker<Project> FSWorker { get; }

        void Callback(bool rezult, string message);

        event EventHandler<ProjectArgs> ProjectChanged;

        event EventHandler<FileArgs> FileChanged;

        event EventHandler<ProjectArgs> ProjectRenamed;

        event EventHandler<FileArgs> FileRenamed;

        event EventHandler<ProjectArgs> ProjectCreated;

        event EventHandler<FileArgs> FileCreated;

        event EventHandler<ProjectArgs> ProjectDeleted;

        event EventHandler<FileArgs> FileDeleted;

        string CurrentProjectsPath { get; }
    }

    public class ProjectArgs : EventArgs
    {
        internal RenamedArgs RenamedArgs;

        public ProjectArgs(string project, Happened happened, Action<bool, string> callback)
        {
            Project = project;
            Happened = happened;
            Callback = callback;
        }

        public ProjectArgs(RenamedArgs args)
        {
            Happened = args.Happened;
            RenamedArgs = args;
            Project = args.From;
            Callback = args.Callback;
        }

        public string Project { get; private set; }

        public Action<bool, string> Callback { get; private set; }

        public Happened Happened { get; private set; }
    }

    public class RenamedArgs : EventArgs
    {
        public RenamedArgs(string from, string to, Action<bool, string> callback)
        {
            From = from;
            To = to;
            Callback = callback;
        }

        public Action<bool, string> Callback { get; private set; }

        public Happened Happened { get { return Happened.Changed; } }

        public string From { get; private set; }

        public string To { get; private set; }
    }

    public class FileArgs : EventArgs
    {
        internal RenamedArgs RenamedArgs;

        internal Action<bool, string> Callback;

        internal string Project;

        public FileArgs(List<string> files, string project, Happened happened, Action<bool, string> callback)
        {
            Files = files;
            Project = project;
            Happened = happened;
            Callback = callback;
        }

        public FileArgs(string project, RenamedArgs args)
        {
            Project = project;
            Happened = args.Happened;
            RenamedArgs = args;
        }

        public List<string> Files { get; private set; }

        public Happened Happened { get; private set; }
    }

    public enum Happened
    {
        Created,
        Changed,
        Deleted
    }
}
