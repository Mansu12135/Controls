using System;
using System.Collections.Generic;

namespace ApplicationLayer
{
    public interface IFileSystemControl
    {
        event EventHandler<ProjectArgs> ProjectChanged;

        event EventHandler<FileArgs> FileChanged;

        event EventHandler<ProjectArgs> ProjectRenamed;

        event EventHandler<FileArgs> FileRenamed;

        event EventHandler<ProjectArgs> ProjectCreated;

        event EventHandler<FileArgs> FileCreated;

        event EventHandler<ProjectArgs> ProjectDeleted;

        event EventHandler<FileArgs> FileDeleted;
    }

    public class ProjectArgs : EventArgs
    {
        internal RenamedArgs RenamedArgs;

        public ProjectArgs(string project, Happened happened)
        {
            Project = project;
            Happened = happened;
        }

        public ProjectArgs(RenamedArgs args)
        {
            Happened = args.Happened;
            RenamedArgs = args;
        }

        public string Project { get; private set; }

        public Happened Happened { get; private set; }
    }

    public class RenamedArgs : EventArgs
    {
        protected RenamedArgs(string from, string to)
        {
            From = from;
            To = to;
        }

        public Happened Happened { get { return Happened.Changed; } }

        public string From { get; private set; }

        public string To { get; private set; }
    }

    public class FileArgs : EventArgs
    {
        internal RenamedArgs RenamedArgs;
        public FileArgs(List<string> files, Happened happened)
        {
            Files = files;
            Happened = happened;
        }

        public FileArgs(RenamedArgs args)
        {
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
