using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для ProjectProperties.xaml
    /// </summary>
    public partial class ProjectProperties : UserControl
    {
        public PropertiesForm propertiesForm;
        public List<ListOfFiles> listOfFiles;
        public List<Property> property;
        public bool isCreateFile;
        public ProjectProperties()
        {
            InitializeComponent();
        }
        public void Init()
        {
            listOfFiles = new List<ListOfFiles>();
            property = new List<Property>();
            var control = propertiesForm.CalledControl as BrowseProject;
            if (control != null)
            {
                var proj = control.Notes[propertiesForm.value.Name];
                proj.Files.ForEach(r => listOfFiles.Add(new ListOfFiles() { FileName = r.Name, imageSources = "Resources/close_icon.png", Tags = "del", FilePath = r.Name }));
                listOfFiles.Add(new ListOfFiles() { FileName = "", imageSources = "Resources/picture.png", Tags = "add", FilePath = "" });
                FileBrowser.dataGrid.ItemsSource = listOfFiles;
                textBoxProjectName.Text = propertiesForm.value.Name;
                FileBrowser.projectProperties = this;
                property.Add(new Property("Author", proj.Author));
                property.Add(new Property("Date of creating", proj.CreateDate.ToShortDateString()));
                property.Add(new Property("Date of publishing", proj.PublishingDate.ToString()));
                DetailOfProject.ItemsSource = property;
            }
        }

        private void DeleteProject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show("Delete current project?",
                     "Confirm the action", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                ((BrowseProject)propertiesForm.CalledControl).DeleteProject(textBoxProjectName.Text);
                propertiesForm.Close();
            }
        }

        private void FileProperties_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //FileBrowser.dataGrid.IsReadOnly = false;
            //FileBrowser.dataGrid.Focus();
            //FileBrowser.dataGrid.UpdateLayout();
            //((Image)sender).IsEnabled = false;
        }

        private void textBoxProjectName_LostFocus(object sender, RoutedEventArgs e)
        {
            string newName = textBoxProjectName.Text;
            string oldname = propertiesForm.value.Name;
            if (newName != oldname)
            {
                propertiesForm.AddTask(new Task(
            () =>
            {
                ((BrowseProject)propertiesForm.CalledControl).RenameProject(oldname, newName);
            }));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (isCreateFile)
            {
                FileBrowser.dataGrid.IsReadOnly = false;
                FileBrowser.dataGrid.CurrentCell = new DataGridCellInfo(
                    FileBrowser.dataGrid.Items[FileBrowser.dataGrid.Items.Count - 1], FileBrowser.dataGrid.Columns[0]);
                FileBrowser.dataGrid.BeginEdit();
                FileProperties.IsEnabled = false;
            }
        }
    }
    public class ListOfFiles
    {
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set { if (Tags != "add") fileName = value; }
        }
        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }
        private string imageSource;
        public string imageSources
        {
            get { return imageSource; }
            set { imageSource = value; }
        }
        private string tag;
        public string Tags
        {
            get { return tag; }
            set { tag = value; }
        }
    }
    public class Property
    {
        public Property(string pName, string pValue)
        {
            Name = pName;
            Value = pValue;
        }
        private string name;
        private string value;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
