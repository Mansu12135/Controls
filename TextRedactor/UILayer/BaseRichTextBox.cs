using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media;
using ApplicationLayer;

namespace UILayer
{
    public class BaseRichTextBox : RichTextBox, IDictionaryManager, IDisposable
    {
        private SaveManager SaveManager;
        private DictionaryManager DictionaryManager;
        private int PreviousParagraphCount;
        private int PrevTextPointer;
        private TextRangeList<TextRange> RangeList;
        private Thread Thread { get; set; }

        public BaseRichTextBox()
        {
            ContextMenu = new ContextMenu();
            DictionaryManager = new DictionaryManager();
            AutoSave = true;
            Document.IsOptimalParagraphEnabled = true;
            //DataObject.AddPastingHandler(this, OnPaste);
        }

        public void Dispose()
        {
            StopSaveManager();
            // DataObject.RemovePastingHandler(this, OnPaste);
        }

        internal SuperTextRedactor Parent;

       
        internal string FilePath
        {
            set
            {
                string oldValue = filePath;
                filePath = value;
                if (oldValue != value)
                {
                    if (Thread != null)
                    {
                        StopSaveManager();
                    }
                    if (string.IsNullOrEmpty(filePath)) { return; }
                    CreateSaveManager(filePath);
                }
            }
            private get { return filePath; }
        }
        private string filePath = "";

        public int LetterCount { get; set; }
        public int WordCount { get; set; }
        public string WordLetterCount { get; set; } = "";
        public bool AutoSave { get; set; }

