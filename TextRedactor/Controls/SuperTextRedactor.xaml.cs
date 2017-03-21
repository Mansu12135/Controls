using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
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
        internal Window Parent;
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
            Format.ButListNum.MouseUp += ButListNum_MouseUp;
            Format.ButListBubl.MouseUp += ButListBubl_MouseUp;
            BrowseProject.HidenProject.MouseUp += HidenProject_MouseUp;
            NotesBrowser.HidenNotes.MouseUp += HidenNotes_MouseUp;
            TextBox.MainControl.Parent = this;
            Format.ColorPicker1.SelectedColorChanged += ColorPicker1_SelectedColorChanged;
            Format.comboWigth.SelectionChanged += ComboWigth_SelectionChanged;
            Format.comboWigth.LostFocus += ComboWigth_LostFocus;
            Format.comboWigth.KeyDown += ComboWigth_KeyDown;

        }

      

        private void ButListBubl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            List list = new List();
            list.MarkerStyle = TextMarkerStyle.Disc;
            var listItem = new ListItem();
            list.ListItems.Add(listItem);
            TextBox.MainControl.Document.Blocks.Add(list);
        }


        private void ButListNum_MouseUp(object sender, MouseButtonEventArgs e)
        {
            List list = new List();
            list.MarkerStyle = TextMarkerStyle.Decimal;
            var listItem = new ListItem();
            list.ListItems.Add(listItem);
            TextBox.MainControl.Document.Blocks.Add(list);
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
            TextBox.MainControl.OnCommandExecuted(TextBox.MainControl,null);
            TextBox.MainControl.Focus();
        }

        private void ComboWigth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Format.comboWigth.SelectedValue != null)
            {
                TextBox.MainControl.Document.PageWidth = Convert.ToDouble(Format.comboWigth.SelectedValue);
            }
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
            TextBox.MainControl.Dispose();
        }

        private void ExportButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (exportPanel == null)
            {
                exportPanel = new ExportPanel();
                exportPanel.HidenExport.MouseUp += new MouseButtonEventHandler((s, r) => { Container.Child = null; });
                exportPanel.Name = "export";
                exportPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                exportPanel.VerticalAlignment = VerticalAlignment.Stretch;
                exportPanel.ButExport.Click += ButExport_Click; ;
                exportPanel.project = BrowseProject.CurentProject;
                exportPanel.Init();
               // Grid.SetColumn(exportPanel, 2);
                //Grid.SetRowSpan(exportPanel, 2);
                //System.Windows.Controls.Panel.SetZIndex(exportPanel, 2);
                //MainContainer.Children.Add(exportPanel);
                Container.Child = exportPanel;
            }
        }

        private void ButExport_Click(object sender, RoutedEventArgs e)
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
                    SavePath = folderPath,
                    ImagePath = exportPanel.ImageName.Tag?.ToString()
                };
                TextBox.MainControl.SaveAsEpub(exportInfo, BrowseProject.CurentProject.Files);
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
                panel.HidenDictionary.MouseUp += new MouseButtonEventHandler((s, r) => { MainContainer.Children.Remove(panel); NotesBrowser.Visibility = Visibility.Visible; panel = null; });
                panel.Name = "dictionary";
                if (ShowNotes.Visibility == Visibility.Visible)
                {
                    ShowNotes_MouseUp(null, null);
                }
                panel.TextWord.KeyUp += TextWord_KeyUp;
                panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                panel.VerticalAlignment = VerticalAlignment.Stretch;
                //  panel.zIndex = 2;
                NotesBrowser.Visibility = Visibility.Hidden;
                Grid.SetColumn(panel, 2);
                Grid.SetRowSpan(panel, 2);
                System.Windows.Controls.Panel.SetZIndex(panel, 2);
                MainContainer.Children.Add(panel);
             //   Container.Child = panel;
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
            if (TextBox.MainControl.IsReadOnly == true) return "";
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
                    var image = new System.Windows.Controls.Image();
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
            var range = TextBox.MainControl.Selection;
            string name = NotesBrowser.GenerateName("Note");
         //   range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.PaleGreen);

              AddFlag(range,name);
            AddNote(range,name);
            NotesBrowser.MainControl.Items.Refresh();
        }
        public void AddNote(TextSelection range,string name)
        {
            int startOffset = TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(range.Start);// new TextRange(TextBox.MainControl.Document.ContentStart, TextBox.MainControl.Selection.Start).Text.Length;
            int endOffset = TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(range.End); //new TextRange(TextBox.MainControl.Document.ContentStart, TextBox.MainControl.Selection.End).Text.Length;
            string text = range.Text;
            NotesBrowser.AddItem(new Note(name, text, new TextRange(range.Start,range.End), startOffset, endOffset));
        }

        private void AddFlag(TextSelection range,string name)
        {
            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.PaleGreen);
            //  new TextRange(TextBox.MainControl.Selection.Start, TextBox.MainControl.Selection.End).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.PaleGreen);
            var tempImage = Properties.Resources.noteFlag;
            var ScreenCapture = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                 tempImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(20, 20));
            var image = new Image();
            image.Source = ScreenCapture;
            image.Stretch = Stretch.Fill;
            image.Cursor = Cursors.Hand;
            image.Height = 14;
            image.Width = 14;
            image.Tag = name;
            image.MouseUp += Image_MouseUp;
         //   TextPointer p = TextBox.MainControl.Selection.Start;
            TextBox.MainControl.BeginChange();
            InlineUIContainer imageContainer = new InlineUIContainer(image, range.Start);
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
               // var document = LoadFile(found[0].Path);
                foreach (var item in found)
                {
                    var range = item.Range;
                       // new TextRange(
                         //   TextBox.MainControl.Document.ContentStart.GetPositionAtOffset(item.Start),
                           // TextBox.MainControl.Document.ContentStart.GetPositionAtOffset(item.End));
                    if (range.Text.Trim() != item.Text.Trim())
                    {
                        FlowPosition = 0;
                        return 0;
                    }
                    //FlowPosition += to.Length - range.Text.Length;
                    range.Text = to;
                   // count++;
                }
            //    SaveChanges(found[0], document);
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
                        list.Add(SearchSelector.rezults.ElementAt(activeFindIndex));
                        count = Replace(list, to);
                        if (count > 0)
                        {
                            SearchSelector.rezults.Remove(SearchSelector.rezults.ElementAt(activeFindIndex));
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
                        
                        // SearchSelector.RestoreOriginalState(this);
                        break;
                    }
            }
            //SearchSelector.rezults.ForEach(item =>
            //    item.Position += FlowPosition);
            BrowseProject.OpenFile(BrowseProject.CurentFile, Path.GetFileNameWithoutExtension(BrowseProject.CurentFile));
            NotesBrowser.MainControl.Items.Refresh();
            return count;
        }

        //private SearchResult Find(string therm, string file)
        //{
        //    var document = LoadFile(file);
        //    TextPointer current = document.ContentStart;
        //    int index;
        //    int count = 0;
        //    foreach (var block in document.Blocks)
        //    {
        //        string textInRun = new TextRange(block.ContentStart, block.ContentEnd).Text;
        //        if (!string.IsNullOrWhiteSpace(textInRun))
        //        {
        //            index = textInRun.IndexOf(therm);
        //            if (index != -1)
        //            {
        //                return new SearchResult { Position = index + count, Path = file, Text = therm };// new TextRange(document.ContentStart.GetPositionAtOffset(index), document.ContentStart.GetPositionAtOffset(index + therm.Length)).Text });
        //            }
        //            count += textInRun.Length;
        //        }
        //    }
        //    //    while (current != null)
        //    //{
        //    //    int index;
        //    //    string textInRun = current.GetTextInRun(LogicalDirection.Forward);
        //    //    if (!string.IsNullOrWhiteSpace(textInRun))
        //    //    {
        //    //        index = textInRun.IndexOf(therm);
        //    //        if (index != -1)
        //    //        {
        //    //            return new KeyValuePair<int, SearchResult>(index, new SearchResult { Path = file, Text = new TextRange(document.ContentStart.GetPositionAtOffset(index), document.ContentStart.GetPositionAtOffset(index + therm.Length)).Text });
        //    //        }
        //    //    }
        //    //    current = current.GetNextContextPosition(LogicalDirection.Forward);
        //    //}
        //    return null;
        //}

        //private void SelectSearchWord(string file, List<SearchResult> items)
        //{
        //    foreach (var item in items)
        //    {
        //        if (item.Path == file)
        //        {
        //            new TextRange
        //                (TextBox.MainControl.GetTextPointAt(TextBox.MainControl.Document.ContentStart, item.Position),
        //                TextBox.MainControl.GetTextPointAt(TextBox.MainControl.Document.ContentStart, item.Position + item.Text.Length))
        //                .ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
        //        }
        //    }
        //}

       
        //private void Founds(string textInRun, string threm, string file, int paragraphPosition, ref List<SearchResult> items)
        //{
        //    if (!string.IsNullOrWhiteSpace(textInRun))
        //    {
        //        int count = 0;
        //        while (count != -1)
        //        {
        //            count = textInRun.IndexOf(threm, count);
        //            if (count > -1)
        //            {
        //                items.Add(new SearchResult { Position = paragraphPosition+count, Path = file, Text = threm});
        //                count += threm.Length;
        //            }
        //        }
        //    }
        //}

        private List<SearchResult> FindAll(string therm, string file)
        {

            var list = new List<SearchResult>();
            var document = TextBox.MainControl;//LoadFile(file);
            document.SelectAll();
            document.Selection
         //   new TextRange
          //             (document.ContentStart, document.ContentEnd)
                       .ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
            Regex reg = new Regex(therm, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            TextPointer position = document.Document.ContentStart;
            List<TextRange> ranges = new List<TextRange>();
            int count = 0;
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = position.GetTextInRun(LogicalDirection.Forward);
                    var matchs = reg.Matches(text);

                    foreach (Match match in matchs)
                    {

                        TextPointer start = position.GetPositionAtOffset(match.Index);
                        TextPointer end = start.GetPositionAtOffset(therm.Trim().Length);
                        TextRange textrange = new TextRange(start, end);
                      //  ranges.Add(textrange);
                 //       var range = new TextRange(start, end);// document.ContentStart.(position)+document.ContentStart.GetPositionAtOffset(match.Index), document.ContentStart.GetPositionAtOffset(therm.Trim().Length));
                       list.Add(new SearchResult { Path = file, Text = textrange.Text, Range= textrange /*Start = document.ContentStart.GetOffsetToPosition(start), End = document.ContentStart.GetOffsetToPosition(end)*/});
                 //range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));

                    }
                }
                else
                {
                    count++;
                }
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
            foreach (var range in list)
            {
                range.Range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Red));
             //   range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }

            //foreach (var item in list)
            //{
            //    new TextRange(document.ContentStart.GetPositionAtOffset(list.LastOrDefault().Start), document.ContentStart.GetPositionAtOffset(list.LastOrDefault().End)).
            //     ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
            //}
            //TextPointer p = TextBox.MainControl.Document.ContentStart;
            //while (true)
            //{
            //    var range = FindWordFromPosition(p, therm);
            //    if(range == null) { break; }
            //    list.Add(new SearchResult { Path = file, Position = TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(range.Start.DocumentStart), Text = range.Text });
            //    p = range.End;
            //}


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
            if(!(w is ISettings)) { throw new Exception();}
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
            ShowNotes.Visibility = Visibility.Hidden;
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
                searchPanel.HidenSearch.MouseUp += new MouseButtonEventHandler((s, r) => { MainContainer.Children.Remove(searchPanel); NotesBrowser.Visibility = Visibility.Visible; searchPanel = null;
                    SearchSelector.RestoreOriginalState(this);
                });
                searchPanel.GoToNextFind.MouseUp += GoToNextFind_MouseUp;
                searchPanel.ButReplace.MouseUp += ButReplace_MouseUp;
                searchPanel.TextWord.KeyUp += TextWord_KeyUp1;
                searchPanel.Name = "search";
                if (ShowNotes.Visibility == Visibility.Visible)
                {
                    ShowNotes_MouseUp(null, null);
                }
                searchPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                searchPanel.VerticalAlignment = VerticalAlignment.Stretch;
                NotesBrowser.Visibility = Visibility.Hidden;
                Grid.SetColumn(searchPanel, 2);
                Grid.SetRowSpan(searchPanel, 2);
                System.Windows.Controls.Panel.SetZIndex(searchPanel, 2);
                MainContainer.Children.Add(searchPanel);
              //  Container.Child = searchPanel;
            }
            else
            {
                //Grid.SetColumn(searchPanel, 2);
                //Grid.SetRowSpan(searchPanel, 2);
                //System.Windows.Controls.Panel.SetZIndex(searchPanel, 2);
                //MainContainer.Children.Add(searchPanel);
               // Container.Child = searchPanel;
            }
            if (!TextBox.MainControl.IsReadOnly && !string.IsNullOrWhiteSpace(TextBox.MainControl.Selection.Text))
            GetSearchResalt(TextBox.MainControl.Selection.Text);
            //   TextBox.MainControl.CaretPosition = BrowseProject.GetTextPointAt(TextBox.MainControl.Document.ContentStart, ResultSearch.ElementAt(activeFindIndex).Key);
        }
        private void GetSearchResalt(string value)
        {
            //TextBox.MainControl.Test(value);
            SearchSelector.rezults = Search(value, FindReplaceExpression.InThisPage);
          //  SearchSelector.SelectAll(BrowseProject.CurentFile, TextBox.MainControl);
            //SelectSearchWord(BrowseProject.CurentFile, ResultSearch);
            searchPanel.SearchResult.ItemsSource = SearchSelector.rezults;
            searchPanel.SearchResult.SelectionChanged += SearchResult_SelectionChanged;
            searchPanel.TextWord.Text = value;
            activeFindIndex = 0;
            if (SearchSelector.rezults.Any())
            {
                searchPanel.SearchResult.SelectedIndex = 0;
            }
        }
        private void TextWord_KeyUp1(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(searchPanel.TextWord.Text) &&!string.IsNullOrEmpty(BrowseProject.CurentFile))
            {
                GetSearchResalt(searchPanel.TextWord.Text);
            }
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
                TextBox.MainControl.CaretPosition = (SearchSelector.rezults[activeFindIndex].Range.Start);
                //TextBox.MainControl.CaretPosition = SearchSelector.rezults[activeFindIndex].Text.End;
            }
        }

        private void GoToNextFind_MouseUp(object sender, MouseButtonEventArgs e)
        {
            activeFindIndex = activeFindIndex < SearchSelector.rezults.Count - 1 ? activeFindIndex + 1 : 0;
            TextBox.MainControl.CaretPosition = (SearchSelector.rezults[activeFindIndex].Range.Start);
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
    
}
