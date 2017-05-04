using System.Collections.Generic;
using System.IO;

namespace ApplicationLayer
{
    internal static class RemoveSnapshotManager
    {
        private static void GetInformation(string path, ref Dictionary<string, byte[]> results)
        {
            if (File.Exists(path))
            {
                results.Add(path, File.ReadAllBytes(path));
                return;
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            results.Add(dir.FullName, null);
            foreach (FileInfo file in dir.GetFiles())
            {
                results.Add(file.FullName, File.ReadAllBytes(file.FullName));
            }
            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                GetInformation(directory.FullName, ref results);
            }
        }

        public static Dictionary<string, byte[]> GetSnapshot(List<string> paths)
        {
            Dictionary<string, byte[]> snapshot = new Dictionary<string, byte[]>();
            foreach (string path in paths)
            {
                if (!File.Exists(path) && !Directory.Exists(path)) { return snapshot; }
                GetInformation(path, ref snapshot);
            }
            return snapshot;
        }
    }
}
