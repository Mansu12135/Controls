using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Cursors = System.Windows.Input.Cursors;
using UserControl = System.Windows.Controls.UserControl;


namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для SuperTextRedactor.xaml
    /// </summary>
    public partial class SuperTextRedactor : UserControl, IDisposable
    {
        private Command OnlyTextCommand;
        private Command OnlyProjectCommand;
        private Command ProjectAndNoteCommand;
        private Window Parent;
        private List<SearchResult> ResultSearch;
        private int FlowPosition;
        private int activeFindIndex;

        public double defaultFont;
        public double defaultSpacing;
        public double defaultMarginWight;

        public SuperTextRedactor()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            BrowseProject.ParentControl = this;
            NotesBrowser.ParentControl = this;
            Format.ButtonAddNote.MouseUp += ButtonAddNote_MouseUp;
            Format.LabelImage.MouseUp += LabelImage_MouseUp;
            RichTextBoxCommandBindings.IntializeCommandBindings(TextBox.MainControl);
            Format.fontItalic.CommandTarget = TextBox.MainControl;
            Format.FontFamily.CommandTarget = TextBox.MainControl;
            Format.comboBoxFont.CommandTarget = TextBox.MainControl;
            Format.fontUnder.CommandTarget = TextBox.MainControl;
            Format.fontBold.CommandTarget = TextBox.MainControl;
            Format.alignLeft.CommandTarget = TextBox.MainControl;
            Format.alignCenter.CommandTarget = TextBox.MainControl;
            Format.alignAll.CommandTarget = TextBox.MainControl;
            Format.alignRight.CommandTarget = TextBox.MainControl;
            Format.comboBoxFont.SelectionChanged += ((s, e) => TextBox.MainControl.Focus());
            Format.FontFamily.SelectionChanged += ((s, e) => TextBox.MainControl.Focus());
            // Format.ButtonDictionary.MouseUp += ButtonDictionary_MouseUp;
            Format.ExportButton.MouseUp += ExportButton_MouseUp;
            BrowseProject.HidenProject.MouseUp += HidenProject_MouseUp;
            NotesBrowser.HidenNotes.MouseUp += HidenNotes_MouseUp;
            TextBox.MainControl.Parent = this;
            Format.ColorPicker1.SelectedColorChanged += ColorPicker1_SelectedColorChanged;
            Format.comboWigth.SelectionChanged += ComboWigth_SelectionChanged;
            Format.comboWigth.LostFocus += ComboWigth_LostFocus;
            Format.comboWigth.KeyDown += ComboWigth_KeyDown;
        }



        private void ComboWigth_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                TextBox.MainControl.Focus();
            }
        }

        private void ComboWigth_LostFocus(object sender, RoutedEventArgs e)
        {
            double res;
            if (Double.TryParse(Format.comboWigth.Text, out res))
            {
                if (res > 0 && res <= System.Windows.Forms.SystemInformation.VirtualScreen.Width)
                {
                    TextBox.MainControl.Document.PageWidth = res;
                }
                else
                {
                    Format.comboWigth.Text = TextBox.MainControl.Document.PageWidth.ToString();
                }
            }
            else
            {
                Format.comboWigth.Text = TextBox.MainControl.Document.PageWidth.ToString();
            }
        }

        private void ColorPicker1_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            new TextRange(TextBox.MainControl.Selection.Start, TextBox.MainControl.Selection.End).ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Format.ColorPicker1.SelectedColor));
            TextBox.MainControl.Focus();
        }

        private void ComboWigth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Format.comboWigth.SelectedValue != null)
            {
                TextBox.MainControl.Document.PageWidth = Convert.ToDouble(Format.comboWigth.SelectedValue);
            }
            //else
            //{
            //    double res;
            //    if (Double.TryParse(Format.comboWigth.Text, out res))
            //    {
            //        if (res > 0 && res <= System.Windows.Forms.SystemInformation.VirtualScreen.Width)
            //        {
            //            TextBox.MainControl.Document.PageWidth = res;
            //        }
            //    }
            //}

        }
        public void SetTextWidth(string value)
        {
            defaultMarginWight = (Convert.ToDouble(value));
            Format.comboWigth.SelectedIndex = Format.comboWigth.Items.IndexOf(defaultMarginWight);
            //  Format.comboWigth.Text = Format.comboWigth.Items.IndexOf(defaultMarginWight).ToString();
        }
        public void SetTextFont(string value)
        {
            defaultFont = Convert.ToDouble(value);
            Format.comboBoxFont.SelectedIndex = Format.comboBoxFont.Items.IndexOf(defaultFont);
        }
        public void SetTextInterval(string value)
        {
            defaultSpacing = Convert.ToDouble(value);
            TextBox.MainControl.Document.LineHeight = defaultSpacing;
        }
        private void OnlyText()
        {
            if (ShowNotes.Visibility == Visibility.Hidden)
            {
                HidenNotes_MouseUp(null, null);
            }
            if (ShowProject.Visibility == Visibility.Hidden)
            {
                HidenProject_MouseUp(null, null);
            }
        }

        private void OnlyProject()
        {
            if (ShowNotes.Visibility == Visibility.Hidden)
            {
                HidenNotes_MouseUp(null, null);
            }
            if (ShowProject.Visibility == Visibility.Visible)
            {
                ShowProject_MouseUp(null, null);
            }
        }
        private void ProjectAndNotes()
        {
            if (ShowNotes.Visibility == Visibility.Visible)
            {
                ShowNotes_MouseUp(null, null);
            }
            if (ShowProject.Visibility == Visibility.Visible)
            {
                ShowProject_MouseUp(null, null);
            }
        }
        private void AnimateMargin(Thickness from, Thickness to, object control, bool hide, System.Windows.Controls.Label label = null)
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = from;
            myThicknessAnimation.To = to;
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            if (label != null)
            {
                myThicknessAnimation.Completed += new EventHandler((s, r) => label.Visibility = hide ? Visibility.Visible : Visibility.Hidden);
            }
            ((Border)control).BeginAnimation(Border.MarginProperty, myThicknessAnimation);
        }
        private void HidenNotes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AnimateMargin(RedactorConteiner.Margin, new Thickness(RedactorConteiner.Margin.Left, RedactorConteiner.Margin.Top, -NotesBrowser.ActualWidth, RedactorConteiner.Margin.Bottom), RedactorConteiner, true, ShowNotes);
        }

        private void HidenProject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AnimateMargin(RedactorConteiner.Margin, new Thickness(-BrowseProject.ActualWidth, RedactorConteiner.Margin.Top, RedactorConteiner.Margin.Right, RedactorConteiner.Margin.Bottom), RedactorConteiner, true, ShowProject);
        }

        public void Dispose()
        {
            BrowseProject.Dispose();
        }

        private void ExportButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            exportPanel = new ExportPanel();
            exportPanel.HidenExport.MouseUp += new MouseButtonEventHandler((s, r) => { Container.Child = null; });
            exportPanel.Name = "export";
            exportPanel.ButExport.MouseUp += ButExport_MouseUp;
            exportPanel.project = BrowseProject.CurentProject;
            exportPanel.Init();
            Container.Child = exportPanel;
        }

        private void ButExport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string folderPath = "";
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                folderPath = folderBrowserDialog.SelectedPath;
                ExportInfo exportInfo = new ExportInfo()
                {
                    Title = exportPanel.TextBoxTitle.Text,
                    Author = exportPanel.TextBoxAuthor.Text,
                    DatePublish = exportPanel.TextBoxPublishingDate.SelectedDate,
                    SavePath = folderPath
                };
                TextBox.MainControl.SaveAsEpub(exportInfo, BrowseProject.CurentFile);
            }
        }

        ExportPanel exportPanel;
        DictionaryPanel panel;
        SearchPanel searchPanel;
        private void InitDictionary()
        {
            if (panel == null)
            {
                panel = new DictionaryPanel();
                panel.HidenDictionary.MouseUp += new MouseButtonEventHandler((s, r) => { Container.Child = null; panel = null; });
                panel.Name = "dictionary";
                panel.TextWord.KeyUp += TextWord_KeyUp;
                Container.Child = panel;
            }
            panel.TextWord.Text = GetCurrentWord();// this.TextBox.MainControl.Selection.Text;
            ShowDictionaryResult(panel.TextWord.Text);
            //TODO
            //есле уже true то оно не обновляет, пока что дописала доп условие
            panel.ModeDictionary.IsChecked = !panel.ModeDictionary.IsChecked;
            panel.ModeDictionary.IsChecked = true;
        }
        private string GetCurrentWord()
        {
            if (!string.IsNullOrEmpty(TextBox.MainControl.Selection.Text))
            {
                return TextBox.MainControl.Selection.Text;
            }
            string textBefore = TextBox.MainControl.CaretPosition.GetTextInRun(LogicalDirection.Backward);
            string textAfter = TextBox.MainControl.CaretPosition.GetTextInRun(LogicalDirection.Forward);

            /*определяем позицию начала слова на котором стоит курсор*/
            int StartCharPossition = textBefore.LastIndexOfAny(new char[] { ' ', '.', ',', ';', ':', '-' });

            /*определяем позицию конца слова на котором стоит курсор*/
            int EndCharPossition = textAfter.IndexOfAny(new char[] { ' ', '.', ',', ';', ':', '-' });

            /*обрезаем строки*/
            if (StartCharPossition > -1)
                textBefore = textBefore.Substring(StartCharPossition + 1);
            if (EndCharPossition > -1)
                textAfter = textAfter.Substring(0, EndCharPossition);

            return textBefore + textAfter;
        }
        internal void ButtonDictionary_MouseUp(object sender, RoutedEventArgs e)
        {
            InitDictionary();
        }

        private void TextWord_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!String.IsNullOrEmpty(panel.TextWord.Text))
                {
                    ShowDictionaryResult(panel.TextWord.Text);
                    //TODO
                    //есле уже true то оно не обновляет, пока что дописала доп условие
                    panel.ModeDictionary.IsChecked = !panel.ModeDictionary.IsChecked;
                    panel.ModeDictionary.IsChecked = true;
                }
            }

        }
        private void ShowDictionaryResult(string word)
        {
            string defenition = "";
            string thesaurus = "";
            List<Structure> result = TextBox.MainControl.GetInformation(panel.TextWord.Text);
            if (result == null) return;
            foreach (var item in result)
            {
                defenition += item.Term + Environment.NewLine + item.PartOfSpeech + Environment.NewLine;
                foreach (var def in item.Definitions)
                    defenition += def + Environment.NewLine;
                thesaurus += "Synonyms:" + Environment.NewLine;
                foreach (var def in item.Synonyms)
                    thesaurus += def + Environment.NewLine;
                thesaurus += "Antonyms:" + Environment.NewLine;
                foreach (var def in item.Antonyms)
                    thesaurus += def + Environment.NewLine;
            }
            panel.defenition = defenition;
            panel.synonimus = thesaurus;
        }
        void LabelImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog open_dialog = new OpenFileDialog(); //создание диалогового окна для выбора файла
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*"; //формат загружаемого файла
            if (open_dialog.ShowDialog() == DialogResult.OK) //если в окне была нажата кнопка "ОК"
            {
                try
                {
                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Stretch = Stretch.Fill;
                    image.Cursor = System.Windows.Input.Cursors.Hand;
                    //image.Height = TextBox.MainControl.ActualWidth;
                    image.Width = TextBox.MainControl.ActualWidth - TextBox.MainControl.Padding.Left * 4;
                    BitmapImage bmp = new BitmapImage(new Uri(open_dialog.FileName, UriKind.RelativeOrAbsolute));
                    image.Source = bmp;
                    image.Stretch = Stretch.Fill;
                    TextPointer p = TextBox.MainControl.CaretPosition;
                    TextBox.MainControl.BeginChange();
                    InlineUIContainer imageContainer = new InlineUIContainer(image, p);
                    TextBox.MainControl.EndChange();
                    TextBox.MainControl.Focus();
                    //  TextBox.MainControl.CaretPosition = imageContainer.ElementEnd;

                }
                catch
                {
                    DialogResult rezult = System.Windows.Forms.MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        void ButtonAddNote_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AddNote(new TextRange(TextBox.MainControl.Selection.Start, TextBox.MainControl.Selection.End).Text);
            AddFlag();
            NotesBrowser.MainControl.Items.Refresh();
        }
        public void AddNote(string text)
        {
            int start = new TextRange(TextBox.MainControl.Document.ContentStart, TextBox.MainControl.Selection.Start).Text.Length;
            int end = new TextRange(TextBox.MainControl.Document.ContentStart, TextBox.MainControl.Selection.End).Text.Length;
            int dif = 0;
            foreach (var note in NotesBrowser.Notes)
            {
                if (note.Value.OffsetStart < start) dif++;
            }
            start -= dif;
            end -= dif;
            NotesBrowser.AddItem(new Note(NotesBrowser.GenerateName("Note"), text, start, end));
        }

        private void AddFlag()
        {
            new TextRange(TextBox.MainControl.Selection.Start, TextBox.MainControl.Selection.End).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.PaleGreen);
            var tempImage = Properties.Resources.noteFlag;
            var ScreenCapture = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
      tempImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(20, 20));

            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = ScreenCapture;
            image.Stretch = Stretch.Fill;
            image.Cursor = Cursors.Hand;
            image.Height = 14;
            image.Width = 14;
            image.Tag = NotesBrowser.Notes.Last().Key;
            image.MouseUp += Image_MouseUp;
            TextPointer p = TextBox.MainControl.Selection.Start;
            TextBox.MainControl.BeginChange();
            InlineUIContainer imageContainer = new InlineUIContainer(image, p);
            TextBox.MainControl.EndChange();
            TextBox.MainControl.Focus();
            //  TextBox.MainControl.CaretPosition = imageContainer.ElementEnd;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as Image;
            if (item != null)
            {
                ClickOnFlag(item.Tag.ToString());
            }
        }
        public void ClickOnFlag(string key)
        {
            NotesBrowser.MainControl.SelectedIndex = NotesBrowser.MainControl.Items.IndexOf(NotesBrowser.Notes[key]);
        }
        private bool GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
            return true;
        }


        private FlowDocument LoadFile(string path)
        {
            FlowDocument document = new FlowDocument();
            FileWorkerManager.Do(document, path, false);
            //using (var stream = new FileStream(path, FileMode.Open))
            //{
            //    var range = new TextRange(document.ContentStart, document.ContentEnd);
            //    range.Load(stream, DataFormats.Rtf);
            //}
            return document;
        }

        private void SaveChanges(SearchResult rezult, FlowDocument document)
        {
            FileWorkerManager.Do(document, rezult.Path);
            //using (var stream = new FileStream(rezult.Path, FileMode.Open))
            //{
            //    new TextRange(document.ContentStart, document.ContentEnd).Save(stream, DataFormats.Rtf);
            //}
        }

        private int Replace(List<SearchResult> found, string to)
        {
            int count = 0;
            if (found.Any())
            {
                FlowPosition = 0;
                var document = LoadFile(found[0].Path);
                foreach (var item in found)
                {
                    var range =
                        new TextRange(
                            BrowseProject.GetTextPointAt(document.ContentStart, item.Position + FlowPosition),
                            BrowseProject.GetTextPointAt(document.ContentStart,
                                item.Position + item.Text.Length + FlowPosition));
                    if (range.Text.Trim() != item.Text.Trim())
                    {
                        FlowPosition = 0;
                        return 0;
                    }
                    FlowPosition += to.Length - range.Text.Length;
                    range.Text = to;
                    count++;
                }
                SaveChanges(found[0], document);
            }
            // TextBox.MainControl.Document.Focus();
            return count;
        }

        private int ReplaceAll(List<SearchResult> founds, string to)
        {
            int count = 0;
            var list = founds.GroupBy(r => r.Path).ToList();
            foreach (var item in list)
            {
                count += Replace(item.OfType<SearchResult>().ToList(), to);
            }
            return count;
        }

        public int Replace(string from, string to, FindReplaceExpression expression)
        {
            int count = 0;
            switch (expression)
            {
                case FindReplaceExpression.FirstFound:
                    {
                        var list = new List<SearchResult>();
                        list.Add(ResultSearch.ElementAt(activeFindIndex));
                        count = Replace(list, to);
                        if (count > 0)
                        {
                            ResultSearch.Remove(ResultSearch.ElementAt(activeFindIndex));
                        }
                        break;
                    }
                case FindReplaceExpression.InThisPage:
                    {
                        count = 0;
                        break;
                    }
                case FindReplaceExpression.InThisParagraph:
                    {
                        count = 0;
                        break;
                    }
                case FindReplaceExpression.InThisBook:
                    {
                        count = ReplaceAll(Search(from, expression), to);
                        ResultSearch.Clear();
                        break;
                    }
            }
            ResultSearch.ForEach(item =>
                item.Position += FlowPosition);
            BrowseProject.OpenFile(BrowseProject.CurentFile, Path.GetFileNameWithoutExtension(BrowseProject.CurentFile));
            NotesBrowser.MainControl.Items.Refresh();
            return count;
        }

        private SearchResult Find(string therm, string file)
        {
            var document = LoadFile(file);
            TextPointer current = document.ContentStart;
            int index;
            int count = 0;
            foreach (var block in document.Blocks)
            {
                string textInRun = new TextRange(block.ContentStart, block.ContentEnd).Text;
                if (!string.IsNullOrWhiteSpace(textInRun))
                {
                    index = textInRun.IndexOf(therm);
                    if (index != -1)
                    {
                        return new SearchResult { Position = index + count, Path = file, Text = therm };// new TextRange(document.ContentStart.GetPositionAtOffset(index), document.ContentStart.GetPositionAtOffset(index + therm.Length)).Text });
                    }
                    count += textInRun.Length;
                }
            }
            //    while (current != null)
            //{
            //    int index;
            //    string textInRun = current.GetTextInRun(LogicalDirection.Forward);
            //    if (!string.IsNullOrWhiteSpace(textInRun))
            //    {
            //        index = textInRun.IndexOf(therm);
            //        if (index != -1)
            //        {
            //            return new KeyValuePair<int, SearchResult>(index, new SearchResult { Path = file, Text = new TextRange(document.ContentStart.GetPositionAtOffset(index), document.ContentStart.GetPositionAtOffset(index + therm.Length)).Text });
            //        }
            //    }
            //    current = current.GetNextContextPosition(LogicalDirection.Forward);
            //}
            return null;
        }

        private void SelectSearchWord(string file, List<SearchResult> items)
        {
            foreach (var item in items)
            {
                if (item.Path == file)
                {
                    new TextRange
                        (BrowseProject.GetTextPointAt(TextBox.MainControl.Document.ContentStart, item.Position),
                        BrowseProject.GetTextPointAt(TextBox.MainControl.Document.ContentStart, item.Position + item.Text.Length))
                        .ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
                }
            }
        }
        private void Founds(string textInRun, string threm, string file, int paragraphPosition, ref List<SearchResult> items)
        {
            if (!string.IsNullOrWhiteSpace(textInRun))
            {
                int count = 0;
                while (count != -1)
                {
                    count = textInRun.IndexOf(threm, count);
                    if (count > -1)
                    {
                        items.Add(new SearchResult { Position = paragraphPosition + count, Path = file, Text = threm });
                        count += threm.Length;
                    }
                }
            }
        }

        private List<SearchResult> FindAll(string therm, string file)
        {
            var list = new List<SearchResult>();
            var document = LoadFile(file);
            foreach (var block in document.Blocks)
            {
                Founds(new TextRange(block.ContentStart, block.ContentEnd).Text, therm, file, new TextRange(document.ContentStart, block.ContentStart).Text.Replace("\r\n", string.Empty).Length, ref list);
            }
            BrowseProject.GetTextPointAt(document.ContentStart, 680);
            return list;
        }

        public List<SearchResult> Search(string therm, FindReplaceExpression expression)
        {
            var list = new List<SearchResult>();
            switch (expression)
            {
                case FindReplaceExpression.InThisPage:
                    {
                        var temp = FindAll(therm, BrowseProject.CurentFile);
                        foreach (var rezult in temp)
                        {
                            list.Add(rezult);
                        }
                        break;
                    }
                case FindReplaceExpression.InThisBook:
                    {
                        string project = new FileInfo(BrowseProject.CurentFile).Directory.Parent.Name;
                        if (!BrowseProject.Notes.ContainsKey(project)) { break; }
                        foreach (var file in BrowseProject.Notes[project].Files)
                        {
                            var temp = FindAll(therm, file.Path);
                            foreach (var rezult in temp)
                            {
                                list.Add(rezult);
                            }
                        }
                        break;
                    }
            }
            return list;
        }

        public enum FindReplaceExpression
        {
            FirstFound = 0,
            InThisPage = 1,
            InThisParagraph = 2,
            InThisBook = 3
        }
        public void InitFullScr(Window w)
        {
            Parent = w;
            OnlyTextCommand = new Command(Parent)
            {
                Name = "OnlyText",
                KeyShortCut = Key.D1,
                OnExecute = OnlyText
            };
            OnlyProjectCommand = new Command(Parent)
            {
                Name = "OnlyProject",
                KeyShortCut = Key.D2,
                OnExecute = OnlyProject
            };
            ProjectAndNoteCommand = new Command(Parent)
            {
                Name = "OnlyProject",
                KeyShortCut = Key.D3,
                OnExecute = ProjectAndNotes
            };
            MenuFullScr.Click += (s, e) => { w.WindowState = WindowState.Maximized; };
        }
        private void ShowProject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AnimateMargin(RedactorConteiner.Margin, new Thickness(0, RedactorConteiner.Margin.Top, RedactorConteiner.Margin.Right, RedactorConteiner.Margin.Bottom), RedactorConteiner, false, ShowProject);
            ShowProject.Visibility = Visibility.Hidden;
        }
        private void ShowNotes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AnimateMargin(RedactorConteiner.Margin, new Thickness(RedactorConteiner.Margin.Left, RedactorConteiner.Margin.Top, 0, RedactorConteiner.Margin.Bottom), RedactorConteiner, false, ShowNotes);
        }

        private void MenuAddPr_Click(object sender, RoutedEventArgs e)
        {
            BrowseProject.CreateProject();
        }

        private void MenuSearch_Click(object sender, RoutedEventArgs e)
        {
            if (searchPanel == null)
            {
                searchPanel = new SearchPanel();
                searchPanel.HidenSearch.MouseUp += new MouseButtonEventHandler((s, r) => { Container.Child = null; searchPanel = null; });
                searchPanel.GoToNextFind.MouseUp += GoToNextFind_MouseUp;
                searchPanel.ButReplace.MouseUp += ButReplace_MouseUp;
                searchPanel.TextWord.KeyUp += TextWord_KeyUp1;
                searchPanel.Name = "search";
                Container.Child = searchPanel;
            }
            else
            {
                Container.Child = searchPanel;
            }
            GetSearchResalt(GetCurrentWord());
            //   TextBox.MainControl.CaretPosition = BrowseProject.GetTextPointAt(TextBox.MainControl.Document.ContentStart, ResultSearch.ElementAt(activeFindIndex).Key);
        }
        private void GetSearchResalt(string value)
        {
            ResultSearch = Search(value, FindReplaceExpression.InThisPage);
            SelectSearchWord(BrowseProject.CurentFile, ResultSearch);
            searchPanel.SearchResult.ItemsSource = ResultSearch;
            searchPanel.SearchResult.SelectionChanged += SearchResult_SelectionChanged;
            searchPanel.TextWord.Text = GetCurrentWord();
            activeFindIndex = 0;
            if (ResultSearch.Count > 0)
            {
                searchPanel.SearchResult.SelectedIndex = 0;
            }
        }
        private void TextWord_KeyUp1(object sender, System.Windows.Input.KeyEventArgs e)
        {
            GetSearchResalt(searchPanel.TextWord.Text);
        }

        private void RefreshAfterSearch(FlowDocument document)
        {

            foreach (var block in document.Blocks)
            {
                foreach (var inline in ((Paragraph)block).Inlines)
                {
                    if (inline.Background == new SolidColorBrush(Colors.Yellow))
                    {
                        inline.Background = new SolidColorBrush(Colors.White);
                        // new TextRange(i.ContentStart, i.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, System.Windows.Media.Brushes.White);
                    }

                }
            }
        }
        private void ButReplace_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((bool)searchPanel.ReplaceOnce.IsChecked)
            {
                Replace(searchPanel.TextWord.Text, searchPanel.TextWordReplace.Text, FindReplaceExpression.FirstFound);
            }
            else if ((bool)searchPanel.ReplaceAll.IsChecked)
            {
                Replace(searchPanel.TextWord.Text, searchPanel.TextWordReplace.Text, FindReplaceExpression.InThisBook);
            }
            searchPanel.SearchResult.Items.Refresh();
        }

        private void SearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as System.Windows.Controls.ListBox;
            if (list == null) { return; }
            activeFindIndex = list.SelectedIndex;
            if (activeFindIndex != -1)
            {
                TextBox.MainControl.Focus();
                TextBox.MainControl.CaretPosition = BrowseProject.GetTextPointAt(TextBox.MainControl.Document.ContentStart, ResultSearch[activeFindIndex].Position);//.ElementAt(activeFindIndex).Position);
            }
        }

        private void GoToNextFind_MouseUp(object sender, MouseButtonEventArgs e)
        {
            activeFindIndex = activeFindIndex < ResultSearch.Count - 1 ? activeFindIndex + 1 : 0;
            TextBox.MainControl.CaretPosition = BrowseProject.GetTextPointAt(TextBox.MainControl.Document.ContentStart, ResultSearch[activeFindIndex].Position);
        }

        private void AboutInformationOnClick(object sender, RoutedEventArgs e)
        {
            var form = new InformationForm();
            form.Top = Parent.Top + 105;
            form.Left = Parent.Left + Parent.Width / 2 - form.Width / 2;
            form.ShowDialog();
        }
        PropertiesForm propertForm;
        private void MenuOptions_Click(object sender, RoutedEventArgs e)
        {
            propertForm = new PropertiesForm(null);
            //    propertForm.Closing += new System.ComponentModel.CancelEventHandler((s, r) => MainProjectList.Items.Refresh());
            propertForm.CalledControl = BrowseProject;
            propertForm.Init();
            ((Options)propertForm.Element.Child).DirectoryPath.Text = BrowseProject.ProjectsPath;
            propertForm.ShowDialog();
        }
    }
    public class SearchResult
    {
        public string Path { get; set; }
        public string Text { get; set; }
        public int Position { get; set; }
    }
}
