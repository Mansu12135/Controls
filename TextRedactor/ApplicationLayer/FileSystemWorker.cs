using System;
using System.IO;

namespace ApplicationLayer
{
    public class FileSystemWorker : IDisposable
    {
        private IFileSystemControl MainCantrol;
        private FileSystemQueue Queue;
        private string CurrentProject;
        private string CurrentFile;
        private string WorkDirectory = @"C:\Users\Никита\Documents\TextRedactor\MyProjects";

        public FileSystemWorker(IFileSystemControl control)
        {
            MainCantrol = control;
            AttachToControl();
            Queue = new FileSystemQueue(control);
        }

        public void Dispose()
        {
            DettachFromControl();
        }

        private void AttachToControl()
        {
            DettachFromControl();
            MainCantrol.ProjectChanged += OnProjectDeletedRenamedChanged;
            MainCantrol.ProjectCreated += OnProjectDeletedRenamedChanged;
            MainCantrol.ProjectRenamed += OnProjectDeletedRenamedChanged;
            MainCantrol.ProjectDeleted += OnProjectDeletedRenamedChanged;
            MainCantrol.FileChanged += OnRenamedChanged;
            MainCantrol.FileDeleted += OnRenamedChanged;
            MainCantrol.FileRenamed += OnRenamedChanged;
            MainCantrol.FileCreated += OnRenamedChanged;
        }

        private void DettachFromControl()
        {
            MainCantrol.ProjectChanged -= OnProjectDeletedRenamedChanged;
            MainCantrol.ProjectCreated -= OnProjectDeletedRenamedChanged;
            MainCantrol.ProjectRenamed -= OnProjectDeletedRenamedChanged;
            MainCantrol.ProjectDeleted -= OnProjectDeletedRenamedChanged;
            MainCantrol.FileChanged -= OnRenamedChanged;
            MainCantrol.FileDeleted -= OnRenamedChanged;
            MainCantrol.FileRenamed -= OnRenamedChanged;
            MainCantrol.FileCreated -= OnRenamedChanged;
        }

        private void OnRenamedChanged(object sender, FileArgs e)
        {
            e.Files.ForEach(file => Queue.AddTask(Path.Combine(WorkDirectory, CurrentProject, file), e, Callback));
        }

        private void Callback(bool b, string message)
        {

        }

        private void OnProjectDeletedRenamedChanged(object sender, ProjectArgs e)
        {
            Queue.AddTask(Path.Combine(WorkDirectory, e.Project), e, Callback);
        }
    }
}
