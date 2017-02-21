using System.Windows.Documents;
using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;

namespace Controls
{
    internal class SaveManager : IDisposable
    {
       // private List<CallStackTask> CallStack;
       // private Thread CallStackThread;
        private FlowDocument Document;
        private string FilePath;
       // private BaseRichTextBox RichTextBox;
        private TextRangeList<TextRange> List;
        private TextRangeList<TextRange> MainList;
        private List<int> l = new List<int>();
        private bool isAlive = true;
        //private int PreviousCarrentPositionOffset;
        //private int SelectionEndPosition;
       // private byte[] Buffer;

        public void DoStart(string filePath, TextRangeList<TextRange> mainList)
        {
            FilePath = filePath;
            Document = new FlowDocument();
            List = new TextRangeList<TextRange>(Document);
            MainList = mainList;
            AttachEventHandler();
        }

        private void AttachEventHandler()
        {
           
            MainList.CollectionChanged -= MainList_CollectionChanged;
            MainList.CollectionChanged += MainList_CollectionChanged;
            DoListen();
            //RichTextBox.PreviewKeyDown += RichTextBox_PreviewKeyDown;
            //RichTextBox.PreviewMouseRightButtonDown += RichTextBox_PreviewMouseRightButtonDown;
            //RichTextBox.TextChanged -= RichTextBox_TextChanged;
            //RichTextBox.TextChanged += RichTextBox_TextChanged;
        }

        private void MainList_CollectionChanged(int index, Changed state)
        {
            l.Add(0);
        }

        private void DoListen()
        {
            while (isAlive)
            {
                if (l.Count > 0)
                {
                    List.SynchronizeTo(MainList);
                    FileWorkerManager.Do(Document, FilePath);
                    l.RemoveAt(0);
                }
            }
        }

        #region MyRegion


        //bool iscansel = false;
        //private void RichTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        //{
        //    lock (RichTextBox)
        //    {
        //        CallStackTask callStackTask = null;
        //        int diff = 0;
        //        foreach (var change in e.Changes)
        //        {
        //            if (change.RemovedLength > 0 && change.AddedLength > 0)
        //            {
        //                if (new TextRange(RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset),
        //               RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset + change.AddedLength)).IsEmpty
        //               && new TextRange(Document.ContentStart.GetPositionAtOffset(change.Offset + diff),
        //               Document.ContentStart.GetPositionAtOffset(change.Offset + change.RemovedLength + diff)).IsEmpty

        //               || new TextRange(Document.ContentStart.GetPositionAtOffset(change.Offset + diff), Document.ContentStart.GetPositionAtOffset(change.Offset + change.RemovedLength + diff)).Text == new TextRange(RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset), RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset + change.AddedLength)).Text)
        //                {
        //                    diff += change.RemovedLength - change.AddedLength;
        //                    continue;
        //                }
        //                var range =
        //          new TextRange(RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset),
        //              RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset + change.AddedLength));
        //                using (var stream = new MemoryStream())
        //                {
        //                    range.Save
        //                        (stream, DataFormats.Rtf);
        //                    callStackTask =  new CallStackTask(new List<byte[]> { stream.ToArray() }, change.Offset + diff,
        //                        change.Offset + change.RemovedLength + diff,
        //                        new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text);
        //                    //AddTaskToCallStack(new List<byte[]> { stream.ToArray() }, change.Offset+diff,
        //                    //    change.Offset+change.RemovedLength + diff,
        //                    //    new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text);
        //                }
        //                diff += change.RemovedLength - change.AddedLength;
        //            }
        //            else if (change.RemovedLength > 0)
        //            {
        //                if (new TextRange(Document.ContentStart.GetPositionAtOffset(change.Offset+ diff),
        //               Document.ContentStart.GetPositionAtOffset(change.Offset + change.RemovedLength+diff)).IsEmpty)
        //                {
        //                    diff+= change.RemovedLength;
        //                    continue;
        //                }
        //                if (callStackTask != null)
        //                {
        //                    callStackTask.Length += change.RemovedLength + change.Offset-callStackTask.CurrentParagraph+diff;
        //                    continue;
        //                }
        //                var range =
        //           new TextRange(RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset),
        //               RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset));
        //                using (var stream = new MemoryStream())
        //                {
        //                    range.Save(stream, DataFormats.Rtf);
        //                    //  var b = new byte[] { 0 };

