using System.Windows.Documents;
using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;

namespace Controls
{
    internal class SaveManager : IDisposable
    {
        private List<CallStackTask> CallStack;
        private Thread CallStackThread;
        private FlowDocument Document;
        private string FilePath;
        private BaseRichTextBox RichTextBox;

        public SaveManager(string filePath, BaseRichTextBox rtBox)
        {
            FilePath = filePath;
            RichTextBox = rtBox;
            CreateCallStack();
        }

        private void CreateCallStack()
        {
            CallStack = new List<CallStackTask>();
            LockObject = new object();
            CallStackThread = new Thread(DoCallStack);
            CallStackThread.ApartmentState = ApartmentState.STA;
            CallStackThread.IsBackground = true;
            CallStackThread.Priority = ThreadPriority.Lowest;
            CallStackThread.Start();
        }
        internal void AddTaskToCallStack(List<byte[]> content, int position, int count, string checkingText)
        {
            CallStack.Add(new CallStackTask(content, position, count, checkingText));
        }
        private void CreateDocument()
        {
            Document = new FlowDocument();
            FileWorkerManager.Do(Document, FilePath, false);
            //using (
            //    FileStream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.None,
            //        20 * 1024 * 1024))
            //{
            //    new TextRange(Document.ContentStart, Document.ContentEnd).Load(stream, DataFormats.Rtf);
            //}
        }

        private List<Paragraph> GetParagraph()
        {
            List<Paragraph> paragraphs = new List<Paragraph>();
            foreach (byte[] bytes in CallStack[0].Buffer)
            {
                var paragraph = new Paragraph();
                using (var stream = new MemoryStream(bytes))
                {
                    new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Load(stream, DataFormats.Rtf);
                }
                paragraphs.Add(paragraph);
            }
            return paragraphs;
        }

        private static object LockObject;
        private void DoCallStack()
        {
            CreateDocument();
            while (true)
            {
                if (CallStack.Count > 0)
                {
                    lock (LockObject)
                    {
                        if (CallStack[0].Stop)
                        {
                            break;
                        }
                        var paragraphsList = Document.Blocks.ToList();
                        var paragraphs = GetParagraph();
                        int i = paragraphs.Count - 1;
                        int n = paragraphsList.Count - 1;
                        int diff = CallStack[0].Length - n;
                        foreach (Paragraph paragraph in paragraphs)
                        {
                            int currentPosition = CallStack[0].CurrentParagraph - i;
                            if (diff == i) //add
                            {
                                if (currentPosition <= n)
                                {
                                    paragraphsList[currentPosition] = paragraph;

                                    //  paragraphsList.Insert(currentPosition, paragraph);
                                }
                                else //add paragraphs to last position
                                {
                                    paragraphsList.Add(paragraph);
                                }
                            }
                            else if (diff > 0) //insert
                            {
                                if (i >= 1 && currentPosition < paragraphsList.Count) //replace paragraphs after insert
                                {
                                    paragraphsList[currentPosition] = paragraph;
                                }
                                else
                                {
                                    paragraphsList.Insert(currentPosition, paragraph);
                                }
                            }
                            else if (diff < 0) //delete
                            {
                                if ((paragraphsList.Count - 1 - CallStack[0].Length) > 0)
                                    //in first iteration delete redudant paragraphs
                                {
                                    paragraphsList.RemoveRange(currentPosition + 1, diff * -1);
                                }
                                paragraphsList[currentPosition] = paragraph;
                            }
                            else if (diff == 0 && i > 0)
                            {
                                paragraphsList[currentPosition] = paragraph;
                            }
                            i--;
                        }
                        Document.Blocks.Clear();
                        if (paragraphs.Count > 0)
                        {
                            Document.Blocks.AddRange(paragraphsList);
                        }
                        //using (
                        //FileStream stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None,
                        //    20 * 1024 * 1024))
                        //{
                        //    new TextRange(Document.ContentStart, Document.ContentEnd).Save(stream, DataFormats.Rtf);
                        //}
                        if (new TextRange(Document.ContentStart, Document.ContentEnd).Text != CallStack[0].CheckingText)
                        {
                            Document.Dispatcher.Invoke(() =>
                            {
                                FileWorkerManager.Do(Document, FilePath);
                                //    using (FileStream stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None,
                                //20 * 1024 * 1024))
                                //    {
                                //        new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Save(stream, DataFormats.Rtf);
                                //    }
                            });
                        }
                        else
                        {
                            FileWorkerManager.Do(Document, FilePath);
                        }
                        CallStack.RemoveAt(0);

                    }
                }
            }
        }

        public void Dispose()
        {
            CallStack.Add(new CallStackTask());
        }
        protected class CallStackTask
        {
            public List<byte[]> Buffer;
            public int Length;
            public int CurrentParagraph;
            public string CheckingText;
            public bool Stop { get; private set; }
            public CallStackTask() {
                Stop = true;
            }
            public CallStackTask(List<byte[]> content, int caretPosition, int count, string text)
            {
                Buffer = content;
                Length = count;
                CheckingText = text;
                CurrentParagraph = caretPosition;
            }
        }
    }

    internal static class FileWorkerManager
    {
        public static void Do(FlowDocument document, string path, bool write = true)
        {
            lock (path)
            {
                if (write)
                {
                    using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                       20 * 1024 * 1024))
                    {
                        new TextRange(document.ContentStart, document.ContentEnd).Save(stream, DataFormats.Rtf);
                    }
                }
                else
                {
                    using (
              FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None,
                  20 * 1024 * 1024))
                    {
                        new TextRange(document.ContentStart, document.ContentEnd).Load(stream, DataFormats.Rtf);
                    }
                }
            }
        }
    }

}
