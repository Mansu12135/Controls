using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using HTMLConverter;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для InformationForm.xaml
    /// </summary>
    public partial class InformationForm : Window
    {
        public InformationForm()
        {
            InitializeComponent();
            }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

    }
}
