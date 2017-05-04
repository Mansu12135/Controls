using System.Collections.Generic;
using System.Windows.Controls;

namespace UILayer
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

        public static Dictionary<int, double> FontSize = new Dictionary<int, double>();

        public static Dictionary<int, double> MarginWigth = new Dictionary<int, double>();
    }
}
