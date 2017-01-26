using Spire.Doc;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Linq;
using System;
using SpireLicense = Spire.License;

namespace Controls
{
    public class BaseRichTextBox : RichTextBox, IDictionaryManager, IDisposable
    {
        private SaveManager SaveManager;
        private DictionaryManager DictionaryManager;
        private int PreviousParagraphCount;
        private int PrevTextPointer;

        public BaseRichTextBox()
        {
            ContextMenu = new ContextMenu();
            DictionaryManager = new DictionaryManager();
            AutoSave = true;
            DataObject.AddPastingHandler(this, OnPaste);
        }

        public void Dispose()
        {
            DataObject.RemovePastingHandler(this, OnPaste);
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
                    if (SaveManager != null)
                    {
                        SaveManager.Dispose();
                        SaveManager = null;
                    }
                    if (string.IsNullOrEmpty(filePath)) { return; }
                    SaveManager = new SaveManager(filePath, this);
                }
            }
            private get { return filePath; }
        }
        private string filePath = "";

        public int LetterCount { get; set; }
        public int WordCount { get; set; }
        private string ww = "";
        public string WordLetterCount { get { return ww; } set { ww = value; } }
        public bool AutoSave { get; set; }

        public int ParagraphCount
        {
            get { return vParagraphCount; }
            private set
            {
                var list = Document.Blocks.ToList();
                PreviousParagraphCount = vParagraphCount > 0 ? vParagraphCount : 0;
                vParagraphCount = list.Count - 1;
                PrevTextPointer = list.IndexOf(CaretPosition.Paragraph);
                OnParagraphCountChanged();
            }
        }
        private int vParagraphCount;

        private bool OnPasteFlag = false;
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            OnPasteFlag = true;
        }

        private void OnParagraphCountChanged()
        {
            string text = new TextRange(Document.ContentStart, Document.ContentEnd).Text;
            WordCount = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length;
            LetterCount = text.Length;
            ww = "Words: " + WordCount + "; Characters: " + LetterCount;
            if (string.IsNullOrEmpty(text))
            {
                vParagraphCount = 0;
                SaveManager.AddTaskToCallStack(new List<byte[]>(), PrevTextPointer, ParagraphCount, text);
                return;
            }
            var list = Document.Blocks.ToList();
            int stratPosition, endPosisition;
            if (OnPasteFlag && PreviousParagraphCount < ParagraphCount)
            {
                stratPosition = string.IsNullOrWhiteSpace(new TextRange(list[ParagraphCount].ContentStart, list[ParagraphCount].ContentEnd).Text) ? PreviousParagraphCount - 1 : PreviousParagraphCount;
                endPosisition = ParagraphCount;
            }
            else if (OnPasteFlag && PreviousParagraphCount == ParagraphCount)
            {
                stratPosition = PrevTextPointer;
                endPosisition = ParagraphCount;
            }
            else
            {
                stratPosition = list.IndexOf(Selection.Start.Paragraph);
                endPosisition = list.IndexOf(Selection.End.Paragraph);
                int diff = ParagraphCount - PreviousParagraphCount;
                if (endPosisition - stratPosition != diff && diff > 0/*I think diff>0 it's general case, but that fixed your bug need checking equals diff 1 */)
                {
                    stratPosition = endPosisition - ParagraphCount + PreviousParagraphCount;
                }
            }
            stratPosition = stratPosition > 0 ? stratPosition : 0;
            PrevTextPointer = (PrevTextPointer < endPosisition) ? endPosisition : PrevTextPointer;
            OnPasteFlag = false;
            List<byte[]> byteArray = new List<byte[]>();
            for (int i = stratPosition; i <= endPosisition; i++)
            {
                using (var stream = new MemoryStream())
                {
                    TextRange range = new TextRange(list[i].ContentStart, list[i].ContentEnd);
                    range.Save(stream, DataFormats.Rtf);
                    byteArray.Add(stream.ToArray());
                }
            }
            SaveManager.AddTaskToCallStack(byteArray, PrevTextPointer, ParagraphCount, text);
        }
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            ContextMenu.Items.Clear();
            AddSpellCheckingMenuItems(ContextMenu);
            AddBasicMenuItems(ContextMenu);
            AddCustomMenuItems();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (!AutoSave || string.IsNullOrEmpty(FilePath))
            {
                PreviousParagraphCount = Document.Blocks.ToList().Count - 1;
                vParagraphCount = PreviousParagraphCount;
                return;
            }
            ParagraphCount = 0;
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

        public void SaveAsEpub(ExportInfo info, string filePath)
        {
            Document document = new Document(filePath);
            document.BuiltinDocumentProperties.Title = info.Title;
            document.BuiltinDocumentProperties.Author = info.Author;
            document.BuiltinDocumentProperties.CreateDate = (DateTime)info.DatePublish == null ? DateTime.Now : (DateTime)info.DatePublish;
            //    var section=document.AddSection();
            //    section.Document.LoadRtf(@"C:\test.rtf");
            //  section = document.AddSection();
            //    section.Document.LoadRtf(filePath);
            document.SaveToFile(info.SavePath + "\\" + Path.GetFileNameWithoutExtension(filePath) + ".epub", FileFormat.EPub);
        }
        public byte[] ConvertDocumentToPdf(Stream aInputStream)
        {
            byte[] tResult = null;
            using (MemoryStream tOutput = new MemoryStream())
            {
                ConvertDocumentToPdf(aInputStream, tOutput);
                tResult = tOutput.ToArray();
            }
            return tResult;
        }

        public void ConvertDocumentToPdf(Stream aInputStream, Stream aOutputStream)
        {
            SpireLicense.LicenseProvider.SetLicenseKey(@"sGB6g9jPENUBAIRgo6LnJuyXETnXf19qM7KMKXiy12yslrfh84Ukseil96/AQOrfQyQOjO6iFpO+28+EgRa2bQK5voH908vfGNak2EmkwZolFWRHzujVNaA9Y95ctuKYBm4iVcelMWn8P6hVLxnasVhESC+cMbOh3jeQW3zE/BbVNQrff/HC8RzIJKMsc47TlOirmeYdjn697g65EeQWSrf+kwjm46UfAsdwdok46z9K8fCgGZMpeFnMXz/zt+NlQrkpwar/kCyoVvB+Zw2Fgv4CfvsoxCjaPUZZ01FctmvbxXwiSSdJY9hbj9tTaHfcoo5stjEvaES6AKMANgFeXDde1dKDO3//wJMFHHfrXghdlGVl5ZB53SwHSnoyGsem3uvr7rehA9wEs6oDlGFFJdKlmVwy+d5ar0ghVF6RZMSC2nskxvu96isVcbS3wr/CnqCGOQxOvtac5JZRaeKhTqMmQDxi+AzgJFYcQyh6/pwo8HgFy+nw2r5mUl4yZofyHD/ivjoPUTuXDHJWDJYm62WGq5QcwDBrxiMqD1KOMC/iAnZ3OqxrAX5KTbfwbjM1d+c7RrTiA7BdsjtNCxRyzLZUoyLreDwQhY2EYf2h2wth/34Ue1u3bihlup88AeuCKbEpsWmY6pt+tXey7JlTRht+UsOsjLBmGUz0K504ik+EvIBiIA00OkFf1+1VHqGAfBZ/SGbD51Fn3WQmrnfraCbyzXhd0BzyfMGv9elVslmfa7ae4vG+uB+rTheS/WCOTrFpytjuIjRKbSKMog98Bsubd1HQKYxQ5KwRVFAkia+XzAX0HBengmrxZlG5mue8UiNc6gHrZfQvvUOEOu7yCsRjhybwc3+gLUkq2r/Q14mRytVN11k/Fl/iNEB0DN2QHNKKu9maLF0oyT9vf7aYFEUbm9iNWH+PXDsyLbQ7VtmZSBf9/vVfJlZ9+p5TBreJFHO0jur6M6VDNRsokfKm+DZyaMzY4ijpLYzLJ0euYwrm/gFOorNpyZCR+7wjxFVZqEstJSsUvXajzy+R8OfAK4Arr66w6LMhSMCNfGE4U8KkIBz2hR3MJtFI91cI5QKFJXj+kCx1TBHFTSTH0UyOFH4nGS/iE80uFCjGOplwejqvF2MFG8H9u0LdfrF9M3vYBIw6/5ZUlef1oGnwtodz2yYLv+3o5Q7Bk4ixlPRw2hehHLqYQix70MmCzEJBgyqsZGQnV59AJDnGyUQutjX9QFrmEAu6cAotMW8Qoh2KSfv+UGJLuffykK7HgZiGvQYT6L//+QlbNscY7F1CTKuBd8yVOtx/U+H3b6L2LLeby8255zdD6dfBN/1e2Thz+LJGedWpytHW/wqkutDoG8gCaMb8zv+JvRy1kRkTxNiEY3os+u/CLQLht0xdiZO7814RGwiaMyH1ixzIiVmtEJzA6RCXDvjUvhSukY2QwVEJfXZMHyGOqWAdLA==");
            FileFormat tFileFormat = FileFormat.EPub;
            Document tDocument = new Document(aInputStream);
            tDocument.SaveToStream(aOutputStream, tFileFormat);
            tDocument.Close();
        }
        public void SaveAsMobi(ExportInfo info, string filePath)
        {
            SaveAsEpub(info, filePath);
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + "\\" + "kindlegen.exe";
            process.StartInfo.Arguments = info.SavePath + "\\ToEpub.epub";
            process.Start();
            process.WaitForExit();
            File.Delete(info.SavePath + "\\ToEpub.epub");
        }
    }
}