        public void OnCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!AutoSave || string.IsNullOrEmpty(FilePath)) return;
            var richText = sender as RichTextBox;
            if (richText == null) return;
            RangeList?.OnTextRangeChanged(richText.Document.ContentStart.GetOffsetToPosition(richText.Selection.Start));
        }

        //public int ParagraphCount
        //{
        //    get { return vParagraphCount; }
        //    private set
        //    {
        //        var list = Document.Blocks.ToList();
        //        PreviousParagraphCount = vParagraphCount > 0 ? vParagraphCount : 0;
        //        vParagraphCount = list.Count - 1;
        //        PrevTextPointer = list.IndexOf(CaretPosition.Paragraph);
        //        OnParagraphCountChanged();
        //    }
        //}
        //private int vParagraphCount;

        //private bool OnPasteFlag = false;
        //private void OnPaste(object sender, DataObjectPastingEventArgs e)
        //{
        //    OnPasteFlag = true;
        //}

        //private void OnParagraphCountChanged()
        //{
        //    string text = new TextRange(Document.ContentStart, Document.ContentEnd).Text;
        //    WordCount = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length;
        //    LetterCount = text.Length;
        //    ww = "Words: " + WordCount + "; Characters: " + LetterCount;
        //    if (string.IsNullOrEmpty(text))
        //    {
        //        vParagraphCount = 0;
        //        SaveManager.AddTaskToCallStack(new List<byte[]>(), PrevTextPointer, ParagraphCount, text);
        //        return;
        //    }
        //    var list = Document.Blocks.ToList();
        //    int stratPosition, endPosisition;
        //    if (OnPasteFlag && PreviousParagraphCount < ParagraphCount)
        //    {
        //        stratPosition = string.IsNullOrWhiteSpace(new TextRange(list[ParagraphCount].ContentStart, list[ParagraphCount].ContentEnd).Text) ? PreviousParagraphCount - 1 : PreviousParagraphCount;
        //        endPosisition = ParagraphCount;
        //    }
        //    else if (OnPasteFlag && PreviousParagraphCount == ParagraphCount || Selection.IsEmpty)
        //    {
        //        stratPosition = PrevTextPointer;
        //        endPosisition = ParagraphCount;
        //    }
        //    else
        //    {
        //        stratPosition = list.IndexOf(Selection.Start.Paragraph);
        //        endPosisition = list.IndexOf(Selection.End.Paragraph);
        //        int diff = ParagraphCount - PreviousParagraphCount;
        //        if (endPosisition - stratPosition != diff && diff > 0/*I think diff>0 it's general case, but that fixed your bug need checking equals diff 1 */)
        //        {
        //            stratPosition = endPosisition - ParagraphCount + PreviousParagraphCount;
        //        }
        //    }
        //    stratPosition = stratPosition > 0 ? stratPosition : 0;
        //    PrevTextPointer = (PrevTextPointer < endPosisition) ? endPosisition : PrevTextPointer;
        //    OnPasteFlag = false;
        //    List<byte[]> byteArray = new List<byte[]>();
        //    for (int i = stratPosition; i <= endPosisition; i++)
        //    {
        //        using (var stream = new MemoryStream())
        //        {
        //            TextRange range = new TextRange(list[i].ContentStart, list[i].ContentEnd);
        //            range.Save(stream, DataFormats.Rtf);
        //            byteArray.Add(stream.ToArray());
        //        }
        //    }
        //  //  SaveManager.AddTaskToCallStack(byteArray, PrevTextPointer, ParagraphCount, text);
        //}

        public TextPointer GetTextPointAt(TextPointer startingPoint, int offset, LogicalDirection direction = LogicalDirection.Forward)
        {
            TextPointer binarySearchPoint1 = null;
            TextPointer binarySearchPoint2 = null;

            // setup arguments appropriately
            if (direction == LogicalDirection.Forward)
            {
                binarySearchPoint2 = Document.ContentEnd;

                if (offset < 0)
                {
                    offset = Math.Abs(offset);
                }
            }

            if (direction == LogicalDirection.Backward)
            {
                binarySearchPoint2 = Document.ContentStart;

                if (offset > 0)
                {
                    offset = -offset;
                }
            }

            // setup for binary search
            bool isFound = false;
            TextPointer resultTextPointer = null;

            int offset2 = Math.Abs(GetOffsetInTextLength(startingPoint, binarySearchPoint2));
            int halfOffset = direction == LogicalDirection.Backward ? -(offset2 / 2) : offset2 / 2;

            binarySearchPoint1 = startingPoint.GetPositionAtOffset(halfOffset, direction);
            int offset1 = Math.Abs(GetOffsetInTextLength(startingPoint, binarySearchPoint1));

            // binary search loop

            while (isFound == false)
            {
                if (Math.Abs(offset1) == Math.Abs(offset))
                {
                    isFound = true;
                    resultTextPointer = binarySearchPoint1;
                }
                else
                if (Math.Abs(offset2) == Math.Abs(offset))
                {
                    isFound = true;
                    resultTextPointer = binarySearchPoint2;
                }
                else
                {
                    if (Math.Abs(offset) < Math.Abs(offset1))
                    {
                        // this is simple case when we search in the 1st half
                        binarySearchPoint2 = binarySearchPoint1;
                        offset2 = offset1;

                        halfOffset = direction == LogicalDirection.Backward ? -(offset2 / 2) : offset2 / 2;

                        binarySearchPoint1 = startingPoint.GetPositionAtOffset(halfOffset, direction);
                        offset1 = Math.Abs(GetOffsetInTextLength(startingPoint, binarySearchPoint1));
                    }
                    else
                    {
                        // this is more complex case when we search in the 2nd half
                        int rtfOffset1 = startingPoint.GetOffsetToPosition(binarySearchPoint1);
                        int rtfOffset2 = startingPoint.GetOffsetToPosition(binarySearchPoint2);
                        int rtfOffsetMiddle = (Math.Abs(rtfOffset1) + Math.Abs(rtfOffset2)) / 2;
                        if (direction == LogicalDirection.Backward)
                        {
                            rtfOffsetMiddle = -rtfOffsetMiddle;
                        }

                        TextPointer binarySearchPointMiddle = startingPoint.GetPositionAtOffset(rtfOffsetMiddle, direction);
                        int offsetMiddle = GetOffsetInTextLength(startingPoint, binarySearchPointMiddle);

                        // two cases possible
                        if (Math.Abs(offset) < Math.Abs(offsetMiddle))
                        {
                            // 3rd quarter of search domain
                            binarySearchPoint2 = binarySearchPointMiddle;
                            offset2 = offsetMiddle;
                        }
                        else
                        {
                            // 4th quarter of the search domain
                            binarySearchPoint1 = binarySearchPointMiddle;
                            offset1 = offsetMiddle;
                        }
                    }
                }
            }

            return resultTextPointer;
        }

        public void Test(string value)
        {
            Regex reg = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            TextPointer position = Document.ContentStart;
            List<TextRange> ranges = new List<TextRange>();
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = position.GetTextInRun(LogicalDirection.Forward);
                    var matchs = reg.Matches(text);

                    foreach (Match match in matchs)
                    {

                        TextPointer start = position.GetPositionAtOffset(match.Index);
                        TextPointer end = start.GetPositionAtOffset(value.Trim().Length);

                        TextRange textrange = new TextRange(start, end);
                        ranges.Add(textrange);
                    }
                }
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }


            foreach (TextRange range in ranges)
            {
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
                range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }
        }

        private int GetOffsetInTextLength(TextPointer pointer1, TextPointer pointer2)
        {
            if (pointer1 == null || pointer2 == null)
                return 0;

            TextRange tr = new TextRange(pointer1, pointer2);

            return tr.Text.Length;
        }

        private void StopSaveManager()
        {
            if (SaveManager == null) { return; }
            SaveManager.Dispose();
            Thread.Join();
            Thread.Abort();
            Thread = null;
        }

        private void CreateSaveManager(string filePath)
        {
            RangeList = new TextRangeList<TextRange>(Document);
            SaveManager = new SaveManager();
            Thread = new Thread(() => SaveManager.DoStart(filePath, RangeList));
            Thread.SetApartmentState(ApartmentState.STA);
            Thread.Start();
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            //Selection.Select(Document.ContentStart, Document.Blocks.ToList()[1].ContentStart);
           // Selection.Select(Document.Blocks.ToList()[2].ContentStart, Document.Blocks.ToList()[5].ContentStart);
            ContextMenu.Items.Clear();
            AddSpellCheckingMenuItems(ContextMenu);
            AddBasicMenuItems(ContextMenu);
            AddCustomMenuItems();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (!AutoSave || string.IsNullOrEmpty(FilePath) || e.Changes.Count == 0) return;
            var change = e.Changes.Where(x=>x.AddedLength!=x.RemovedLength).OrderByDescending(item => item.Offset + item.AddedLength + item.RemovedLength).First();
            RangeList?.OnTextRangeChanged(change.Offset);
            //PreviousParagraphCount = Document.Blocks.ToList().Count - 1;
            //vParagraphCount = PreviousParagraphCount;
            //return;
            //ParagraphCount = 0;
        }

        private MenuItem MenuItemDictionary;

        private void AddCustomMenuItems()
        {
            ContextMenu.Items.Add(new Separator());
            MenuItemDictionary = new MenuItem() { Header = "Look up in dictionary", Tag = "dictionary" };
            MenuItemDictionary.Click += Parent.ButtonDictionary_MouseUp;
            ContextMenu.Items.Add(MenuItemDictionary);
        }
        private void AddSpellCheckingMenuItems(ContextMenu menu)
        {
            MenuItem menuItem;
            SpellingError spellingError = GetSpellingError(CaretPosition);
            if (spellingError == null)
            {
                menuItem = new MenuItem() { Header = "No Suggestions", IsEnabled = false };
                ContextMenu.Items.Add(menuItem);
            }
            else
            {
                foreach (string suggestion in spellingError.Suggestions)
                {
                    menuItem = new MenuItem()
                    {
                        Header = suggestion,
                        FontWeight = FontWeights.Bold,
                        Command = EditingCommands.CorrectSpellingError,
                        CommandParameter = suggestion,
                        CommandTarget = this
                    };
                    menu.Items.Add(menuItem);
                }
            }
            ContextMenu.Items.Add(new Separator());
            menuItem = new MenuItem() { Header = "Ignore All", Command = EditingCommands.IgnoreSpellingError, CommandTarget = this };
            ContextMenu.Items.Add(menuItem);
            ContextMenu.Items.Add(new Separator());
        }
        private void AddBasicMenuItems(ContextMenu menu)
        {
            ContextMenu.Items.Add(new MenuItem() { Command = ApplicationCommands.Cut });
            ContextMenu.Items.Add(new MenuItem() { Command = ApplicationCommands.Copy });
            ContextMenu.Items.Add(new MenuItem() { Command = ApplicationCommands.Paste });
            ContextMenu.Items.Add(new Separator());
            ContextMenu.Items.Add(new MenuItem() { Command = ApplicationCommands.Delete });
            ContextMenu.Items.Add(new Separator());
            ContextMenu.Items.Add(new MenuItem() { Command = ApplicationCommands.SelectAll });
        }
        public void AddMenuItem(object menuItem)
        {
            ContextMenu.Items.Add(menuItem);
        }
        public List<Structure> GetInformation(string word)
        {
            return DictionaryManager.GetInformation(word);
        }
        public List<string> GetSynonyms(string word)
        {
            return DictionaryManager.GetSynonyms(word);
        }
        public List<string> GetAntonyms(string word)
        {
            return DictionaryManager.GetAntonyms(word);
        }

        public void SaveAsEpub(ExportInfo info, List<LoadedFile> files)
        {
            //var document = new Document(InputFormat.rtf);
            //foreach (var file in files)
            //{
            //    document.AddFile(file.Path);
            //}
            //document.Author = info.Author;
            //document.Language = "en";
            //document.Publisher = "Home entertainment";
            //document.PublicationTime = (DateTime.Today);// info.DatePublish;
            //document.Rights = "all";
            //document.Title = info.Title;
            //if (info.ImagePath != null && !string.IsNullOrEmpty(info.ImagePath))
            //{
            //    document.PathToCover = info.ImagePath;
            //}
            //var convertor = new Convertor(document);
            //convertor.CreateEpub(info.SavePath.EndsWith("\\")? info.SavePath: (info.SavePath+"\\") + info.Title + ".epub");
            //  HackDocument document = new HackDocument(filePath);
            //  document.BuiltinDocumentProperties.Title = info.Title;
            //  document.BuiltinDocumentProperties.Author = info.Author;
            //document.BuiltinDocumentProperties.CreateDate = (DateTime)info.DatePublish == null ? DateTime.Now : (DateTime)info.DatePublish;
            //    var section=document.AddSection();
            //    section.Document.LoadRtf(@"C:\test.rtf");
            //  section = document.AddSection();
            //    section.Document.LoadRtf(filePath);
            //   document.PremiumSave(info.SavePath + "\\" + Path.GetFileNameWithoutExtension(filePath) + ".epub", FileFormat.EPub);
        }
        //public byte[] ConvertDocumentToPdf(Stream aInputStream)
        //{
        //    byte[] tResult = null;
        //    using (MemoryStream tOutput = new MemoryStream())
        //    {
        //        ConvertDocumentToPdf(aInputStream, tOutput);
        //        tResult = tOutput.ToArray();
        //    }
        //    return tResult;
        //}

        //public void ConvertDocumentToPdf(Stream aInputStream, Stream aOutputStream)
        //{
        //  //  SpireLicense.LicenseProvider.SetLicenseKey(@"sGB6g9jPENUBAIRgo6LnJuyXETnXf19qM7KMKXiy12yslrfh84Ukseil96/AQOrfQyQOjO6iFpO+28+EgRa2bQK5voH908vfGNak2EmkwZolFWRHzujVNaA9Y95ctuKYBm4iVcelMWn8P6hVLxnasVhESC+cMbOh3jeQW3zE/BbVNQrff/HC8RzIJKMsc47TlOirmeYdjn697g65EeQWSrf+kwjm46UfAsdwdok46z9K8fCgGZMpeFnMXz/zt+NlQrkpwar/kCyoVvB+Zw2Fgv4CfvsoxCjaPUZZ01FctmvbxXwiSSdJY9hbj9tTaHfcoo5stjEvaES6AKMANgFeXDde1dKDO3//wJMFHHfrXghdlGVl5ZB53SwHSnoyGsem3uvr7rehA9wEs6oDlGFFJdKlmVwy+d5ar0ghVF6RZMSC2nskxvu96isVcbS3wr/CnqCGOQxOvtac5JZRaeKhTqMmQDxi+AzgJFYcQyh6/pwo8HgFy+nw2r5mUl4yZofyHD/ivjoPUTuXDHJWDJYm62WGq5QcwDBrxiMqD1KOMC/iAnZ3OqxrAX5KTbfwbjM1d+c7RrTiA7BdsjtNCxRyzLZUoyLreDwQhY2EYf2h2wth/34Ue1u3bihlup88AeuCKbEpsWmY6pt+tXey7JlTRht+UsOsjLBmGUz0K504ik+EvIBiIA00OkFf1+1VHqGAfBZ/SGbD51Fn3WQmrnfraCbyzXhd0BzyfMGv9elVslmfa7ae4vG+uB+rTheS/WCOTrFpytjuIjRKbSKMog98Bsubd1HQKYxQ5KwRVFAkia+XzAX0HBengmrxZlG5mue8UiNc6gHrZfQvvUOEOu7yCsRjhybwc3+gLUkq2r/Q14mRytVN11k/Fl/iNEB0DN2QHNKKu9maLF0oyT9vf7aYFEUbm9iNWH+PXDsyLbQ7VtmZSBf9/vVfJlZ9+p5TBreJFHO0jur6M6VDNRsokfKm+DZyaMzY4ijpLYzLJ0euYwrm/gFOorNpyZCR+7wjxFVZqEstJSsUvXajzy+R8OfAK4Arr66w6LMhSMCNfGE4U8KkIBz2hR3MJtFI91cI5QKFJXj+kCx1TBHFTSTH0UyOFH4nGS/iE80uFCjGOplwejqvF2MFG8H9u0LdfrF9M3vYBIw6/5ZUlef1oGnwtodz2yYLv+3o5Q7Bk4ixlPRw2hehHLqYQix70MmCzEJBgyqsZGQnV59AJDnGyUQutjX9QFrmEAu6cAotMW8Qoh2KSfv+UGJLuffykK7HgZiGvQYT6L//+QlbNscY7F1CTKuBd8yVOtx/U+H3b6L2LLeby8255zdD6dfBN/1e2Thz+LJGedWpytHW/wqkutDoG8gCaMb8zv+JvRy1kRkTxNiEY3os+u/CLQLht0xdiZO7814RGwiaMyH1ixzIiVmtEJzA6RCXDvjUvhSukY2QwVEJfXZMHyGOqWAdLA==");
        //    FileFormat tFileFormat = FileFormat.EPub;
        //    Document tDocument = new Document(aInputStream);
        //    tDocument.SaveToStream(aOutputStream, tFileFormat);
        //    tDocument.Close();
        //}
        public void SaveAsMobi(ExportInfo info, string filePath)
        {
            //SaveAsEpub(info, filePath);
            //var process = new Process();
            //process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardOutput = true;
            //process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + "\\" + "kindlegen.exe";
            //process.StartInfo.Arguments = info.SavePath + "\\ToEpub.epub";
            //process.Start();
            //process.WaitForExit();
            //File.Delete(info.SavePath + "\\ToEpub.epub");
        }
    }
}
