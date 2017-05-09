using System.Windows;
using System.Windows.Controls;
using ApplicationLayer;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для DictionaryPanel.xaml
    /// </summary>
    public partial class DictionaryPanel : BasicPanel<Item>
    {
        public DictionaryPanel()
        {
            InitializeComponent();
        }
        public string defenition = "";
        public string synonimus = "";
        private void ModeDictionary_Checked(object sender, RoutedEventArgs e)
        {
            var but = sender as RadioButton;
            if (but == null) return;
            if (but.Name == "ModeDictionary")
            {
                DictionaryResult.Text = defenition;
            }
            else if (but.Name == "ModeThesaurus")
            {
                DictionaryResult.Text = synonimus;
            }
            // but.t
        }
    }
}
