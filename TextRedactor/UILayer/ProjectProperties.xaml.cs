using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ApplicationLayer;
using UserControl = System.Windows.Controls.UserControl;

namespace UILayer
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
                proj.Files.ForEach(r => listOfFiles.Add(new ListOfFiles() { FileName = r.Name, FilePath = r.Name }));
                FileList.ItemsSource = listOfFiles;
                //     FileBrowser.dataGrid.ItemsSource = listOfFiles;
                textBoxProjectName.Text = propertiesForm.value.Name;
                //  FileBrowser.projectProperties = this;
                Author.Text = proj.Author;
                DateOfCreating.Text = proj.CreateDate.ToShortDateString();
                DateOfPublishing.Text = "-";
                Words.Text = "-";
                Symbols.Text = "-";

                //property.Add(new Property("Author", proj.Author));
                //property.Add(new Property("Date of creating", proj.CreateDate.ToShortDateString()));
                //property.Add(new Property("Date of publishing", proj.PublishingDate.ToString()));
                //  DetailOfProject.ItemsSource = property;
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
                //FileBrowser.dataGrid.IsReadOnly = false;
                //FileBrowser.dataGrid.CurrentCell = new DataGridCellInfo(
                //    FileBrowser.dataGrid.Items[FileBrowser.dataGrid.Items.Count - 1], FileBrowser.dataGrid.Columns[0]);
                //FileBrowser.dataGrid.BeginEdit();
                //FileProperties.IsEnabled = false;
            }
        }

        private void ImportFile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckPathExists = true;
            dialog.Filter = "TextFile (*.rtf)|*.rtf";
            dialog.InitialDirectory = ((BrowseProject)propertiesForm.CalledControl).ProjectsPath + "\\" + propertiesForm.value.Name;
            var rezult = dialog.ShowDialog();
            if (rezult != DialogResult.OK) { return; }
            propertiesForm.AddTask(new System.Threading.Tasks.Task(
          () =>
          {
              ((BrowseProject)propertiesForm.CalledControl).AddFileToProject(propertiesForm.value.Name, dialog.FileName);
          }));
            listOfFiles.Add(new ListOfFiles() { FileName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName), FilePath = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName) });
            FileList.Items.Refresh();
        }

        private void textBoxFileName_LostFocus(object sender, RoutedEventArgs e)
        {
            var text = sender as System.Windows.Controls.TextBox;
            if (text == null || text.Tag == null) return;
            string oldName = text.Tag.ToString();
            string newName = text.Text;
            if (text.Text == oldName) return;
            propertiesForm.AddTask(new System.Threading.Tasks.Task(
               () =>
               {
                   ((BrowseProject)propertiesForm.CalledControl).RenameFileInProject(propertiesForm.value.Name,
                   ((Project)propertiesForm.value).Files.Find(r => r.Name == oldName),
                  newName);
               }));
        }

        private void textBoxFileName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //     var text = sender as System.Windows.Controls.TextBox;
            //    if (text == null || text.Tag == null) return;
            if (e.Key == Key.Enter)
            {
                FileList.Focus();
                // text.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void DelFile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            if (image == null || image.Tag == null) return;
            string fileName = image.Tag.ToString();
            propertiesForm.AddTask(new System.Threading.Tasks.Task(
            () =>
            {
                var project = (Project)propertiesForm.value;
                if (project == null || string.IsNullOrEmpty(fileName)) { return; }
                string fullFileName = project.ListFiles.Find(file => file.Name == fileName).Path;
                if (string.IsNullOrEmpty(fullFileName)) { return; }
                ((BrowseProject)propertiesForm.CalledControl).DeleteFile(project.Name, fullFileName);
            }));
            listOfFiles.Remove(listOfFiles.Where(item => item.FilePath == fileName).First());
            FileList.Items.Refresh();
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