        //                    callStackTask = new CallStackTask
        //                   // AddTaskToCallStack
        //                        (new List<byte[]> { stream.ToArray() }, change.Offset + diff,
        //                            change.Offset + change.RemovedLength + diff,
        //                            new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text);
        //                }
        //                diff += change.RemovedLength;
        //            }
        //            else if (change.AddedLength > 0)
        //            {
        //                if (new TextRange(RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset),
        //               RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset + change.AddedLength)).IsEmpty)
        //                {
        //                    diff -= change.AddedLength;
        //                    continue;
        //                }
        //                var range =
        //           new TextRange(RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset),
        //               RichTextBox.Document.ContentStart.GetPositionAtOffset(change.Offset + change.AddedLength));
        //                using (var stream = new MemoryStream())
        //                {
        //                    range.Save
        //                        (stream, DataFormats.Rtf);
        //                    callStackTask = new CallStackTask
        //                  //  AddTaskToCallStack
        //                        (new List<byte[]> { stream.ToArray() }, change.Offset + diff,
        //                        change.Offset + diff,
        //                        new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text);
        //                }
        //                diff -= change.AddedLength;
        //            }
        //         //   diff += (change.AddedLength - change.RemovedLength);
        //        }
        //        CallStack.Add(callStackTask);
        //        callStackTask = null;
        //       // iscansel = true;
        //        //var range =
        //        //    new TextRange(RichTextBox.Document.ContentStart.GetPositionAtOffset(PreviousCarrentPositionOffset),
        //        //        RichTextBox.CaretPosition);
        //        //        //SelectionEndPosition > -1
        //        //        //    ? (RichTextBox.Document.ContentStart.GetOffsetToPosition(RichTextBox.Document.ContentEnd) < SelectionEndPosition ? RichTextBox.Document.ContentStart.GetPositionAtOffset(PreviousCarrentPositionOffset) : RichTextBox.Document.ContentStart.GetPositionAtOffset(SelectionEndPosition))
        //        //        //    : RichTextBox.CaretPosition);
        //        //using (var stream = new MemoryStream())
        //        //{
        //        //    range.Save
        //        //        (stream, DataFormats.Rtf);
        //        //    AddTaskToCallStack(new List<byte[]> {stream.ToArray()}, PreviousCarrentPositionOffset,
        //        //        SelectionEndPosition,
        //        //        new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text);
        //        //}
        //    }
        //}

        //private void GetPreviousState()
        //{
        //    if (RichTextBox.Selection.IsEmpty)
        //    {
        //        PreviousCarrentPositionOffset =
        //            RichTextBox.Document.ContentStart.GetOffsetToPosition(RichTextBox.CaretPosition);
        //        SelectionEndPosition = -1;
        //    }
        //    else
        //    {
        //        PreviousCarrentPositionOffset =
        //            RichTextBox.Document.ContentStart.GetOffsetToPosition(RichTextBox.Selection.Start);
        //        SelectionEndPosition = RichTextBox.Document.ContentStart.GetOffsetToPosition(RichTextBox.Selection.End);
        //    }
        //}

        //private void RichTextBox_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //   // GetPreviousState();
        //}

        //private void RichTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) { return; }
        // //   GetPreviousState();
        //}

        //private void CreateCallStack()
        //{
        //    CallStack = new List<CallStackTask>();
        //    LockObject = new object();
        //    CallStackThread = new Thread(DoCallStack);
        //    CallStackThread.ApartmentState = ApartmentState.STA;
        //    CallStackThread.IsBackground = true;
        //    CallStackThread.Priority = ThreadPriority.Lowest;
        //    CallStackThread.Start();
        //}
        //internal void AddTaskToCallStack(List<byte[]> content, int position, int count, string checkingText)
        //{
        //    CallStack.Add(new CallStackTask(content, position, count, checkingText));
        //}
        //private void CreateDocument()
        //{
        //    Document = new FlowDocument();
        //    FileWorkerManager.Do(Document, FilePath, false);
        //    //using (
        //    //    FileStream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.None,
        //    //        20 * 1024 * 1024))
        //    //{
        //    //    new TextRange(Document.ContentStart, Document.ContentEnd).Load(stream, DataFormats.Rtf);
        //    //}
        //}

        //private List<Paragraph> GetParagraph()
        //{
        //    List<Paragraph> paragraphs = new List<Paragraph>();
        //    foreach (byte[] bytes in CallStack[0].Buffer)
        //    {
        //        var paragraph = new Paragraph();
        //        using (var stream = new MemoryStream(bytes))
        //        {
        //            new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Load(stream, DataFormats.Rtf);
        //        }
        //        paragraphs.Add(paragraph);
        //    }
        //    return paragraphs;
        //}

