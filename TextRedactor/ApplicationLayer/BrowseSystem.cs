using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Transactions;

namespace ApplicationLayer
{
    internal static class BrowseSystem
    {
        private static readonly string SubFolder = "Files";
        private static readonly string Extension = ".rtf";
        private static readonly string ProjectExtension = ".prj";

        private static bool CreateProjectFile(object project, string path, ref string message)
        {
            using (var transaction = new TransactionScope())
            {
                try
                {
                    using (FileStream fs = File.Create(path))
                    {
                        BinaryFormatter write = new BinaryFormatter();
                        write.Serialize(fs, project);
                    }
                    transaction.Complete();
                }
                catch (Exception ex)
                {
                    Transaction.Current.Rollback();
                    message = ex.Message;
                    return false;
                }
                return true;
            }
        }

        private static bool Save(object project, string path, ref string message)
        {
            return CreateProjectFile(project, path, ref message);
        }

        public static bool RemoveProject(string path, ref string message)
        {
            using (var transaction = new TransactionScope())
            {
                if (!TransactionDirectory.RemoveDirectory(path, ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                transaction.Complete();
            }
            return true;
        }

        public static bool RemoveFile(string path, IFileSystemControl control, ref string message)
        {
            using (var transaction = new TransactionScope())
            {
                if (!TransactionFile.DeleteFile(path, ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                var dir = new DirectoryInfo(path);
                if (Save(control.Save(dir.Parent.Name), Path.Combine(dir.Parent.FullName, dir.Parent.Name + ProjectExtension), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                transaction.Complete();
            }
            return true;
        }

        public static bool RenameProject(string path, string newName, IFileSystemControl control, ref string message)
        {
            using (var transaction = new TransactionScope())
            {
                var dir = new DirectoryInfo(path);
                if (!TransactionDirectory.MoveDirectory(path,
                    Path.Combine(dir.Parent != null ? dir.Parent.FullName : "", newName), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                if (!TransactionFile.DeleteFile(Path.Combine(path, SubFolder, dir.Name + ProjectExtension), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                if (Save(control.Save(dir.Name), Path.Combine(path, SubFolder, newName + ProjectExtension), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                transaction.Complete();
            }
            return true;
        }

        public static bool RenameFile(string path, string newName, IFileSystemControl control, ref string message)
        {
            using (var transaction = new TransactionScope())
            {
                if (!TransactionFile.MoveFile(path,
                    Path.Combine(Path.GetDirectoryName(path), newName + Path.GetExtension(path)), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                var dir = new DirectoryInfo(path);
                if (Save(control.Save(dir.Parent.Name), Path.Combine(dir.Parent.FullName, dir.Parent.Name + ProjectExtension), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                transaction.Complete();
            }
            return true;
        }

        public static bool CreateProject(string path, IFileSystemControl control, ref string message)
        {
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
                string name = new DirectoryInfo(path).Name;
                if (Save(control.Save(name), Path.Combine(path, name + ProjectExtension), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                transaction.Complete();
            }
            return true;
        }

        public static bool CreateFile(string name, string path, IFileSystemControl control, ref string message)
        {
            using (var transaction = new TransactionScope())
            {
                if (TransactionFile.CreateFile(Path.Combine(path, name + Extension), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                if (Save(control.Save(name), Path.Combine(path, name + ProjectExtension), ref message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                transaction.Complete();
            }
            return true;
        }
    }
}
