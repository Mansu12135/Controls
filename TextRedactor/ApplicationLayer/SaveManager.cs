using System.Windows.Documents;
using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Threading;

namespace ApplicationLayer
{
    public class SaveManager : IDisposable
    {
        private FlowDocument Document;
        private string FilePath;
        private TextRangeList<TextRange> List;
        private TextRangeList<TextRange> MainList;
        private List<int> l = new List<int>();
        private bool isAlive = true;
        private ManualResetEventSlim Lock = new ManualResetEventSlim(true);

        public void Dispose()
        {
            MainList.CollectionChanged -= MainList_CollectionChanged;
            isAlive = false;
        }

        public void DoStart(string filePath, TextRangeList<TextRange> mainList)
        {
            FilePath = filePath;
            Document = new FlowDocument();
            Style style = new Style(typeof(Paragraph));
            style.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));
            Document.Resources.Add(typeof(Paragraph), style);
            List = new TextRangeList<TextRange>(Document);
            MainList = mainList;
            AttachEventHandler();
        }

        private void AttachEventHandler()
        {
            MainList.CollectionChanged -= MainList_CollectionChanged;
            MainList.CollectionChanged += MainList_CollectionChanged;
            DoListen();
        }

        private void MainList_CollectionChanged(int index, Changed state)
        {
            if (OnStatusChanged != null)
            {
                if (!l.Any())
                {
                    OnStatusChanged.Invoke(SaveManagerStatus.Started, 0);
                }
                else
                {
                    OnStatusChanged.Invoke(SaveManagerStatus.Running, l.Count + 1);
                }
            }
            l.Add(index);
        }

        private void DoListen()
        {
            while (isAlive)
            {
                if (l.Count > 0 || l.Count > 0)
                {
                    Lock.Wait();
                    List.SynchronizeTo(l[0], MainList);
                    FileWorkerManager.DoAsync(Document, FilePath, Lock);
                    l.RemoveAt(0);
                    if(l.Any() || OnStatusChanged == null) { continue; }
                    OnStatusChanged.Invoke(SaveManagerStatus.Stoped, 0);
                }
            }
        }

        public delegate void SaveManagerStatusChangedHandler(SaveManagerStatus status, int inQueue);

        public event SaveManagerStatusChangedHandler OnStatusChanged;
    }

    public enum SaveManagerStatus
    {
        Started,
        Running,
        Stoped
    }

    public static class FileWorkerManager
    {
        public static void DoAsync(FlowDocument document, string path, ManualResetEventSlim locker, bool write = true)
        {
            Do(document, path, write);
            locker.Set();
        }

        public static void Do(FlowDocument document, string path, bool write = true)
        {
            lock (path)
            {
                if (write)
                {
                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    //using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                    //   20 * 1024 * 1024))
                    {
                        new TextRange(document.ContentStart, document.ContentEnd).Save(stream, DataFormats.XamlPackage);
                    }
                }
                else
                {
                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        new TextRange(document.ContentStart, document.ContentEnd).Load(stream, DataFormats.XamlPackage);
                    }
                }
            }
        }
    }

}
