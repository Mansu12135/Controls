using System;
using System.IO;

namespace ApplicationLayer
{
    public class SaveItemManager<T> where T : Item
    {
        private IBasicPanel<T> Control;
        public SaveItemManager(IBasicPanel<T> control)
        {
            Control = control;
        }

        public Action DoSave(EventArgs args)
        {
            var typeControl = Control as IBasicPanel<Project>;
            Type type = args.GetType();
            if (type == typeof(FileArgs))
            {
                var fileArgs = (FileArgs)args;
                return () =>
                {
                    Project project = typeControl.Notes[fileArgs.Project];
                    if (fileArgs.RenamedArgs != null)
                    {
                        project.Files.Remove(project.Files.Find(file => file.Name == fileArgs.RenamedArgs.From));
                        string projectPath = Path.Combine(typeControl.FSWorker.WorkDirectory, fileArgs.Project);
                        project.Files.Add(new LoadedFile(Path.Combine(projectPath, BrowseSystem.SubFolder, fileArgs.RenamedArgs.To + BrowseSystem.Extension), projectPath));
                        return;
                    }
                    if (fileArgs.Happened == Happened.Created)
                    {
                        string projectPath = Path.Combine(typeControl.FSWorker.WorkDirectory, fileArgs.Project);
                        fileArgs.Files.ForEach(item => project.Files.Add(new LoadedFile(Path.Combine(projectPath, BrowseSystem.SubFolder, item + BrowseSystem.Extension), projectPath)));
                        return;
                    }
                    if (fileArgs.Happened == Happened.Deleted)
                    {
                        fileArgs.Files.ForEach(item => project.Files.Remove(project.Files.Find(file => file.Name == item)));
                    }
                };
            }
            if (type == typeof(ProjectArgs))
            {
                var projectArgs = (ProjectArgs)args;
                return () =>
                {
                    if (projectArgs.RenamedArgs != null)
                    {
                        typeControl.Notes.Remove(projectArgs.RenamedArgs.From);
                        typeControl.Notes.Add(projectArgs.RenamedArgs.To, new Project(projectArgs.RenamedArgs.To));
                        return;
                    }
                    if (projectArgs.Happened == Happened.Created)
                    {
                        typeControl.Notes.Add(projectArgs.Project, new Project(projectArgs.Project));
                        return;
                    }
                    if (projectArgs.Happened == Happened.Deleted)
                    {
                        typeControl.Notes.Remove(projectArgs.Project);
                    }
                };
            }
            throw new Exception();
        }
    }
}
