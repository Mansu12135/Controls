using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Controls
{
    internal static class SearchSelector
    {
        internal static List<SearchResult> rezults = new List<SearchResult>();

        public static void SelectAll(string file, BaseRichTextBox control)
        {
            new TextRange
                       (control.Document.ContentStart, control.Document.ContentEnd)
                       .ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
            foreach (var item in rezults)
            {
                if (item.Path == file)
                {
                        new TextRange(control.Document.ContentStart.GetPositionAtOffset(item.Start), control.Document.ContentStart.GetPositionAtOffset(item.End)).      
                        ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
                }
            }
        }

        public static void ClearAll(BaseRichTextBox control)
        {
            var list = rezults.GroupBy(item => item.Path).ToList();
            foreach(var item in list)
            {
                var range = item.ToList();
                if (!range.Any()) { continue; }
                string path = range[0].Path;
                range.ForEach(x => rezults.Remove(x));
            }
        }

        public static void RestoreOriginalState(SuperTextRedactor control)
        {
            new TextRange
                        (control.TextBox.MainControl.Document.ContentStart, control.TextBox.MainControl.Document.ContentEnd)
                        .ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
            foreach(var item in control.NotesBrowser.Notes)
            {
                new TextRange
                       (control.TextBox.MainControl.GetTextPointAt(control.TextBox.MainControl.Document.ContentStart, item.Value.OffsetStart), (control.TextBox.MainControl.GetTextPointAt(control.TextBox.MainControl.Document.ContentStart, item.Value.OffsetEnd)))
                       .ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.PaleGreen);
            }
        }

    }
    public class SearchResult
    {
        public string Path { get; set; }
        public string Text { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public TextRange Range { get; set; }
    }
}
