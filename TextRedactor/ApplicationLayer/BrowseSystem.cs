using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Transactions;

namespace ApplicationLayer
{
    internal static class BrowseSystem
    {
        internal static readonly string SubFolder = "Files";
        internal static readonly string Extension = ".xaml";
        private static readonly string ProjectExtension = ".prj";

        private static bool CreateProjectFile(object project, string path, ref string message)
        {
                try
                {
                    using (FileStream fs = File.Create(path))
                    {
                        BinaryFormatter write = new BinaryFormatter();
                        write.Serialize(fs, project);
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return false;
                }
                return true;
        }

        private static bool Save(object project, string path, ref string message)
        {
            return CreateProjectFile(project, path, ref message);
        }

        public static bool RemoveProject(string path, ref string message)
        {
            return TransactionDirectory.RemoveDirectory(path);
        }

        public static bool RemoveFile(FileArgs args, IBasicPanel<Project> control, ref string message)
        {
            TransactionFile.DeleteFiles(args);
            return Save(control.Save(args.Project, control.SaveItemManager.DoSave(args)),
               Path.Combine(control.FSWorker.WorkDirectory, args.Project, args.Project + ProjectExtension), ref message);
        }

        public static bool RenameProject(ProjectArgs args, IBasicPanel<Project> control, ref string message)
        {
            string path = Path.Combine(control.FSWorker.WorkDirectory, args.RenamedArgs.From);
            ComplexTransaction transaction = new ComplexTransaction();
            transaction.AddOperation(() =>
                TransactionDirectory.CopyDirectory(path,
                    Path.Combine(control.FSWorker.WorkDirectory, args.RenamedArgs.To)), () =>
            {
                if (!TransactionDirectory.RemoveDirectory(
   Path.Combine(control.FSWorker.WorkDirectory, args.RenamedArgs.To)))
                {
                    Transaction.Current.Rollback();
                }
            });
            transaction.AddOperation(() =>
            {
                TransactionDirectory.RemoveDirectory(path);
            }, new List<string> { path });
            transaction.AddOperation(() =>
            {
                TransactionFile.DeleteFiles(
                    new FileArgs(
                        new List<string>
                        {
                            Path.Combine(control.FSWorker.WorkDirectory, args.RenamedArgs.To,
                                args.RenamedArgs.From + ProjectExtension)
                        }, args.RenamedArgs.From, Happened.Deleted,
                        (b, s, arg) => { }));
            }, new List<string>
                        {
                            Path.Combine(control.FSWorker.WorkDirectory, args.RenamedArgs.To,
                                args.RenamedArgs.From + ProjectExtension)
                        });
            transaction.DoOperation();
            return Save(control.Save(args.RenamedArgs.To, control.SaveItemManager.DoSave(args)),
                    Path.Combine(control.FSWorker.WorkDirectory, args.RenamedArgs.To,
                        args.RenamedArgs.To + ProjectExtension), ref message);
        }

        public static bool RenameFile(FileArgs args, IBasicPanel<Project> control, ref string message)
        {
            string path = Path.Combine(control.FSWorker.WorkDirectory, args.Project, BrowseSystem.SubFolder);
            TransactionFile.MoveFile(Path.Combine(path, args.RenamedArgs.From),
                Path.Combine(path, args.RenamedArgs.To), ref message);
            return Save(control.Save(args.Project, control.SaveItemManager.DoSave(args)),
                Path.Combine(control.FSWorker.WorkDirectory, args.Project, args.Project + ProjectExtension), ref message);
        }

        public static bool CreateProject(ProjectArgs args, IBasicPanel<Project> control, ref string message)
        {
            string path = Path.Combine(control.FSWorker.WorkDirectory, args.Project);
            using (var transaction = new TransactionScope())
            {
                if (!TransactionDirectory.CreateDirectory(path, ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                if (!TransactionDirectory.CreateDirectory(Path.Combine(path, SubFolder), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                transaction.Complete();
            }
            string name = new DirectoryInfo(path).Name;
            if (!Save(control.Save(args.Project, control.SaveItemManager.DoSave(args)), Path.Combine(path, name + ProjectExtension), ref message))
            {
                Directory.Delete(path, true);
                return false;
            }
            return true;
        }

        public static bool CreateFile(FileArgs args, IBasicPanel<Project> control, ref string message)
        {
            string path = Path.Combine(control.FSWorker.WorkDirectory, args.Project, BrowseSystem.SubFolder);
            foreach (string file in args.Files)
            {
                if (!TransactionFile.CreateFile(Path.Combine(path, file + Extension), ref message))
                {
                    return false;
                }
            }
            return Save(control.Save(args.Project, control.SaveItemManager.DoSave(args)),
                Path.Combine(control.FSWorker.WorkDirectory, args.Project, args.Project + ProjectExtension), ref message);
        }
    }
}
