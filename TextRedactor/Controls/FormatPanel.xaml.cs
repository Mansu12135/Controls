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

        public static Dictionary<int, double> FontSize = new Dictionary<int, double>();

        public static Dictionary<int, double> MarginWigth = new Dictionary<int, double>();
    }
}
