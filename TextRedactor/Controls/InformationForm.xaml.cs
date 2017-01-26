using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using HTMLConverter;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для InformationForm.xaml
    /// </summary>
    public partial class InformationForm : Window
    {
        public InformationForm()
        {
            InitializeComponent();
            TextRange tr = new TextRange(Info.Document.ContentStart, Info.Document.ContentEnd);
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(HtmlToXamlConverter.ConvertHtmlToXaml(TextRedactor.ResourceManager.GetString("Information"), false))))
            {
                tr.Load(ms, DataFormats.Xaml);
            }
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
