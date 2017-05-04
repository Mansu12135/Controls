using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;


namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для SuperTextRedactor.xaml
    /// </summary>
    public partial class SuperTextRedactor : UserControl
    {
        public SuperTextRedactor()
        {
            InitializeComponent();
        }

        public void HideNoteBrowser()
        {
            Notes.HidePanel();
            Width += Notes.PreviousWidth;
        }

        public void UnHideNoteBrowser()
        {
            Width -= Notes.PreviousWidth;
            Notes.ShowPanel();
        }

        public void Export()
        {
            
        }

        private int Replace(List<Point> founds)
        {
            return 0;
        }

        public int Replace(string from, string to, FindReplaceExpression expression)
        {
            int count=0;
            switch (expression)
            {
                case FindReplaceExpression.FirstFound:
                {
                    count=Replace(Search(from, expression));
                    break;
                }
                case FindReplaceExpression.InThisPage:
                {
                    count=Replace(Search(from, expression));
                    break;
                }
                case FindReplaceExpression.InThisParagraph:
                {
                    count=Replace(Search(from, expression));
                    break;
                }
                case FindReplaceExpression.InThisBook:
                {
                    count=Replace(Search(from, expression));
                    break;
                }
            }
            return count;
        }

        public List<Point> Search(string therm, FindReplaceExpression expression)
        {
            return null;
        }

        public enum FindReplaceExpression
        {
            FirstFound = 0,            
            InThisPage = 1,
            InThisParagraph = 2,
            InThisBook = 3
        }
    }
}
