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
    /// Логика взаимодействия для DictionaryPanel.xaml
    /// </summary>
    public partial class DictionaryPanel : UserControl
    {
        public DictionaryPanel()
        {
            InitializeComponent();
        }
        public string defenition="";
        public string synonimus="";
        private void ModeDictionary_Checked(object sender, RoutedEventArgs e)
        {
            var but = sender as RadioButton;
            if (but == null) return;
            if(but.Name == "ModeDictionary")
            {
                DictionaryResult.Text = defenition;
            }
            else if(but.Name == "ModeThesaurus")
            {
                DictionaryResult.Text = synonimus;
            }
           // but.t
        }
    }
}
