using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для FilesBrowser.xaml
    /// </summary>
    public partial class FilesBrowser : System.Windows.Controls.UserControl
    {
        public FilesBrowser()
        {
            InitializeComponent();
        }
        public ProjectProperties projectProperties;

        private void DeleteFile_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Border border = sender as Border;
            if (border == null) return;
            switch (border.Tag.ToString())
            {
                case "del":
                    string fileName = ((ListOfFiles)dataGrid.SelectedItem).FileName;
                    projectProperties.propertiesForm.AddTask(new System.Threading.Tasks.Task(
                    () =>
                    {
                        var project = (Project)projectProperties.propertiesForm.value;
                        if (project == null || string.IsNullOrEmpty(fileName)) { return; }
                        string fullFileName = project.ListFiles.Find(file => file.Name == fileName).Path;
                        if (string.IsNullOrEmpty(fullFileName)) { return; }
                        ((BrowseProject)projectProperties.propertiesForm.CalledControl).DeleteFile(project.Name, fullFileName);
                    }));
                    projectProperties.listOfFiles.Remove((ListOfFiles)dataGrid.SelectedItem);
                    dataGrid.CommitEdit();
                    dataGrid.Items.Refresh();
                    break;
                case "add":
                    var dialog = new OpenFileDialog();
                    dialog.CheckPathExists = true;
                    dialog.Filter = "TextFile (*.rtf)|*.rtf";
                    dialog.InitialDirectory = ((BrowseProject)projectProperties.propertiesForm.CalledControl).ProjectsPath + "\\" + projectProperties.propertiesForm.value.Name;
                    var rezult = dialog.ShowDialog();
                    if (rezult != DialogResult.OK) { return; }
                    projectProperties.propertiesForm.AddTask(new System.Threading.Tasks.Task(
                  () =>
                  {
                      ((BrowseProject)projectProperties.propertiesForm.CalledControl).AddFileToProject(projectProperties.propertiesForm.value.Name, dialog.FileName);
                  }));
                    ListOfFiles last = projectProperties.listOfFiles.FindLast(r => r.Tags == "add");
                    projectProperties.listOfFiles.RemoveAt(projectProperties.listOfFiles.Count - 1);
                    projectProperties.listOfFiles.Add(new ListOfFiles() { FileName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName), imageSources = "Resources/close_icon.png", Tags = "del", FilePath = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName) });
                    projectProperties.listOfFiles.Add(last);
                    dataGrid.CommitEdit();
                    dataGrid.Items.Refresh();
                    break;
            }
        }

        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            System.Windows.Controls.TextBox txt = e.EditingElement as System.Windows.Controls.TextBox;
            if (txt == null) return;
            string newName = txt.Text;
            ListOfFiles file = (ListOfFiles)e.Row.DataContext;
            if (newName == System.IO.Path.GetFileNameWithoutExtension(file.FilePath)) { return; }
            switch (file.Tags)
            {
                case "del":
                    projectProperties.propertiesForm.AddTask(new System.Threading.Tasks.Task(
               () =>
               {
                   ((BrowseProject)projectProperties.propertiesForm.CalledControl).RenameFileInProject(projectProperties.propertiesForm.value.Name,
                   ((Project)projectProperties.propertiesForm.value).Files.Find(r => r.Name == file.FilePath),
                   newName);
               }));
                    break;
                case "add":
                    projectProperties.listOfFiles.Remove(file);
                    projectProperties.listOfFiles.Add(new ListOfFiles() { FileName = newName, imageSources = "Resources/close_icon.png", Tags = "del", FilePath = newName });
                    file.FileName = "";
                    projectProperties.listOfFiles.Add(file);
                    projectProperties.propertiesForm.AddTask(new System.Threading.Tasks.Task(
                     () =>
                     {
                         ((BrowseProject)projectProperties.propertiesForm.CalledControl).AddNewFileToProject(projectProperties.propertiesForm.value.Name, newName);
                     }));
                    break;
            }
            dataGrid.Dispatcher.BeginInvoke(new Action(() => dataGrid.Items.Refresh()), System.Windows.Threading.DispatcherPriority.Background);

        }

        private void dataGrid_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                dataGrid.CancelEdit();
                dataGrid.Items.Refresh();
            }
        }
    }

}
