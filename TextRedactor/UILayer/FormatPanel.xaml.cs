using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
            List list = new List();
            list.MarkerStyle = TextMarkerStyle.Square;
            var listItem = new ListItem();
            NumerCombo.Items.Add(new TextBlock() {Text = "1. 2. 3.", Tag = "Decimal" , Margin = new Thickness(3)});
            NumerCombo.Items.Add(new TextBlock() { Text = "a. b. c.", Tag = "LowerLatin", Margin = new Thickness(3) });
            NumerCombo.Items.Add(new TextBlock() { Text = "i. ii. iii.", Tag = "LowerRoman", Margin = new Thickness(3) });
            NumerCombo.Items.Add(new TextBlock() { Text = "A. B. C.", Tag = "UpperLatin", Margin = new Thickness(3) });
            NumerCombo.Items.Add(new TextBlock() { Text = "I. II. III.", Tag = "UpperRoman", Margin = new Thickness(3) });
            bublCombo.Items.Add(new TextBlock() { Text = ((char)176).ToString(), Tag = "Circle", Margin = new Thickness(3) });
            bublCombo.Items.Add(new TextBlock() { Text = ((char)183).ToString(), Tag = "Disc", Margin = new Thickness(3) });
            bublCombo.Items.Add(new TextBlock() { Text = "■", Tag = "Box", Margin = new Thickness(3) });
            bublCombo.Items.Add(new TextBlock() { Text = "□", Tag = "Square", Margin = new Thickness(3) });
        }

        public static Dictionary<int, double> FontSize = new Dictionary<int, double>();

        public static Dictionary<int, double> MarginWigth = new Dictionary<int, double>();
        public static Dictionary<int, double> Spacing = new Dictionary<int, double>();

    }
}
