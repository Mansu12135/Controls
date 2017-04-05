using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Transactions;
using ChinhDo.Transactions;
using Microsoft.Win32.SafeHandles;

namespace ApplicationLayer
{
    internal sealed class TransactionDirectory
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        private extern static bool PathIsDirectoryEmpty(string pszPath);

        [DllImport("Kernel32.Dll", EntryPoint = "CreateDirectoryTransacted", CharSet = CharSet.Unicode, SetLastError = true)]
        protected static extern bool CreateDirectoryTransacted(
           [In] IntPtr lpTemplateDirectory,
           [In] String lpNewDirectory,
           [In] IntPtr lpSecurityAttributes,
           [In] SafeTransactionHandle hTransaction
       );

        [DllImport("Kernel32.Dll", EntryPoint = "GetLastError", CharSet = CharSet.Unicode, SetLastError = true)]
        protected static extern uint GetLastError();

        [DllImport("Kernel32.Dll", EntryPoint = "RemoveDirectoryTransacted", CharSet = CharSet.Unicode, SetLastError = true)]
        protected static extern bool RemoveDirectoryTransacted(
          [In] String lpPathName,
          [In] SafeTransactionHandle hTransaction
      );
        [ComImport]
        [Guid("79427A2B-F895-40e0-BE79-B57DC82ED231")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        protected interface IKernelTransaction
        {
            void GetHandle(out SafeTransactionHandle ktmHandle);
        }

        private static void CopyDirectories(string path, string pathTo, ref string message)
        {
            //pathTo = Path.Combine(pathTo, Path.GetDirectoryName(path));
            if (!CreateDirectory(pathTo, ref message)) { return; }
            foreach (var file in Directory.GetFiles(path))
            {
                if (!TransactionFile.MoveFile(Path.Combine(path, file), Path.Combine(pathTo, Path.GetFileName(file)), ref message)) { return; }
            }
            foreach (var directory in Directory.GetDirectories(path))
            {
                string name = new DirectoryInfo(directory).Name;
                CopyDirectories(Path.Combine(path, name), Path.Combine(pathTo, name), ref message);
            }
        }

        public static bool CopyDirectory(string path, string pathTo, ref string message)
        {
            if (!Directory.Exists(path) || !Directory.Exists(pathTo))
            {
                message = "Wrong path or folders are not exists";
                return false;
            }
            bool isParentTransaction = Transaction.Current == null;
            using (var transaction = isParentTransaction ? new TransactionScope() : new TransactionScope(Transaction.Current))
            {
                CopyDirectories(path, pathTo, ref message);
                if (!string.IsNullOrEmpty(message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
                if (!isParentTransaction)
                {
                    transaction.Complete();
                }
            }
            return true;
        }

        public static bool MoveDirectory(string path, string pathTo)
        {
            bool response = true;
            string message = String.Empty;
            if (!Directory.Exists(path) || Directory.Exists(pathTo))
            {
                message = "Wrong path or folders are not exists";
                return false;
            }
                CopyDirectories(path, pathTo, ref message);
                if (!string.IsNullOrEmpty(message))
                {
                    Transaction.Current.Rollback();
                    return false;
                }
            return response;
        }

        private static bool CreateDir(string path, ref string message)
        {
            bool response = false;
            SafeTransactionHandle txHandle = null;
            try
            {
                IKernelTransaction kernelTx =
                   (IKernelTransaction)TransactionInterop.GetDtcTransaction(Transaction.Current);
                kernelTx.GetHandle(out txHandle);
                response = CreateDirectoryTransacted(IntPtr.Zero, path, IntPtr.Zero, txHandle);
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                response = false;
                Transaction.Current.Rollback();
            }
            return response;
        }

        public static bool CreateDirectory(string path, ref string message)
        {
            if (Directory.Exists(path))
            {
                message = "Wrong path or directory exists";
                return false;
            }
            bool response = false;
            bool isParentTransaction = Transaction.Current == null;
            //  if (Transaction.Current != null) { return CreateDir(path, ref message); }
            using (var transaction = isParentTransaction ? new TransactionScope() : new TransactionScope(Transaction.Current))
            {
                response = CreateDir(path, ref message);
                if (!isParentTransaction)
                {
                    transaction.Complete();
                }
            }
            return response;
        }


        public static bool RemoveDirectory(string path)
        {
            string message = string.Empty;
            if (!Directory.Exists(path))
            {
                message = "Wrong path or directory not exists";
                return false;
            }
            SafeTransactionHandle txHandle = null;
            try
            {
                new TxFileManager().DeleteDirectory(path);
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                Transaction.Current.Rollback();
                return false;
            }
            return true;
        }

        public static bool DirectoryIsEmpty(string path)
        {
            return PathIsDirectoryEmpty(path);
        }

        public sealed class SafeTransactionHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [DllImport("Kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(IntPtr handle);

            public SafeTransactionHandle() : base(true)
            {
            }

            public SafeTransactionHandle(IntPtr preexistingHandle, bool ownsHandle)
                    : base(ownsHandle)
            {
                SetHandle(preexistingHandle);
            }
            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
    }
}
