using System.Windows.Forms;
using System.Windows.Input;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для Options.xaml
    /// </summary>
    public partial class Options : System.Windows.Controls.UserControl
    {
        internal PropertiesForm ParentControl;
        public Options()
        {
            InitializeComponent();
        }

        private void ChangeDirectory_MouseUp(object sender, MouseButtonEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    DirectoryPath.Text = fbd.SelectedPath;
                    ((BrowseProject)ParentControl.CalledControl).ProjectsPath = DirectoryPath.Text;
                }
            }
        }
    }
}
