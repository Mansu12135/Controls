using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для FormatPanel.xaml
    /// </summary>
    public partial class FormatPanel : UserControl
    {
        public FormatPanel()
        {
            InitializeComponent();
        }
        static FormatPanel()
        {
            FontSize = new Dictionary<int, double>();
            FontSize.Add(1, 8);
            FontSize.Add(2, 9);
            FontSize.Add(3, 11);
            FontSize.Add(4, 12);
            FontSize.Add(5, 14);
            FontSize.Add(6, 16);
            FontSize.Add(7, 18);
            FontSize.Add(8, 20);
            FontSize.Add(9, 22);
            FontSize.Add(10, 24);
            FontSize.Add(11, 26);
            FontSize.Add(12, 28);
            FontSize.Add(13, 36);
            FontSize.Add(14, 48);
            FontSize.Add(15, 72);
            MarginWigth = new Dictionary<int, double>();
            MarginWigth.Add(1, 100);
            MarginWigth.Add(2, 200);
            MarginWigth.Add(3, 300);
            MarginWigth.Add(4, 400);
            MarginWigth.Add(5, 500);
            MarginWigth.Add(6, 600);
            MarginWigth.Add(7, 700);
            MarginWigth.Add(8, 800);
            MarginWigth.Add(9, 900);

        }
        public static readonly Dictionary<int, double> FontSize;

        public static readonly Dictionary<int, double> MarginWigth;

     
    }
}
