﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Windows.Markup;
using ApplicationLayer;
using Gma.UserActivityMonitor;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для BrowseProject.xaml
    /// </summary>
    public partial class BrowseProject : BasicPanel<Project>
    {
        internal string LoadedFile { get; private set; }
        public Project CurentProject;
        public string CurentFile;
        private bool IsChangeFileName = false;
        private bool ChangedFileName;

        public BrowseProject() : base()
        {
            InitializeComponent();
            OnInitialize();
        }
        public override void Disposing()
        {
            ((IFileSystemControl)this).FSWorker.Dispose();
            if (string.IsNullOrEmpty(LoadedFile)) { return; }
            ParentControl.BrowseProject.UpdateOffsetOnNotes();
            ParentControl.NotesBrowser.CloseNotes(LoadedFile);
        }

        //internal string Test(string path)
        //{
        //    var document = new FlowDocument();
        //    var t = new TextRange(document.ContentStart, document.ContentEnd);
        //    using (var stream = File.OpenRead(path))
        //    {
        //        t.Load(stream,
        //            DataFormats.XamlPackage);
        //    }
        //    var a = XamlWriter.Save(document);
        //    return a;
        //}
        //public void Dispose()
        //{
        //    ((IFileSystemControl)this).FSWorker.Dispose();
        //    if (string.IsNullOrEmpty(LoadedFile)) { return; }
        //    ParentControl.BrowseProject.UpdateOffsetOnNotes();
        //    ParentControl.NotesBrowser.CloseNotes(LoadedFile);
        //}

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public override string ProjectsPath
        {
            get { return vProjectPath; }
            set
            {
                if (!string.IsNullOrEmpty(vProjectPath))
                {
                    value = value[value.Length - 1].ToString() == @"\" ? value : value + "\\";
                    string newValue = value + "TextRedactor\\";
                    CopyAll(new DirectoryInfo(vProjectPath).Parent, new DirectoryInfo(newValue));
                    Directory.Delete(Directory.GetParent(vProjectPath).FullName, true);
                    newValue += "MyProjects";
                    foreach (var project in ItemsCollection)
                    {
                        project.Value.Files.ForEach(item => item.Path = item.Path.Replace(vProjectPath, newValue));
                        CreateProjectFile(project.Value, newValue + "\\" + project.Key + "\\" + project.Key + ".prj");
                    }
                    LoadedFile = string.IsNullOrEmpty(LoadedFile) ? "" : LoadedFile.Replace(vProjectPath, newValue);
                    CurentFile = string.IsNullOrEmpty(CurentFile) ? "" : CurentFile.Replace(vProjectPath, newValue);
                    RefreshNotes(new Dictionary<string, Project>(ItemsCollection));
                    value = newValue;
                }
                vProjectPath = value;
                if (!Directory.Exists(vProjectPath))
                {
                    Directory.CreateDirectory(vProjectPath);
                    ItemsCollection.Clear();
                }
            }
        }

        internal void DelFlag(Note note)
        {
            //  UpdateNoteAfterOpening();
        }
        public void OpenFirstFile()
        {
            if (Notes.Count > 0)
            {
                var proj = Notes.Where(item => item.Value.Files.Count > 0).FirstOrDefault();
                if (proj.Value == null)
                {
                    LoadLogo();
                }
                else
                {
                    if (ParentControl.TextBox.MainControl.IsReadOnly)
                    {
                        ParentControl.TextBox.MainControl.IsReadOnly = false;
                        ParentControl.TextBox.CountLeterInfo.Visibility = Visibility.Visible;
                        ParentControl.TextBox.Logo.Visibility = Visibility.Collapsed;

                        ParentControl.Format.IsEnabled = true;
                    }
                    OpenFile(proj.Value.Files[0].Path, Path.GetFileNameWithoutExtension(proj.Value.Files[0].Path));
                    CurentProject = proj.Value;
                    if (!string.IsNullOrEmpty(LoadedFile))
                    {
                        UpdateRangeOnNotes();
                    }
                    MainProjectList.Items.Refresh();
                    if (ParentControl.Format.comboWigth.SelectedValue != null)
                    {
                        ParentControl.TextBox.MainControl.Document.PageWidth = Convert.ToDouble(ParentControl.Format.comboWigth.SelectedValue);
                    }
                    if (ParentControl.Format.comboBoxFont.SelectedValue != null)
                    {
                        ParentControl.Format.comboBoxFont.SelectedIndex = ParentControl.Format.comboBoxFont.Items.IndexOf(ParentControl.defaultFont);
                    }
                }
            }
            else
            {
                LoadLogo();
            }

        }
        private string vProjectPath = "";
        private void LoadLogo()
        {
            ParentControl.TextBox.MainControl.IsReadOnly = true;
            ParentControl.Format.IsEnabled = false;
            ParentControl.TextBox.CountLeterInfo.Visibility = Visibility.Hidden;
            ParentControl.TextBox.Logo.Visibility = Visibility.Visible;
            ParentControl.TextBox.ButAddProj.MouseUp -= AddProjectFromMainScreen;
            ParentControl.TextBox.ButAddProj.MouseUp += AddProjectFromMainScreen;
            ParentControl.TextBox.MainControl.Document.Blocks.Clear();


        }

        private void AddProjectFromMainScreen(object sender, MouseButtonEventArgs e)
        {
            OnCreateProject(sender, new ProjectArgs(GenerateName("NewProject", ProjectsPath), Happened.Created, Callback));

        }

        public BrowseProject(Dictionary<string, Project> collection)
        {
            InitializeComponent();
            ItemsCollection = collection;
        }

        private void OnFileOpen(string projectPath, string file)
        {
            if (!string.IsNullOrEmpty(LoadedFile)) { ParentControl.NotesBrowser.CloseNotes(LoadedFile); }
            ParentControl.NotesBrowser.LoadNotes(projectPath + System.IO.Path.GetFileNameWithoutExtension(file) + ".not");
            ParentControl.NotesBrowser.MainControl.Items.Refresh();
        }

        public void RefreshNotes(Dictionary<string, Project> collection)
        {
            Notes.Clear();
            foreach (var note in collection)
            {
                AddItem(note.Value);
            }
            MainProjectList.Items.Refresh();
        }
        private void buttonAddFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string projName = ((TextBlock)sender).Tag.ToString();
            string path = ProjectsPath + "\\" + projName + "\\Files";
            OnCreateFiles(sender, new FileArgs(new List<string>() { GenerateName("Chapter", ProjectsPath + "\\" + projName + "\\Files", false) }, projName, Happened.Created, Callback));
            //AddNewFileToProject(projName, GenerateName("Chapter", ProjectsPath + "\\" + projName + "\\Files", false));
            // MainProjectList.Items.Refresh();
        }

        private void CreateProjectFile(Project project, string path)
        {
            using (FileStream fs = File.Create(path))
            {
                BinaryFormatter write = new BinaryFormatter();
                write.Serialize(fs, project);
            }
        }

        internal override string GenerateName(string name, string path, bool isProg = true)
        {
            string generattingName = name;
            int i = 0;
            if (isProg)
            {
                List<DirectoryInfo> directories = new DirectoryInfo(path).GetDirectories().ToList();
                while (true)
                {
                    if (directories.FindIndex(dir => dir.Name == generattingName) < 0) { return generattingName; }
                    i++;
                    generattingName = name + i;
                }
            }
            else
            {
                List<FileInfo> directories = new DirectoryInfo(path).GetFiles().ToList();
                while (true)
                {
                    if (directories.FindIndex(dir => Path.GetFileNameWithoutExtension(dir.Name) == generattingName) < 0) { return generattingName; }
                    i++;
                    generattingName = name + i;
                }
            }
        }
        public void CreateProject()
        {
            if (!Directory.Exists(ProjectsPath))
            {
                Directory.CreateDirectory(ProjectsPath);
            }

            string name = GenerateName("NewProject", ProjectsPath);
            string path = ProjectsPath + "\\" + name;
            if (File.Exists(path)) { return; }
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "\\Files");
            var project = new Project(name);

            AddItem(project);
            CreateProjectFile(project, path + "\\" + name + ".prj");
            MainProjectList.Items.Refresh();
        }

        public void AddNewFileToProject(string project, string file)
        {
            string path = ProjectsPath + "\\" + project + "\\Files\\" + file + ".xaml";
            if (File.Exists(path)) { return; }
            OnCreatedFiles(new object(), new FileArgs(new List<string> { file }, project, Happened.Created, EndFilesCreated));
        }

        private void EndFilesCreated(bool rezult, string message, EventArgs args)
        {
            var fileArgs = args as FileArgs;
            foreach (string file in fileArgs.Files)
            {
                Notes[fileArgs.Project].Files.Add(new LoadedFile(Path.Combine(ProjectsPath + "\\" + fileArgs.Project, "Files", file), ProjectsPath + "\\" + fileArgs.Project));
            }
        }

        public void RenameProject(string project, string newName)
        {
            OnRenamedProject(new object(), new ProjectArgs(new RenamedArgs(project, newName, OnRenamed)));
            //if (!Notes.ContainsKey(project)) { return; }
            //if (System.IO.Directory.Exists(ProjectsPath + "\\" + newName))
            //{
            //    MessageBox.Show("Project with the same name already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //Directory.Move(ProjectsPath + "\\" + project, ProjectsPath + "\\" + newName);
            //var newProject = new Project(newName, Notes[project].CreateDate);
            //foreach (var file in Notes[project].ListFiles)
            //{
            //    newProject.Files.Add(new LoadedFile(ProjectsPath + "\\" + newName + "\\Files\\" + System.IO.Path.GetFileName(file.Path), ProjectsPath + "\\" + newName));
            //}
            //newProject.Author = Notes[project].Author;
            //newProject.PublishingDate = Notes[project].PublishingDate;
            //Notes.Remove(project);
            //Notes.Add(newName, newProject);
            //File.Delete(ProjectsPath + "\\" + newName + "\\" + project + ".prj");
            //CreateProjectFile(Notes[newName], ProjectsPath + "\\" + newName + "\\" + newName + ".prj");
            //if (!String.IsNullOrEmpty(LoadedFile))
            //{
            //    string lastFile = System.IO.Path.GetFileName(LoadedFile);
            //    LoadedFile = ProjectsPath + "\\" + newName + "\\Files\\" + lastFile;
            //}
            //((ISettings)ParentControl.Parent).SaveSettings();
            //if (propertForm == null) { return; }
            //propertForm.value.Name = newName;
            //((Project)propertForm.value).Files = newProject.Files;
        }

        private void OnRenamed(bool rezult, string message, EventArgs args)
        {
            var projectArgs = args as ProjectArgs;
            string newName = projectArgs.RenamedArgs.To;

            List<LoadedFile> files = new List<LoadedFile>();
            Notes[newName].Files.ForEach(item => files.Add(item));
            Notes[newName].Files.Clear();
            foreach (var file in files)
            {
                if (file.IsOpen > 0)
                {
                    ParentControl.TextBox.MainControl.FilePath = ProjectsPath + "\\" + newName + "\\Files\\" + System.IO.Path.GetFileName(file.Path);
                }
                Notes[newName].Files.Add(new LoadedFile(ProjectsPath + "\\" + newName + "\\Files\\" + System.IO.Path.GetFileName(file.Path), ProjectsPath + "\\" + newName, file.IsOpen));
            }
            if (!string.IsNullOrEmpty(LoadedFile))
            {
                string lastFile = Path.GetFileName(LoadedFile);
                LoadedFile = ProjectsPath + "\\" + newName + "\\Files\\" + lastFile;
            }
            ((ISettings)ParentControl.Parent).SaveSettings();
            if (propertForm != null)
            {
                propertForm.value.Name = newName;
            }
            MainProjectList.Items.Refresh();
        }

        protected override object OnSave(Action action, string project)
        {
            action.Invoke();
            if (!Notes.ContainsKey(project)) { Notes.Add(project, new Project(project)); }
            return Notes[project];
            //CreateProjectFile(Notes[project], ProjectsPath + "\\" + project + "\\" + project + ".prj");
        }

        public void DeleteFile(string project, string filePath)
        {
            int index = Notes[project].Files.FindIndex(item => item.Path == filePath);
            if (index < 0) { return; }
            var file = Notes[project].Files[index];
            if (LoadedFile == System.IO.Path.GetDirectoryName(file.Path) + "\\" + file.Name + ".not") { LoadedFile = ""; }
            File.Delete(file.Path);
            Notes[project].Files.RemoveAt(index);
            if (!Dispatcher.CheckAccess()) { return; }
            OnDeleted(project);
        }

        internal void OnDeleted(string project)
        {
            if (Notes[project].Files.Count > 0)
            {
                OpenFile(Notes[project].Files[0].Path, Path.GetFileNameWithoutExtension(Notes[project].Files[0].Path));
                if (!string.IsNullOrEmpty(LoadedFile))
                {
                    UpdateRangeOnNotes();
                }
            }
            else
            {
                var proj = Notes.Where(item => item.Value.Files.Count > 0).FirstOrDefault();
                if (proj.Value == null)
                {
                    CurentProject = null;
                    LoadLogo();
                }
                else
                {
                    OpenFile(proj.Value.Files[0].Path, Path.GetFileNameWithoutExtension(proj.Value.Files[0].Path));
                    CurentProject = proj.Value;
                    if (!string.IsNullOrEmpty(LoadedFile))
                    {
                        UpdateRangeOnNotes();
                    }
                }
            }
        }

        public void DeleteProject(string project)
        {
            if (!Notes.ContainsKey(project)) { return; }

            if (!string.IsNullOrEmpty(LoadedFile) && LoadedFile.StartsWith(ProjectsPath + "\\" + project)) { LoadedFile = ""; }
            if (!string.IsNullOrEmpty(CurentFile) && CurentFile.StartsWith(ProjectsPath + "\\" + project))
            {
                // CurentProject = No
                CurentFile = "";
            }

            Directory.Delete(ProjectsPath + "\\" + project, true);
            Notes.Remove(project);

            if (Notes.Count == 0)
            {
                CurentProject = null;
                ParentControl.TextBox.MainControl.FilePath = string.Empty;
                LoadLogo();
            }
            else
            {
                var proj = Notes.Where(item => item.Value.Files.Count > 0).FirstOrDefault();
                if (proj.Value == null)
                {
                    CurentProject = null;
                    ParentControl.TextBox.MainControl.FilePath = string.Empty;
                    LoadLogo();
                    return;
                }
                OpenFile(proj.Value.Files[0].Path, Path.GetFileNameWithoutExtension(proj.Value.Files[0].Path));
                CurentProject = proj.Value;
                if (!string.IsNullOrEmpty(LoadedFile))
                {
                    UpdateRangeOnNotes();
                }
                MainProjectList.Items.Refresh();
            }

        }

        public void RenameFileInProject(string project, LoadedFile file, string newFile)
        {
            if (System.IO.Path.GetFileNameWithoutExtension(LoadedFile) == file.Name) LoadedFile = System.IO.Path.GetDirectoryName(LoadedFile) + "\\" + newFile + ".not";
            int index = Notes[project].Files.FindIndex(item => item.Path == file.Path);
            if (index < 0) { return; }
            OnRenamedFiles(new object(), new FileArgs(project, new RenamedArgs(file.Name, newFile, EndRenameFile)));
        }

        private void EndRenameFile(bool rezult, string message, EventArgs args)
        {
            var renamedArgs = args as FileArgs;
            string project = renamedArgs.Project;
            int index = Notes[project].Files.FindIndex(item => item.Name == renamedArgs.RenamedArgs.From);
            if (index < 0) { return; }
            Notes[project].Files[index] = new LoadedFile(Path.Combine(ProjectsPath, project, "Files") + "\\" + renamedArgs.RenamedArgs.To + ".xaml", ProjectsPath + "\\" + project, Notes[project].Files[index].IsOpen);//EXTENSION
            OnSave(() => { }, project);
            ((ISettings)ParentControl.Parent).SaveSettings();
            MainProjectList.Items.Refresh();
        }

        public void AddFileToProject(string project, string filePath)
        {
            if (!Notes.ContainsKey(project)) { return; }
            File.Copy(filePath, ProjectsPath + "\\" + project + "\\Files\\" + System.IO.Path.GetFileName(filePath));
            Notes[project].Files.Add(new LoadedFile(filePath, ProjectsPath + "\\" + project));
        }

        private void ButAddProject_MouseDown(object sender, RoutedEventArgs e)
        {
            OnCreateProject(sender, new ProjectArgs(GenerateName("NewProject", ProjectsPath), Happened.Created, Callback));
        }

        internal void OpenFile(string path, string content)
        {
            if (!string.IsNullOrEmpty(LoadedFile))
            {
                UpdateOffsetOnNotes();
            }
            foreach (var item in Notes.Values)
            {
                for (int i = 0; i < item.Files.Count; i++)
                {
                    item.ListFiles[i].IsOpen = (item.Files[i].Path == path) ? 10 : 0;
                }
                //item.Files.ForEach(x => x.IsOpen = (x.Path == path) ? 10 : 0);
            }
            ParentControl.TextBox.MainControl.FilePath = "";
            try
            {
                //can write Blocks.Clear;
                ParentControl.TextBox.MainControl.Document = new FlowDocument();
                var range = new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, ParentControl.TextBox.MainControl.Document.ContentEnd);
                using (var fStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    range.Load(fStream, DataFormats.XamlPackage);
                    string folder = Path.GetDirectoryName(path);
                    OnFileOpen(folder + "\\", content);
                    LoadedFile = folder + "\\" + content + ".not";
                    CurentFile = path;
                    CurentProject = Notes[Directory.GetParent(folder).Name];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            ParentControl.TextBox.MainControl.FilePath = path;
            ParentControl.TextBox.MainControl.UpdateWordCount();
            ParentControl.updateDefaultValue();
            ParentControl.TextBox.UpdateLayout();
        }
        private void buttonOpenFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsChangeFileName)
            {
                IsChangeFileName = false;
                return;
            }
            if (ParentControl.TextBox.MainControl.IsReadOnly)
            {
                ParentControl.TextBox.MainControl.IsReadOnly = false;
                ParentControl.TextBox.CountLeterInfo.Visibility = Visibility.Visible;
                ParentControl.TextBox.Logo.Visibility = Visibility.Collapsed;

                ParentControl.Format.IsEnabled = true;
            }
            //if (ParentControl.searchPanel != null)
            //{
            //    ParentControl.searchPanel.ClearSearch();
            //}
            var textBlock = (System.Windows.Controls.Label)sender;
            if (textBlock == null) return;
            if (textBlock.Tag == null) return;
            if (CurentFile == textBlock.Tag.ToString()) { return; }
            OpenFile(textBlock.Tag.ToString(), Path.GetFileNameWithoutExtension(textBlock.Tag.ToString()));
            if (!string.IsNullOrEmpty(LoadedFile))
            {
                UpdateRangeOnNotes();
            }
            //preview for export
            //maybe do with event
            if (ParentControl.exportPanel != null)
            {
                ParentControl.exportPanel.LoadPreview(textBlock.Tag.ToString());
            }
            MainProjectList.Items.Refresh();
            if (ParentControl.Format.comboWigth.SelectedValue != null)
            {
                ParentControl.TextBox.MainControl.Document.PageWidth = Convert.ToDouble(ParentControl.Format.comboWigth.SelectedValue);
            }
            if (ParentControl.Format.comboBoxFont.SelectedValue != null)
            {
                ParentControl.Format.comboBoxFont.SelectedIndex = ParentControl.Format.comboBoxFont.Items.IndexOf(ParentControl.defaultFont);
            }
        }

        public void UpdateRangeOnNotes()
        {
            foreach (var note in ParentControl.NotesBrowser.Notes)
            {
                var end =
                    ParentControl.TextBox.MainControl.Document.ContentStart.GetPositionAtOffset(note.Value.OffsetEnd);
                note.Value.Range = new TextRange(
                    ParentControl.TextBox.MainControl.Document.ContentStart.GetPositionAtOffset(note.Value.OffsetStart),
                    end != null ? end : ParentControl.TextBox.MainControl.Document.ContentEnd);
                var pointer = ParentControl.AddFlag(note.Value.Range, note.Value.Name);
                if (note.Value.Range.Start != pointer)
                {
                    note.Value.Range = new TextRange(pointer, note.Value.Range.End);
                }
            }

            //byte[] flag = NotesBrowser.getJPGFromImageControl(Properties.Resources.noteFlag);
            //for (TextPointer position = note.Value.Range.Start; position != null && position.CompareTo(note.Value.Range.End) != 1; position = position.GetNextContextPosition(LogicalDirection.Forward))
            //{
            //    InlineUIContainer element = position.Parent as InlineUIContainer;
            //    if (element != null && element.Child is System.Windows.Controls.Image)
            //    {
            //        var image = element.Child as System.Windows.Controls.Image;
            //        if (image == null) continue;
            //        var imageSourse = image.Source as System.Windows.Media.ImageSource;
            //        if (imageSourse == null) continue;
            //        byte[] byt = NotesBrowser.getJPGFromImageControl(imageSourse);
            //        //сравнивает картинки
            //        if (byt.Length == flag.Length)
            //        {
            //            bool isflag = true;
            //            for (int t = 0; t < byt.Length; t++)
            //            {
            //                if (byt[t] != flag[t]) { isflag = false; break; }
            //            }
            //            if (!isflag) continue;
            //            image.Cursor = Cursors.Hand;
            //            image.Tag = note.Key;
            //            image.MouseUp += NoteFlag_MouseUp;
            //            element.Unloaded += Element_Unloaded;
            //            //new TextRange(element.ContentStart, element.ContentEnd).Text = string.Empty;
            //        }
            //    }
            //}
        }

        internal void Element_Unloaded(object sender, RoutedEventArgs e)
        {
            var element = sender as InlineUIContainer;
            if (element.Parent != null) { return; }
            if (element != null)
            {
                var image = element.Child as System.Windows.Controls.Image;
                if (image == null || image.Tag == null || !ParentControl.NotesBrowser.Notes.ContainsKey(image.Tag.ToString())) return;
                ParentControl.NotesBrowser.Notes[image.Tag.ToString()].Range.ApplyPropertyValue(TextElement.BackgroundProperty, null);
                ParentControl.NotesBrowser.Notes.Remove(image.Tag.ToString());
                ParentControl.NotesBrowser.MainControl.Items.Refresh();
            }
        }

        public void UpdateOffsetOnNotes()
        {
            byte[] flag = NotesBrowser.getJPGFromImageControl(Properties.Resources.noteFlag);
            for (int i = ParentControl.NotesBrowser.Notes.Count - 1; i >= 0; i--)
            {
                var note = ParentControl.NotesBrowser.Notes.ElementAt(i);
                var a = SuperTextRedactor.TestRange(note.Value.Range);
                note.Value.Range.ApplyPropertyValue(TextElement.BackgroundProperty, null);
                for (TextPointer position = note.Value.Range.Start;
                    position != null && position.CompareTo(note.Value.Range.End) != 1;
                    position = position.GetNextContextPosition(LogicalDirection.Forward))
                {
                    InlineUIContainer element = position.Parent as InlineUIContainer;
                    if (element != null && element.Child is System.Windows.Controls.Image)
                    {
                        var image = element.Child as System.Windows.Controls.Image;
                        if (image == null) continue;
                        var imageSourse = image.Source as System.Windows.Media.ImageSource;
                        if (imageSourse == null) continue;
                        byte[] byt = NotesBrowser.getJPGFromImageControl(imageSourse);
                        //сравнивает картинки
                        if (byt.Length == flag.Length)
                        {
                            bool isflag = true;
                            for (int t = 0; t < byt.Length; t++)
                            {
                                if (byt[t] != flag[t])
                                {
                                    isflag = false;
                                    break;
                                }
                            }
                            if (!isflag) continue;
                            element.Unloaded -= Element_Unloaded;
                            element.SiblingInlines.Remove(element);
                            break;
                        }
                    }
                }
                note.Value.OffsetStart =
                    ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(note.Value.Range.Start);
                note.Value.OffsetEnd =
                    ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(note.Value.Range.End);
            }
            if (ParentControl.searchPanel != null)
            {
                ParentControl.searchPanel.ClearSearch();
            }
        }



        private void NoteFlag_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as System.Windows.Controls.Image;
            if (item != null)
            {
                ParentControl.ClickOnFlag(item.Tag.ToString());
            }
        }


        PropertiesForm propertForm;
        private IFileSystemControl _fileSystemControlImplementation;

        private void Propert_Click(object sender, RoutedEventArgs e)
        {
            string projName = ((System.Windows.Controls.Image)sender).Tag.ToString();
            onOpenProperty(projName, false);
        }
        private void onOpenProperty(string projName, bool isCreateFile)
        {
            propertForm = new PropertiesForm(Notes[projName]);
            propertForm.Closing -= PropertForm_Closing;
            propertForm.Closing += PropertForm_Closing;
            propertForm.CalledControl = this;
            propertForm.Init();
            ((ProjectProperties)propertForm.Element.Child).isCreateFile = isCreateFile;
            propertForm.ShowDialog();
        }

        private void PropertForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainProjectList.Items.Refresh();
        }


        private void ProjectName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab != null)
            {
                TextBox text = lab.Content as TextBox;
                if (text != null)
                {
                    BeginChangingDynamicItem(text, MainProjectList);
                    ChangedFileName = false;
                }
            }
        }
        protected override Rectangle GetCloneControlLocation(TextBox control)
        {
            var point = control.TranslatePoint(new System.Windows.Point(0, 0), MainProjectList);
            return new Rectangle((int)point.X, (int)point.Y, (int)control.ActualWidth, (int)control.ActualHeight);
        }

        protected override void AddDynamicControls()
        {
            BrowseContainer.Children.Add(CloneTextBox);
            BrowseContainer.Children.Add(DisableBorder);
            CloneTextBox.Focus();
        }

        protected override void RemoveDynamicControls()
        {
            BrowseContainer.Children.Remove(CloneTextBox);
            BrowseContainer.Children.Remove(DisableBorder);
        }

        protected override void CloneTextBox_LostFocus(object sender, EventArgs e)
        {
            base.CloneTextBox_LostFocus(sender, e);
            var mouseEventArgs = e as MouseEventExtArgs;
            if (!IsValid)
            {
                if (mouseEventArgs != null)
                {
                    mouseEventArgs.Handled = true;
                }
                return;
            }
            if (e != null && mouseEventArgs == null) { return; }
            if (e != null)
            {
                System.Windows.Point absolutePoint =
                    CloneTextBox.PointToScreen(new System.Windows.Point(0d, 0d));
                var absoluteRectangle = new Rectangle((int)absolutePoint.X, (int)absolutePoint.Y, CloneTextBoxLocation.Width, CloneTextBoxLocation.Height);
                if (absoluteRectangle.Contains(mouseEventArgs.X, mouseEventArgs.Y)) { return; }
            }
            if (CloneTextBox.Tag.ToString() != CloneTextBox.Text)
            {
                if (!ChangedFileName)
                {
                    RenameProject(CloneTextBox.Tag.ToString(), CloneTextBox.Text);
                }
                else
                {
                    LoadedFile file = null;
                    foreach (var proj in Notes)
                    {
                        foreach (var f in proj.Value.Files)
                        {
                            if (f.Path == CloneTextBox.Tag.ToString())
                            {
                                file = f;
                                break;
                            }
                        }
                    }
                    if (file == null) { return; }
                    string directoryPath = Path.GetDirectoryName(file.Path) + "\\";
                    RenameFileInProject(Directory.GetParent(directoryPath).Parent.Name, file, CloneTextBox.Text);
                    IsChangeFileName = false;
                }
            }
            // MainProjectList.Items.Refresh();
            EndChangingDynamicItem();
        }

        public override void Callback(bool rezult, string message, EventArgs args)
        {
            MainProjectList.Items.Refresh();
        }

        private void EditFileName_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var text = sender as Border;
            if (text == null) return;
            var panel = text.Parent as Panel;
            if (panel == null) return;
            var lab = panel.Children[1] as Label;
            if (lab == null) return;
            var textbox = lab.Content as TextBox;
            if (textbox != null)
            {
                BeginChangingDynamicItem(textbox, MainProjectList);
                ChangedFileName = true;
                IsChangeFileName = true;
            }
        }

        public override void Show()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = ParentControl.RedactorConteiner.Margin;
            myThicknessAnimation.To = new Thickness(0, ParentControl.RedactorConteiner.Margin.Top, ParentControl.RedactorConteiner.Margin.Right, ParentControl.RedactorConteiner.Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            ((Border)ParentControl.RedactorConteiner).BeginAnimation(Border.MarginProperty, myThicknessAnimation);
            ParentControl.ShowProject.Visibility = Visibility.Hidden;
        }

        public override void Hide()
        {
            ThicknessAnimation myThicknessAnimation = new ThicknessAnimation();
            myThicknessAnimation.From = ParentControl.RedactorConteiner.Margin;
            myThicknessAnimation.To = new Thickness(-ActualWidth, ParentControl.RedactorConteiner.Margin.Top, ParentControl.RedactorConteiner.Margin.Right, ParentControl.RedactorConteiner.Margin.Bottom);
            myThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            myThicknessAnimation.Completed += new EventHandler((s, r) => ParentControl.ShowProject.Visibility = Visibility.Visible);
            ((Border)ParentControl.RedactorConteiner).BeginAnimation(Border.MarginProperty, myThicknessAnimation);
        }

        public void SetLineSpacing()
        {
            foreach (var block in ParentControl.TextBox.MainControl.Document.Blocks)
            {
                var p = block as Paragraph;
                if (p == null) continue;
                p.LineHeight = p.FontSize + ParentControl.defaultSpacing;
            }
        }


    }
}
