using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ApplicationLayer;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для SearchPanel.xaml
    /// </summary>
    public partial class SearchPanel : UserControl
    {
        public SearchPanel(SuperTextRedactor Parent)
        {
            InitializeComponent();
            ParentControl = Parent;
        }
        private SuperTextRedactor ParentControl;
        public int activeFindIndex;
        private int FlowPosition;
        public void Show()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = ParentControl.RedactorConteiner.Margin;
            myThicknessAnimation.To = new Thickness(ParentControl.RedactorConteiner.Margin.Left, ParentControl.RedactorConteiner.Margin.Top, 0, ParentControl.RedactorConteiner.Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            ParentControl.RedactorConteiner.BeginAnimation(Border.MarginProperty, myThicknessAnimation);
            ParentControl.ShowNotes.Visibility = Visibility.Hidden;
        }
        public void Disposing()
        {
            SearchSelector.RestoreOriginalState(ParentControl);
        }
        public void Hide()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = ParentControl.RedactorConteiner.Margin;
            myThicknessAnimation.To = new Thickness(ParentControl.RedactorConteiner.Margin.Left, ParentControl.RedactorConteiner.Margin.Top, -ActualWidth, ParentControl.RedactorConteiner.Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            myThicknessAnimation.Completed += MyThicknessAnimation_Completed;
            ((Border)ParentControl.RedactorConteiner).BeginAnimation(Border.MarginProperty, myThicknessAnimation);
            ParentControl.TextBox.MainControl.TextChanged -= MainControl_TextChanged;
        }

        private void MyThicknessAnimation_Completed(object sender, EventArgs e)
        {
            ParentControl.ShowNotes.Visibility = Visibility.Visible;
            ParentControl.MainContainer.Children.Remove(this);
        }

        public void GetSearchResalt(string value)
        {
            if (SearchSelector.rezults.Count != 0)
            {
                ClearSearch();
            }
            SearchSelector.rezults = Search(value, FindReplaceExpression.InThisPage);
            SearchResult.ItemsSource = SearchSelector.rezults;
            SearchResult.SelectionChanged -= SearchResult_SelectionChanged;
            SearchResult.SelectionChanged += SearchResult_SelectionChanged;
            TextWord.Text = value;

            if (SearchSelector.rezults.Any())
            {
                SearchResult.SelectedIndex = 0;
            }
            activeFindIndex = 0;
            ParentControl.TextBox.MainControl.TextChanged -= MainControl_TextChanged;
            ParentControl.TextBox.MainControl.TextChanged += MainControl_TextChanged;
        }
        private void MainControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            ParentControl.TextBox.MainControl.TextChanged -= MainControl_TextChanged;
            ClearSearch();
        }

        public void ClearSearch()
        {
            if (this == null) return;
            SearchSelector.RestoreOriginalState(ParentControl);
            SearchSelector.ClearAll(ParentControl.TextBox.MainControl);
            SearchResult.Items.Refresh();
        }
        private void TextWord_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(TextWord.Text) && !string.IsNullOrEmpty(ParentControl.BrowseProject.CurentFile))
            {
                ClearSearch();
                GetSearchResalt(TextWord.Text);
            }
        }
        public List<SearchResult> Search(string therm, FindReplaceExpression expression)
        {
            var list = new List<SearchResult>();
            switch (expression)
            {
                case FindReplaceExpression.InThisPage:
                    {
                        var temp = FindAll(therm, ParentControl.BrowseProject.CurentFile);
                        foreach (var rezult in temp)
                        {
                            list.Add(rezult);
                        }
                        break;
                    }
                case FindReplaceExpression.InThisBook:
                    {
                        string project = new FileInfo(ParentControl.BrowseProject.CurentFile).Directory.Parent.Name;
                        if (!ParentControl.BrowseProject.Notes.ContainsKey(project)) { break; }
                        foreach (var file in ParentControl.BrowseProject.Notes[project].Files)
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

        private List<SearchResult> FindAll(string therm, string file)
        {
            int thermLength = therm.Length;
            var list = new List<SearchResult>();
            var document = ParentControl.TextBox.MainControl;
            Regex reg = new Regex(therm, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matchs = reg.Matches(new TextRange(document.Document.ContentStart, document.Document.ContentEnd).Text);
            var a = new FlowDocumentWorker(document.Document);
            TextPointer pointer = null;
            TextRange resultRange;
            foreach (Match match in matchs)
            {
                pointer = a.GetTextPointer(match.Index, pointer == null);
                resultRange = new TextRange(pointer, a.GetTextPointer(match.Index + thermLength));
                list.Add(new SearchResult
                {
                    Path = file,
                    Text = GetTextAround(resultRange),
                    Range = resultRange
                });
                resultRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
            }

            return list;
        }

        private int Replace(string from, string to, FindReplaceExpression expression)
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
                            GetSearchResalt(from);
                            //   SearchSelector.rezults.Remove(SearchSelector.rezults.ElementAt(activeFindIndex));
                        }
                        break;
                    }
                case FindReplaceExpression.InThisPage:
                    {
                        count = ReplaceAllWords(Search(from, expression), to);
                        break;
                    }
                case FindReplaceExpression.InThisParagraph:
                    {
                        count = 0;
                        break;
                    }
                case FindReplaceExpression.InThisBook:
                    {
                        count = ReplaceAllWords(Search(from, expression), to);

                        // SearchSelector.RestoreOriginalState(this);
                        break;
                    }
            }
            //SearchSelector.rezults.ForEach(item =>
            //    item.Position += FlowPosition);
            // ParentControl.BrowseProject.OpenFile(ParentControl.BrowseProject.CurentFile, System.IO.Path.GetFileNameWithoutExtension(ParentControl.BrowseProject.CurentFile));
            //  ParentControl.NotesBrowser.MainControl.Items.Refresh();
            return count;
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
                        //?????????????????????????????
                        //FlowPosition = 0;
                        // return 0;
                    }
                    //FlowPosition += to.Length - range.Text.Length;
                    range.Text = to;
                    count++;
                }
                //    SaveChanges(found[0], document);
            }
            // TextBox.MainControl.Document.Focus();
            return count;
        }

        private int ReplaceAllWords(List<SearchResult> founds, string to)
        {
            int count = 0;
            var list = founds.GroupBy(r => r.Path).ToList();
            foreach (var item in list)
            {
                count += Replace(item.OfType<SearchResult>().ToList(), to);
            }
            return count;
        }

        public void DoReplace()
        {
            var index = activeFindIndex;
            if ((bool)ReplaceOnce.IsChecked)
            {
                Replace(TextWord.Text, TextWordReplace.Text, FindReplaceExpression.FirstFound);
            }
            else if ((bool)ReplaceAll.IsChecked)
            {
                Replace(TextWord.Text, TextWordReplace.Text, FindReplaceExpression.InThisPage);
                ClearSearch();
            }
            SearchResult.Items.Refresh();
            if (SearchSelector.rezults.Any())
            {
                SearchResult.SelectedIndex = index;
            }
            activeFindIndex = index;
        }

        private void SearchResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as System.Windows.Controls.ListBox;
            if (list == null) { return; }
            activeFindIndex = list.SelectedIndex;
            if (activeFindIndex != -1)
            {
                ParentControl.TextBox.MainControl.Focus();
                ParentControl.TextBox.MainControl.CaretPosition = (SearchSelector.rezults[activeFindIndex].Range.Start);
                //TextBox.MainControl.CaretPosition = SearchSelector.rezults[activeFindIndex].Text.End;
            }
        }
        public enum FindReplaceExpression
        {
            FirstFound = 0,
            InThisPage = 1,
            InThisParagraph = 2,
            InThisBook = 3
        }

        private void GoToNextFind_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (SearchSelector.rezults.Count <= 0) return;
            activeFindIndex = activeFindIndex < SearchSelector.rezults.Count - 1 ? activeFindIndex + 1 : 0;
            SearchResult.SelectedIndex = activeFindIndex;
            ParentControl.TextBox.MainControl.Focus();
            ParentControl.TextBox.MainControl.CaretPosition = (SearchSelector.rezults[activeFindIndex].Range.Start);
        }

        private void ButReplace_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!SearchSelector.rezults.Any()) return;
            ParentControl.TextBox.MainControl.TextChanged -= MainControl_TextChanged;
            SearchSelector.RestoreOriginalState(ParentControl);
            DoReplace();
            activeFindIndex = SearchResult.SelectedIndex;
            // ParentControl.TextBox.MainControl.TextChanged += MainControl_TextChanged;
        }
        private string GetTextAround(TextRange range)
        {
            string rezult = "";
            TextPointer start, end;
            var rangeStartOffset = ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(range.Start);
            var rangeEndOffset = ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(range.End);
            var docEnd = ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(ParentControl.TextBox.MainControl.Document.ContentEnd);
            if (rangeStartOffset < 10)
            {
                start = ParentControl.TextBox.MainControl.Document.ContentStart;
            }
            else
            {
                start = ParentControl.TextBox.MainControl.Document.ContentStart.GetPositionAtOffset(rangeStartOffset - 10);
            }
            if (rangeEndOffset > docEnd - 10)
            {
                end = ParentControl.TextBox.MainControl.Document.ContentEnd;
            }
            else
            {
                end = ParentControl.TextBox.MainControl.Document.ContentStart.GetPositionAtOffset(rangeEndOffset + 10);
            }
            rezult = new TextRange(start, end).Text;
            return rezult;
        }

        private void ReplaceOnce_Checked(object sender, RoutedEventArgs e)
        {
            if (ReplaceAll == null) return;
            ReplaceAll.IsChecked = !ReplaceOnce.IsChecked;
        }

        private void ReplaceAll_Checked(object sender, RoutedEventArgs e)
        {
            if (ReplaceOnce == null) return;
            ReplaceOnce.IsChecked = !ReplaceAll.IsChecked;
        }

        private void TextWord_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;
            if (String.IsNullOrWhiteSpace(textBox.Text))
                textBox.Text = "Enter word...";
        }

        private void TextWord_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;
            textBox.Text = "";
        }
    }
}
