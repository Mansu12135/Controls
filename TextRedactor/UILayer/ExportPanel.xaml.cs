using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ApplicationLayer;
using Microsoft.Win32;
using UILayer;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using System.Windows.Media.Animation;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для ExportPanel.xaml
    /// </summary>
    public partial class ExportPanel : System.Windows.Controls.UserControl
    {
        public Project project;
        private BaseRichTextBox document;
        private SuperTextRedactor parentControl;
        public ExportPanel()
        {
            InitializeComponent();
        }

        public void Init(SuperTextRedactor parent)
        {
            parentControl = parent;
            document = parentControl.TextBox.MainControl;
            TextBoxAuthor.Text = project.Author;
            TextBoxTitle.Text = project.Name;
            //ExportTextBox.Document = new FlowDocument();
            //foreach (var doc in project.Files)
            //{
            //    FlowDocument paragraph = new FlowDocument();
            //    //using (var fStream = new FileStream(doc.Path, FileMode.OpenOrCreate, FileAccess.Read))
            //    //{
            //    try
            //    {
            //        FileWorkerManager.Do(paragraph, doc.Path, false);
            //        //  new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Load(fStream, System.Windows.DataFormats.Rtf);
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Windows.MessageBox.Show(ex.Message);
            //        return;
            //    }
            //    //   }
            //    ExportTextBox.Document.Blocks.AddRange(paragraph.Blocks.ToList());
            //}

        }

        private void ButAddCover_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckPathExists = true;
            dialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.bmp;*.jpg;*.PNG";
            // dialog.InitialDirectory = ((BrowseProject)projectProperties.propertiesForm.CalledControl).ProjectsPath + "\\" + projectProperties.propertiesForm.value.Name;
            var rezult = dialog.ShowDialog();
            if (rezult != DialogResult.OK) { return; }
            ImageName.Tag = dialog.FileName;
            ImageName.Content = System.IO.Path.GetFileName(dialog.FileName);
            DeleteImage.Visibility = Visibility.Visible;
        }

        private void DeleteImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ImageName.Tag = null;
            ImageName.Content = "";
            DeleteImage.Visibility = Visibility.Hidden;
        }

        private void ButExport_Click(object sender, RoutedEventArgs e)
        {
            if(CheckEpub.IsChecked== false && CheckMobi.IsChecked == false) { return; }
            string folderPath = "";
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                folderPath = folderBrowserDialog.SelectedPath;
                ExportInfo exportInfo = new ExportInfo()
                {
                    Title = TextBoxTitle.Text,
                    Author = TextBoxAuthor.Text,
                    DatePublish = TextBoxPublishingDate.SelectedDate,
                    SavePath = folderPath,
                    ImagePath = (ImageName.Tag == null) ? "" : ImageName.Tag.ToString()
                };
                if (CheckEpub.IsChecked == true)
                {
                    document.SaveAsEpub(exportInfo, project.Files);
                }
                if (CheckMobi.IsChecked == true)
                {
                    document.SaveAsMobi(exportInfo, project.Files);
                }
            }
        }
        public void Show()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = Margin;
            myThicknessAnimation.To = new Thickness(-Margin.Left, Margin.Top, 0, Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            BeginAnimation(MarginProperty, myThicknessAnimation);
         
        }

        public void Hide()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = Margin;
            myThicknessAnimation.To = new Thickness(-Margin.Left, Margin.Top, Margin.Left, Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            myThicknessAnimation.Completed += MyThicknessAnimation_Completed;
            BeginAnimation(MarginProperty, myThicknessAnimation);
          
        }

        private void MyThicknessAnimation_Completed(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, Project> item in parentControl.BrowseProject.MainProjectList.Items)
            {
                if (item.Value != parentControl.BrowseProject.CurentProject)
                {
                    ((ListBoxItem)parentControl.BrowseProject.MainProjectList.ItemContainerGenerator.ContainerFromItem(item)).IsEnabled = true;
                }
            }
            parentControl.Format.IsEnabled = true;
            parentControl.TextBox.IsEnabled = true;
            parentControl.MainContainer.Children.Remove(this);
        }
    }
    public class ExportInfo
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string SavePath { get; set; }
        public string ImagePath { get; set; }
        public DateTime? DatePublish { get; set; }

    }
}
