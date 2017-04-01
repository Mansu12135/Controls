using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Transactions;

namespace ApplicationLayer
{
    internal static class BrowseSystem
    {
        internal static readonly string SubFolder = "Files";
        internal static readonly string Extension = ".rtf";
        private static readonly string ProjectExtension = ".prj";

        private static bool CreateProjectFile(object project, string path, ref string message)
        {
           // using (var transaction = new TransactionScope())
            //{
                try
                {
                    using (FileStream fs = File.Create(path))
                    {
                        BinaryFormatter write = new BinaryFormatter();
                        write.Serialize(fs, project);
                    }
                   // transaction.Complete();
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return false;
                }
                return true;
            //}
        }

        private static bool Save(object project, string path, ref string message)
        {
            return CreateProjectFile(project, path, ref message);
        }

        //public static bool RemoveProject(string path, ref string message)
        //{
        //    using (var transaction = new TransactionScope())
        //    {
        //        if (!TransactionDirectory.RemoveDirectory(path, ref message))
        //        {
        //            Transaction.Current.Rollback();
        //            return false;
        //        }
        //        transaction.Complete();
        //    }
        //    return true;
        //}

        //public static bool RemoveFile(string path, IBasicPanel<Project> control, ref string message)
        //{
        //    using (var transaction = new TransactionScope())
        //    {
        //        if (!TransactionFile.DeleteFile(path, ref message))
        //        {
        //            Transaction.Current.Rollback();
        //            return false;
        //        }
        //        var dir = new DirectoryInfo(path);
        //        if (Save(control.Save(control.SaveItemManager.DoSave()), Path.Combine(dir.Parent.FullName, dir.Parent.Name + ProjectExtension), ref message))
        //        {
        //            Transaction.Current.Rollback();
        //            return false;
        //        }
        //        transaction.Complete();
        //    }
        //    return true;
        //}

        //public static bool RenameProject(string path, string newName, IFileSystemControl control, ref string message)
        //{
        //    using (var transaction = new TransactionScope())
        //    {
        //        var dir = new DirectoryInfo(path);
        //        if (!TransactionDirectory.MoveDirectory(path,
        //            Path.Combine(dir.Parent != null ? dir.Parent.FullName : "", newName), ref message))
        //        {
        //            Transaction.Current.Rollback();
        //            return false;
        //        }
        //        if (!TransactionFile.DeleteFile(Path.Combine(path, SubFolder, dir.Name + ProjectExtension), ref message))
        //        {
        //            Transaction.Current.Rollback();
        //            return false;
        //        }
        //        if (Save(control.Save(dir.Name), Path.Combine(path, SubFolder, newName + ProjectExtension), ref message))
        //        {
        //            Transaction.Current.Rollback();
        //            return false;
        //        }
        //        transaction.Complete();
        //    }
        //    return true;
        //}

        //public static bool RenameFile(string path, string newName, IFileSystemControl control, ref string message)
        //{
        //    using (var transaction = new TransactionScope())
        //    {
        //        if (!TransactionFile.MoveFile(path,
        //            Path.Combine(Path.GetDirectoryName(path), newName + Path.GetExtension(path)), ref message))
        //        {
        //            Transaction.Current.Rollback();
        //            return false;
        //        }
        //        var dir = new DirectoryInfo(path);
        //        if (Save(control.Save(dir.Parent.Name), Path.Combine(dir.Parent.FullName, dir.Parent.Name + ProjectExtension), ref message))
        //        {
        //            Transaction.Current.Rollback();
        //            return false;
        //        }
        //        transaction.Complete();
        //    }
        //    return true;
        //}

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
                    Transaction.Current.Rollback();
                    return false;
                }
            }
            if (!Save(control.Save(args.Project, control.SaveItemManager.DoSave(args)), Path.Combine(control.FSWorker.WorkDirectory, args.Project, args.Project + ProjectExtension), ref message))
            {
                Transaction.Current.Rollback();
                return false;
            }
            return true;
        }
    }
}
