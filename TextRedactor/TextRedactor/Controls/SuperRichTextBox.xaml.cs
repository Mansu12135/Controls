using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Office.Interop.Word;
using Point = System.Windows.Point;
using Word=Microsoft.Office.Interop.Word;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для SuperRichTextBox.xaml
    /// </summary>
    public partial class SuperRichTextBox : UserControl
    {
        private Word.Application appWord;
        private object objLanguage = WdLanguageID.wdEnglishUS;

        public SuperRichTextBox()
        {
            InitializeComponent();
            appWord = new Word.Application();
        }

        public List<string> GetSynonyms(string word)
        {
            List<string> synonymsList = new List<string>();
            object objFalse = false;      // false entries and language
            SynonymInfo si =
            appWord.get_SynonymInfo(word, ref objLanguage);

            var MeaningList = si.MeaningList as Array;
            if (MeaningList == null) { return null; }
            foreach (string strMeaning in MeaningList)
            {
                object objMeaning = strMeaning;
                List<string> SynonymList = (si.get_SynonymList(ref objMeaning) as Array).Cast<string>().ToList();
                if (SynonymList == null) { continue; }
                synonymsList.AddRange(SynonymList);
            }
            return synonymsList;
        }   

        public BitmapSource Draw()
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(this);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(this);
                ctx.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);
            return rtb;
        }
    }
}
