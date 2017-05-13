using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static UILayer.SuperTextRedactor;

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

        public void Hide()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = ParentControl.RedactorConteiner.Margin;
            myThicknessAnimation.To = new Thickness(ParentControl.RedactorConteiner.Margin.Left, ParentControl.RedactorConteiner.Margin.Top, -ActualWidth, ParentControl.RedactorConteiner.Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            myThicknessAnimation.Completed += MyThicknessAnimation_Completed;
            ((Border)ParentControl.RedactorConteiner).BeginAnimation(Border.MarginProperty, myThicknessAnimation);
        }

        private void MyThicknessAnimation_Completed(object sender, EventArgs e)
        {
            ParentControl.ShowNotes.Visibility = Visibility.Visible;
            ParentControl.MainContainer.Children.Remove(this);
        }

        public void GetSearchResalt(string value)
        {
            //TextBox.MainControl.Test(value);
            SearchSelector.rezults = Search(value, FindReplaceExpression.InThisPage);
            //  SearchSelector.SelectAll(BrowseProject.CurentFile, TextBox.MainControl);
            //SelectSearchWord(BrowseProject.CurentFile, ResultSearch);
            SearchResult.ItemsSource = SearchSelector.rezults;
            SearchResult.SelectionChanged += SearchResult_SelectionChanged;
            TextWord.Text = value;
           
            if (SearchSelector.rezults.Any())
            {
                SearchResult.SelectedIndex = 0;
            }
            activeFindIndex = 0;
        }

        private void TextWord_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(TextWord.Text) && !string.IsNullOrEmpty(ParentControl.BrowseProject.CurentFile))
            {
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

            var list = new List<SearchResult>();
            var document = ParentControl.TextBox.MainControl;//LoadFile(file);
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
                        list.Add(new SearchResult { Path = file, Text = textrange.Text, Range = textrange /*Start = document.ContentStart.GetOffsetToPosition(start), End = document.ContentStart.GetOffsetToPosition(end)*/});
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
                range.Range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
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
                        count = ReplaceAllWords(Search(from, expression), to);

                        // SearchSelector.RestoreOriginalState(this);
                        break;
                    }
            }
            //SearchSelector.rezults.ForEach(item =>
            //    item.Position += FlowPosition);
            ParentControl.BrowseProject.OpenFile(ParentControl.BrowseProject.CurentFile, System.IO.Path.GetFileNameWithoutExtension(ParentControl.BrowseProject.CurentFile));
            ParentControl.NotesBrowser.MainControl.Items.Refresh();
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
            if ((bool)ReplaceOnce.IsChecked)
            {
                Replace(TextWord.Text, TextWordReplace.Text, FindReplaceExpression.FirstFound);
            }
            else if ((bool)ReplaceAll.IsChecked)
            {
                Replace(TextWord.Text, TextWordReplace.Text, FindReplaceExpression.InThisBook);
            }
            SearchResult.Items.Refresh();
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
            activeFindIndex = activeFindIndex < SearchSelector.rezults.Count - 1 ? activeFindIndex + 1 : 0;
            ParentControl.TextBox.MainControl.Focus();
            ParentControl.TextBox.MainControl.CaretPosition = (SearchSelector.rezults[activeFindIndex].Range.Start);
        }

        private void ButReplace_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DoReplace();
        }
    }
}