        //private static object LockObject;
        //private void DoCallStack()
        //{
        //    CreateDocument();
        //    while (true)
        //    {
        //        if (CallStack.Count > 0)
        //        {
        //            lock (LockObject)
        //            {
        //                if (CallStack[0].Stop)
        //                {
        //                    break;
        //                }
        //                #region Saving locate on Paragraphs
        //                //var paragraphsList = Document.Blocks.ToList();
        //                //var paragraphs = GetParagraph();
        //                //int i = paragraphs.Count - 1;
        //                //int n = paragraphsList.Count - 1;
        //                //int diff = CallStack[0].Length - n;
        //                //foreach (Paragraph paragraph in paragraphs)
        //                //{
        //                //    int currentPosition = CallStack[0].CurrentParagraph - i;
        //                //    if (diff == i) //add
        //                //    {
        //                //        if (currentPosition <= n)
        //                //        {
        //                //            paragraphsList[currentPosition] = paragraph;

        //                //            //  paragraphsList.Insert(currentPosition, paragraph);
        //                //        }
        //                //        else //add paragraphs to last position
        //                //        {
        //                //            paragraphsList.Add(paragraph);
        //                //        }
        //                //    }
        //                //    else if (diff > 0) //insert
        //                //    {
        //                //        if (i >= 1 && currentPosition < paragraphsList.Count) //replace paragraphs after insert
        //                //        {
        //                //            paragraphsList[currentPosition] = paragraph;
        //                //        }
        //                //        else
        //                //        {
        //                //            paragraphsList.Insert(currentPosition, paragraph);
        //                //        }
        //                //    }
        //                //    else if (diff < 0) //delete
        //                //    {
        //                //        if ((paragraphsList.Count - 1 - CallStack[0].Length) > 0)
        //                //            //in first iteration delete redudant paragraphs
        //                //        {
        //                //            paragraphsList.RemoveRange(currentPosition + 1, diff * -1);
        //                //        }
        //                //        paragraphsList[currentPosition] = paragraph;
        //                //    }
        //                //    else if (diff == 0 && i > 0)
        //                //    {
        //                //        paragraphsList[currentPosition] = paragraph;
        //                //    }
        //                //    i--;
        //                //}
        //                //Document.Blocks.Clear();
        //                //if (paragraphs.Count > 0)
        //                //{
        //                //    Document.Blocks.AddRange(paragraphsList);
        //                //}
        //                //using (
        //                //FileStream stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None,
        //                //    20 * 1024 * 1024))
        //                //{
        //                //    new TextRange(Document.ContentStart, Document.ContentEnd).Save(stream, DataFormats.Rtf);
        //                //}
        //                #endregion

        //                using (
        //                    var stream = new MemoryStream(CallStack[0].Buffer[0]))
        //                {
        //                    var range =
        //                        new TextRange(Document.ContentStart.GetPositionAtOffset(CallStack[0].CurrentParagraph),
        //                                //  CallStack[0].Length > -1  ?
        //                                Document.ContentStart.GetPositionAtOffset(CallStack[0].Length));
        //                                //: Document.ContentStart.GetPositionAtOffset(CallStack[0].CurrentParagraph));
        //                    range.Load(stream, DataFormats.Rtf);
        //                }
        //                if (new TextRange(Document.ContentStart, Document.ContentEnd).Text.Replace("\r\n", "") != CallStack[0].CheckingText.Replace("\r\n", ""))
        //                {
        //                    Console.WriteLine("Error");
        //                }
        //                else
        //                     if (CallStack.Count==1)
        //                {
        //                    FileWorkerManager.Do(Document, FilePath);
        //                }
        //                //     Document.Dispatcher.Invoke(() =>
        //                //   {
        //                //     FileWorkerManager.Do(Document, FilePath);
        //                //    using (FileStream stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None,
        //                //20 * 1024 * 1024))
        //                //    {
        //                //        new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Save(stream, DataFormats.Rtf);
        //                //    }
        //                // });
        //                // CreateDocument();
        //                // }
        //                // else
        //                //if(CallStack[0].Stop) {
        //                //     FileWorkerManager.Do(Document, FilePath);
        //                // }
        //                CallStack.RemoveAt(0);

        //            }
        //        }
        //    }
        //}
        #endregion

        public void Dispose()
        {
            isAlive = false;
            //CallStack.Add(new CallStackTask());
        }
        //protected class CallStackTask
        //{
        //    public List<byte[]> Buffer;
        //    public int Length;
        //    public int CurrentParagraph;
        //    public string CheckingText;
        //    public bool Stop { get; private set; }
        //    public CallStackTask()
        //    {
        //        Stop = true;
        //    }
        //    public CallStackTask(List<byte[]> content, int caretPosition, int count, string text)
        //    {
        //        Buffer = content;
        //        Length = count;
        //        CheckingText = text;
        //        CurrentParagraph = caretPosition;
        //    }
        //}
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
