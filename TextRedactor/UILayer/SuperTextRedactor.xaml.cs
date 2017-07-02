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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ApplicationLayer;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using UserControl = System.Windows.Controls.UserControl;


namespace UILayer
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
       // private int FlowPosition;
       // private int activeFindIndex;

        public double defaultFont;
        public double defaultSpacing;
        public double DefaultMarginWight;

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
            //Format.ButListNum.MouseUp += ButListNum_MouseUp;
          //  Format.ButListBubl.MouseUp += ButListBubl_MouseUp;
            BrowseProject.HidenProject.MouseUp += HidenProject_MouseUp;
            NotesBrowser.HidenNotes.MouseUp += HidenNotes_MouseUp;
            TextBox.MainControl.Parent = this;
            Format.ColorPicker1.SelectedColorChanged += ColorPicker1_SelectedColorChanged;
            Format.comboWigth.SelectionChanged += ComboWigth_SelectionChanged;
            Format.comboWigth.LostFocus += ComboWigth_LostFocus;
            Format.comboWigth.KeyDown += ComboWigth_KeyDown;
            Format.NumerCombo.SelectionChanged += NumerCombo_SelectionChanged;
            Format.bublCombo.SelectionChanged += BublCombo_SelectionChanged;
        }

        private void BublCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var textBox = Format.bublCombo.SelectedValue as TextBlock;
            if (textBox == null || textBox.Tag == null) { return; }
            List list = new List();
            list.MarkerStyle = TextMarkerStyle.Disc;
            switch (textBox.Tag.ToString())
            {
                case "Circle":
                    list.MarkerStyle = TextMarkerStyle.Circle;
                    break;
                case "Disc":
                    list.MarkerStyle = TextMarkerStyle.Disc;
                    break;
                case "Box":
                    list.MarkerStyle = TextMarkerStyle.Box;
                    break;
                case "Square":
                    list.MarkerStyle = TextMarkerStyle.Square;
                    break;

                default:
                    list.MarkerStyle = TextMarkerStyle.Decimal;
                    break;
            }
            var listItem = new ListItem();
            list.ListItems.Add(listItem);
            TextBox.MainControl.Document.Blocks.InsertBefore(TextBox.MainControl.CaretPosition.Paragraph, list);
            Format.bublCombo.SelectedIndex = -1;
        }

        private void NumerCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var textBox = Format.NumerCombo.SelectedValue as TextBlock;
            if (textBox == null || textBox.Tag == null) { return; }
            List list = new List();
            list.MarkerStyle = TextMarkerStyle.Disc;
            switch (textBox.Tag.ToString())
            {
                case "Decimal":
                    list.MarkerStyle = TextMarkerStyle.Decimal;
                    break;
                case "LowerLatin":
                    list.MarkerStyle = TextMarkerStyle.LowerLatin;
                    break;
                case "LowerRoman":
                    list.MarkerStyle = TextMarkerStyle.LowerRoman;
                    break;
                case "UpperLatin":
                    list.MarkerStyle = TextMarkerStyle.UpperLatin;
                    break;
                case "UpperRoman":
                    list.MarkerStyle = TextMarkerStyle.UpperRoman;
                    break;
                default:
                    list.MarkerStyle = TextMarkerStyle.Decimal;
                    break;
            }
            var listItem = new ListItem();
            list.ListItems.Add(listItem);
            TextBox.MainControl.Document.Blocks.InsertBefore(TextBox.MainControl.CaretPosition.Paragraph,list);
            Format.NumerCombo.SelectedIndex = -1;
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
            TextBox.MainControl.OnCommandExecuted(TextBox.MainControl, null);
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
            DefaultMarginWight = (Convert.ToDouble(value));
            Format.comboWigth.SelectedIndex = Format.comboWigth.Items.IndexOf(DefaultMarginWight);

            //  Format.comboWigth.Text = Format.comboWigth.Items.IndexOf(DefaultMarginWight).ToString();
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
                NotesBrowser.Hide();
            }
            if (ShowProject.Visibility == Visibility.Hidden)
            {
                BrowseProject.Hide();
            }
        }

        private void OnlyProject()
        {
            if (ShowNotes.Visibility == Visibility.Hidden)
            {
                NotesBrowser.Hide();
            }
            if (ShowProject.Visibility == Visibility.Visible)
            {
                BrowseProject.Show();
            }
        }
        private void ProjectAndNotes()
        {
            if (ShowNotes.Visibility == Visibility.Visible)
            {
                NotesBrowser.Show();
            }
            if (ShowProject.Visibility == Visibility.Visible)
            {
                BrowseProject.Show();
            }
        }
       
        private void HidenNotes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NotesBrowser.Hide();
        }

        private void HidenProject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BrowseProject.Hide();
        }

        public void Dispose()
        {
            //if (searchPanel != null)
            //{
            //    searchPanel.Disposing();
            //}
            BrowseProject.Disposing();
            TextBox.MainControl.Dispose();
        }

        private void ExportButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (exportPanel == null)
            {
               // BrowseProject.UpdateOffsetOnNotes();
                exportPanel = new ExportPanel();
                exportPanel.HidenExport.MouseUp += HidenExport_MouseUp; 
                exportPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                exportPanel.VerticalAlignment = VerticalAlignment.Stretch;
                exportPanel.Margin = new Thickness(NotesBrowser.ActualWidth,0, -NotesBrowser.ActualWidth, 0);
                exportPanel.project = BrowseProject.CurentProject;
                exportPanel.Init(this);
                foreach(KeyValuePair<string,Project> item in BrowseProject.MainProjectList.Items)
                {
                    if (item.Value != BrowseProject.CurentProject)
                    {
                        ((ListBoxItem)BrowseProject.MainProjectList.ItemContainerGenerator.ContainerFromItem(item)).IsEnabled = false;
                    }
                }
                Format.IsEnabled = false;
                TextBox.IsEnabled = false;
                 Grid.SetColumn(exportPanel, 2);
                Grid.SetRowSpan(exportPanel, 2);
                System.Windows.Controls.Panel.SetZIndex(exportPanel, 2);
                MainContainer.Children.Add(exportPanel);
                exportPanel.Show();
                //Container.Child = exportPanel;
            }
        }

        private void HidenExport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            exportPanel.Hide();
            exportPanel = null;
           // BrowseProject.UpdateRangeOnNotes();
        }

       public ExportPanel exportPanel;
        DictionaryPanel panel;
        public SearchPanel searchPanel;
        bool inSearch = false;
        private void InitDictionary()
        {
            if (panel == null)
            {
                panel = new DictionaryPanel(this);
                panel.HidenDictionary.MouseUp += HidenDictionary_MouseUp; 
                panel.TextWord.KeyUp += TextWord_KeyUp;
                panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                panel.VerticalAlignment = VerticalAlignment.Stretch;
               Grid.SetColumn(panel, 2);
                Grid.SetRowSpan(panel, 2);
                System.Windows.Controls.Panel.SetZIndex(panel, 1);
               MainContainer.Children.Add(panel);
                panel.Show();
            }
            panel.TextWord.Text = GetCurrentWord();
            ShowDictionaryResult(panel.TextWord.Text);
            panel.ModeDictionary.IsChecked = !panel.ModeDictionary.IsChecked;
            panel.ModeDictionary.IsChecked = true;
        }

        private void HidenDictionary_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!NotesBrowser.IsVisible)
            {
                panel.Hide();
            }
            else
            {
                MainContainer.Children.Remove(panel);
            }
            panel = null;
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
            panel.defenition = string.Empty;
            panel.synonimus = string.Empty;
            List<Structure> result = TextBox.MainControl.GetInformation(panel.TextWord.Text.Trim());
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

            
            AddNote(range, name);
            NotesBrowser.MainControl.Items.Refresh();
        }
        public void AddNote(TextSelection range, string name)
        {
            int startOffset = TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(range.Start);// new TextRange(TextBox.MainControl.Document.ContentStart, TextBox.MainControl.Selection.Start).Text.Length;
            int endOffset = TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(range.End); //new TextRange(TextBox.MainControl.Document.ContentStart, TextBox.MainControl.Selection.End).Text.Length;
            string text = range.Text;
           var pointer = AddFlag(range, name);
            NotesBrowser.AddItem(new Note(name, text, new TextRange(pointer, range.End), startOffset, endOffset));
            NotesBrowser.CloseNotes(BrowseProject.LoadedFile);
        }

        internal static string TestRange(TextRange range)
        {
            var a = new FlowDocument();
            var r = new TextRange(a.ContentStart, a.ContentEnd);
            using (var stream = new MemoryStream())
            {
                range.Save(stream, DataFormats.XamlPackage);
                r.Load(stream, DataFormats.XamlPackage);
                // note.Value.Range.Save(File.Create("D:\\a.xaml"),DataFormats.XamlPackage );
                //  r.Load(File.OpenRead("D:\\a.xaml"), DataFormats.XamlPackage);
            }
            return XamlWriter.Save(a);
        }

        public TextPointer AddFlag(TextRange range, string name)
        {
            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.PaleGreen);
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
            imageContainer.Unloaded -= BrowseProject.Element_Unloaded;
            imageContainer.Unloaded += BrowseProject.Element_Unloaded;
            TextBox.MainControl.EndChange();
            TextBox.MainControl.Focus();
            return imageContainer.ContentStart;
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

      
        public void InitFullScr(Window w)
        {
            if (!(w is ISettings)) { throw new Exception(); }
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
            BrowseProject.Show();
        }
        private void ShowNotes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NotesBrowser.Show();
        }

        private void MenuAddPr_Click(object sender, RoutedEventArgs e)
        {
            BrowseProject.CreateProject();
        }

        private void MenuSearch_Click(object sender, RoutedEventArgs e)
        {
            if (searchPanel == null)
            {
                searchPanel = new SearchPanel(this);
                searchPanel.HidenSearch.MouseUp += HidenSearch_MouseUp;
                searchPanel.Show();
                searchPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                searchPanel.VerticalAlignment = VerticalAlignment.Stretch;
                Grid.SetColumn(searchPanel, 2);
                Grid.SetRowSpan(searchPanel, 2);
                System.Windows.Controls.Panel.SetZIndex(searchPanel, 1);
                MainContainer.Children.Add(searchPanel);
            }
            if (!TextBox.MainControl.IsReadOnly && !string.IsNullOrWhiteSpace(TextBox.MainControl.Selection.Text))
            {
                searchPanel.GetSearchResalt(TextBox.MainControl.Selection.Text);
            }
        }

        private void HidenSearch_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!NotesBrowser.IsVisible)
            {
                searchPanel.Hide();
            }
            else
            {
                MainContainer.Children.Remove(searchPanel);
            }
            SearchSelector.RestoreOriginalState(this);
            searchPanel = null;
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

        public void updateDefaultValue()
        {
            Format.comboWigth.SelectedIndex = Format.comboWigth.Items.IndexOf(DefaultMarginWight);
            Format.comboBoxFont.SelectedIndex = Format.comboBoxFont.Items.IndexOf(defaultFont);
        }
    }

}
