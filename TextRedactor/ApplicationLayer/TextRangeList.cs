using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace ApplicationLayer
{
    public class TextRangeList<T> where T : TextRange
    {
        private readonly List<TextRange> MainList = new List<TextRange>();
        private FlowDocument Document;
        private int TextRangeLength;
        private static readonly object Equals = new object();

        public TextRangeList(FlowDocument document, int textRangeLength = 20)
        {
            Document = document;
            MainList.Add((T)new TextRange(document.ContentStart, document.ContentEnd));
            TextRangeLength = textRangeLength;
        }

        public void OnTextRangeChanged(int position)
        {
            TextPointer pointer = Document.ContentStart.GetPositionAtOffset(position);
            int n = 0;
            if (!MainList.Any() || MainList[0].Start != Document.ContentStart)
            {
                Repopulate(0);
                return;
            }
            for (int i = 0; i < MainList.Count; i++)
            {
                if (MainList[i].Contains(pointer))
                {
                    n = i;
                }
                if (IsValid(MainList[i])) continue;
                Repopulate(i);
                return;
            }
            OnCollectionChanged(n, Changed.Changed);
        }

        private TextPointer GetEnd(TextPointer pointer)
        {
            TextPointer nextPointer = pointer.GetPositionAtOffset(TextRangeLength);
            if(nextPointer == null)
            {
                return Document.ContentEnd;
            }
            return nextPointer;//.GetNextContextPosition(LogicalDirection.Backward);
        }

        private bool CanCreateItem(TextPointer pointer)
        {
            if (pointer == null) { return false; }
            return pointer.GetPositionAtOffset(1) != null;
        }

        public void Repopulate(int index)
        {
            MainList.RemoveRange(index, MainList.Count - index);
            TextPointer startPoistion = index > 0 ? MainList[index-1].End : Document.ContentStart;
            int length = new TextRange(startPoistion, Document.ContentEnd).Text.Length;
            int temp = 0;
            while (temp < length)
            {
                if (!CanCreateItem(startPoistion)) { break; }
                TextPointer pointer = startPoistion;
                startPoistion = GetEnd(pointer);
                MainList.Add((T)new TextRange(pointer, startPoistion));
                temp += MainList[MainList.Count - 1].Text.Length;
            }
            OnCollectionChanged(index, Changed.Populated);
        }

        private bool IsEquals(object obj, object obj1, Dispatcher disp)
        {
            lock (Equals)
            {
                var range = (TextRange)obj;
                var range1 = (TextRange)obj1;
                if (range == null || range1 == null)
                {
                    return false;
                }
                TextRange rangeUI = null;
                disp.Invoke(() => rangeUI = range);
                return range1.Start.GetOffsetToPosition(range1.End) == rangeUI.Start.GetOffsetToPosition(rangeUI.End); //String.Compare(range.Text, range1.Text, StringComparison.CurrentCulture) == 0;
            }
        }

        public byte[] SereaLize(int index)
        {
            byte[] bytes = new byte[0];
            Application.Current.Dispatcher.Invoke(() =>
            {
                using (var stream = new MemoryStream())
                {
                    MainList[index].Save(stream, DataFormats.Rtf);
                    bytes = stream.ToArray();
                }
            });
            return bytes;
        }

        private TextRange LoadTextRange(TextRange range, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                range.Load(stream, DataFormats.Rtf);
            }
            //if (range.Text.EndsWith(Environment.NewLine))
            //{
            //   return new TextRange(range.Start, range.End.GetPositionAtOffset(2, LogicalDirection.Backward));
            //}
           
            // range.Text = range.Text.TrimEnd(Environment.NewLine.ToCharArray());
            // range = new TextRange(range.Start, range.End.GetNextContextPosition(LogicalDirection.Backward));
            return range;
        }

        public void SynchronizeTo(int from, TextRangeList<T> list)
        {
            list.Document.Dispatcher.Invoke(() => {
            string s = "";
            for (int i = 0; i < list.Count; i++)
            {
                s += list[i].Text;
            }
            Console.WriteLine("IN UI {0}",s);
        });
            MainList.Clear();
            new TextRange(Document.ContentStart, Document.ContentEnd).Text = "";
            //if (!MainList.Any())
            //{
            //    MainList.RemoveRange(from, MainList.Count - from);
            //      new TextRange(MainList[from].End, Document.ContentEnd).Text = "";
            //}
            // Document.ContentEnd = Document.ContentStart;
            //  new TextRange(Document.ContentStart, Document.ContentEnd) = 
            int count = list.Count;
            for (int i = from; i < count; i++)
            {
               // if (Count > i && IsEquals(list[i], this[i], list.Document.Dispatcher)) { continue; }
                if (Count <= i)
                {
                    MainList.Add((T)LoadTextRange(new TextRange(Document.ContentEnd, Document.ContentEnd), list.SereaLize(i)));
                    continue;
                }
                LoadTextRange(this[i], list.SereaLize(i));
            }
        }

        private bool IsValid(TextRange item)
        {
            return item.Text.Length != 0 && item.Text.Length <= TextRangeLength;
        }

        public int Count => MainList.Count;

        private void OnCollectionChanged(int index, Changed state)
        {
            CollectionChanged?.Invoke(index, state);
        }

        public TextRange this[int index] => MainList[index];

        public delegate void CollectionChangedHandler(int index, Changed state);

        public event CollectionChangedHandler CollectionChanged;
    }
    public enum Changed
    {
        Changed = 1,
        Populated = 2,
    }
}
