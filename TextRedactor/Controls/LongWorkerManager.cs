using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Documents;

namespace Controls
{
    public class LongWorkerManager : IDisposable
    {
        public volatile List<byte[]> tasks = new List<byte[]>();
        private Thread thread;
        private SuperRichTextBox Control;
        internal FlowDocument Document;

        public LongWorkerManager()
        {
            thread = new Thread(ReadCallback);
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.AboveNormal;
            thread.Start();
        }

        private void ReadCallback()
        {
            while (true)
            {
                int n = tasks.Count-1;
                if (n >= 0)
                {
                    using (
                        FileStream stream = new FileStream(@"Test.xaml", FileMode.Create, FileAccess.Write,
                            FileShare.None,
                            20*1024*1024))
                    {
                        foreach (byte b in tasks[n])
                        {
                            stream.WriteByte(b);
                        }
                    }
                    tasks.RemoveRange(0, n);
                }
            }
        }

        public void Dispose()
        {
            thread.Join();
            tasks.Clear();
        }
    }
}
