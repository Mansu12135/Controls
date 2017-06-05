using System;
using System.Threading.Tasks;
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
        public void Init()
        {
            ComboFont.SelectedValue = ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultFont;
            ComboMargin.SelectedValue = ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultMarginWight;
            ComboSpacing.SelectedValue = ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultSpacing;
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

        private void ComboFont_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboFont.SelectedValue == null) return;
            double font = Convert.ToDouble(ComboFont.SelectedValue);

            ParentControl.AddTask(new Task(
            () =>
            {
                if(font != ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultFont)
                {
                    ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultFont = font;
                }
            }));
        }

        private void ComboMargin_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            if (ComboMargin.SelectedValue == null) return;
            double margin = Convert.ToDouble(ComboMargin.SelectedValue);

            ParentControl.AddTask(new Task(
            () =>
            {
                if (margin != ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultMarginWight)
                {
                    ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultMarginWight = margin;
                }
            }));
        }

        private void ComboSpacing_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            if (ComboSpacing.SelectedValue == null) return;
            double spacing = Convert.ToDouble(ComboSpacing.SelectedValue);

            ParentControl.AddTask(new Task(
            () =>
            {
                if (spacing != ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultSpacing)
                {
                    ((BrowseProject)ParentControl.CalledControl).ParentControl.defaultSpacing = spacing;
                  //  Dispatcher.Invoke(() => { ((BrowseProject)ParentControl.CalledControl).SetLineSpacing(); });
                   
                }
            }));
        }
    }
}
