using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Net.Sgoliver.NRtfTree.Util;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media;

namespace Controls
{
    /// <summary>
    /// Логика взаимодействия для BrowseProject.xaml
    /// </summary>
    public partial class BrowseProject : BasicPanel<Project>, IDisposable
    {
        internal string LoadedFile { get; private set; }
        public Project CurentProject;
        public string CurentFile;
        bool IsChangeFileName = false;
        public BrowseProject() : base()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            if (string.IsNullOrEmpty(LoadedFile)) { return; }
            ParentControl.NotesBrowser.CloseNotes(LoadedFile);
        }
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

        public string ProjectsPath
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
            }
        }

        private string vProjectPath = "";

        public BrowseProject(Dictionary<string, Project> collection)
        {
            InitializeComponent();
            ItemsCollection = collection;
        }

        private void OnFileOpen(string projectPath, string file)
        {
            if (!string.IsNullOrEmpty(LoadedFile)) { ParentControl.NotesBrowser.CloseNotes(LoadedFile); }
            ParentControl.NotesBrowser.LoadNotes(projectPath + System.IO.Path.GetFileNameWithoutExtension(file) + ".not");
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
            onOpenProperty(projName, true);
        }

        private void CreateProjectFile(Project project, string path)
        {
            using (FileStream fs = File.Create(path))
            {
                BinaryFormatter write = new BinaryFormatter();
                write.Serialize(fs, project);
            }
        }

        internal override string GenerateName(string name)
        {
            List<DirectoryInfo> directories = new DirectoryInfo(ProjectsPath).GetDirectories().ToList();
            string generattingName = name;
            int i = 0;
            while (true)
            {
                if (directories.FindIndex(dir => dir.Name == generattingName) < 0) { return generattingName; }
                i++;
                generattingName = name + i;

            }
        }
        public void CreateProject()
        {
            if (!Directory.Exists(ProjectsPath))
            {
                Directory.CreateDirectory(ProjectsPath);
            }

            string name = GenerateName("NewProject");
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
            string path = ProjectsPath + "\\" + project + "\\Files\\" + file + ".rtf";
            if (File.Exists(path)) { return; }
            RtfDocument doc = new RtfDocument();
            doc.Save(path);
            Notes[project].Files.Add(new LoadedFile(path, ProjectsPath + "\\" + project));
        }

        public void RenameProject(string project, string newName)
        {
            if (!Notes.ContainsKey(project)) { return; }
            if (System.IO.Directory.Exists(ProjectsPath + "\\" + newName))
            {
                MessageBox.Show("Project with the same name already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Directory.Move(ProjectsPath + "\\" + project, ProjectsPath + "\\" + newName);
            var newProject = new Project(newName, Notes[project].CreateDate);
            foreach (var file in Notes[project].ListFiles)
            {
                newProject.Files.Add(new LoadedFile(ProjectsPath + "\\" + newName + "\\Files\\" + System.IO.Path.GetFileName(file.Path), ProjectsPath + "\\" + newName));
            }
            newProject.Author = Notes[project].Author;
            newProject.PublishingDate = Notes[project].PublishingDate;
            Notes.Remove(project);
            Notes.Add(newName, newProject);
            File.Delete(ProjectsPath + "\\" + newName + "\\" + project + ".prj");
            CreateProjectFile(Notes[newName], ProjectsPath + "\\" + newName + "\\" + newName + ".prj");
            if (!String.IsNullOrEmpty(LoadedFile))
            {
                string lastFile = System.IO.Path.GetFileName(LoadedFile);
                LoadedFile = ProjectsPath + "\\" + newName + "\\Files\\" + lastFile;
            }
            if (propertForm == null) { return; }
            propertForm.value.Name = newName;
            ((Project)propertForm.value).Files = newProject.Files;
        }

        protected override void OnSave(string project)
        {
            if (!Notes.ContainsKey(project)) { return; }
            CreateProjectFile(Notes[project], ProjectsPath + "\\" + project + "\\" + project + ".prj");
        }

        public void DeleteFile(string project, string filePath)
        {
            int index = Notes[project].Files.FindIndex(item => item.Path == filePath);
            if (index < 0) { return; }
            var file = Notes[project].Files[index];
            if (LoadedFile == System.IO.Path.GetDirectoryName(file.Path) + "\\" + file.Name + ".not") { LoadedFile = ""; }
            File.Delete(file.Path);
            Notes[project].Files.RemoveAt(index);
        }

        public void DeleteProject(string project)
        {
            if (!Notes.ContainsKey(project)) { return; }
            if (LoadedFile.StartsWith(ProjectsPath + "\\" + project)) { LoadedFile = ""; }
            Directory.Delete(ProjectsPath + "\\" + project, true);
            Notes.Remove(project);
        }

        public void RenameFileInProject(string project, LoadedFile file, string newFile)
        {
            if (System.IO.Path.GetFileNameWithoutExtension(LoadedFile) == file.Name) LoadedFile = System.IO.Path.GetDirectoryName(LoadedFile) + "\\" + newFile + ".not";
            int index = Notes[project].Files.FindIndex(item => item.Path == file.Path);
            if (index < 0) { return; }
            newFile = newFile + System.IO.Path.GetExtension(file.Path);
            File.Move(file.Path, System.IO.Path.GetDirectoryName(file.Path) + "\\" + newFile);
            if (System.IO.File.Exists(ProjectsPath + "\\" + project + "\\Files\\" + System.IO.Path.GetFileNameWithoutExtension(file.Name) + ".not"))
            {
                File.Move(ProjectsPath + "\\" + project + "\\Files\\" + System.IO.Path.GetFileNameWithoutExtension(file.Name) + ".not", ProjectsPath + "\\" + project + "\\Files\\" + System.IO.Path.GetFileNameWithoutExtension(newFile) + ".not");
                if (LoadedFile == ProjectsPath + "\\" + project + "\\Files\\" + System.IO.Path.GetFileNameWithoutExtension(file.Name) + ".not") { LoadedFile = ProjectsPath + "\\" + project + "\\Files\\" + System.IO.Path.GetFileNameWithoutExtension(newFile) + ".not"; }
            }
            Notes[project].Files[index] = new LoadedFile(System.IO.Path.GetDirectoryName(file.Path) + "\\" + newFile, ProjectsPath + "\\" + project);
            OnSave(project);
        }

        public void AddFileToProject(string project, string filePath)
        {
            if (!Notes.ContainsKey(project)) { return; }
            File.Copy(filePath, ProjectsPath + "\\" + project + "\\Files\\" + System.IO.Path.GetFileName(filePath));
            Notes[project].Files.Add(new LoadedFile(filePath, ProjectsPath + "\\" + project));
        }

        private void ButAddProject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CreateProject();
        }

        internal void OpenFile(string path, string content)
        {
           
            ParentControl.TextBox.MainControl.FilePath = "";
            var range = new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, ParentControl.TextBox.MainControl.Document.ContentEnd);
            using (var fStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                try
                {
                    range.Load(fStream, System.Windows.DataFormats.Rtf);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                    return;
                }
                string folder = Path.GetDirectoryName(path);
                OnFileOpen(folder + "\\", content);
                LoadedFile = folder + "\\" + content + ".not";
                if (!String.IsNullOrEmpty(LoadedFile))
                {
                    UpdateNoteBeforeClosing();
                }
                //UpdateNoteInFile();
                //AddNoteAfterOpenFile();

            }
            //  DirectoryInfo dir = new DirectoryInfo(System.IO.Path.GetDirectoryName(path) + "\\");
            ParentControl.TextBox.MainControl.FilePath = path;
            //Notes[dir.Parent.Name].Files.Find(item => item.Name == content).Path;
        }
        private void buttonOpenFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsChangeFileName)
            {
                IsChangeFileName = false;
                return;
            }
            if (!ParentControl.TextBox.MainControl.IsEnabled)
            {
                ParentControl.TextBox.MainControl.IsEnabled = true;
            }
            var textBlock = (System.Windows.Controls.Label)sender;
            if (CurentFile == textBlock.Tag.ToString()) { return; }
            OpenFile(textBlock.Tag.ToString(), Path.GetFileNameWithoutExtension(textBlock.Tag.ToString()));

        }
        public byte[] getJPGFromImageControl(BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            byte[] b = memStream.ToArray();
            memStream.Close();
            return b;
        }
        public byte[] getJPGFromImageControl(Bitmap tempImage)
        {
            byte[] flag;
            BitmapSource ScreenCapture = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            tempImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(20, 20));
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(ScreenCapture));
                encoder.Save(stream);
                flag = stream.ToArray();
                stream.Close();
            }
            return flag;
        }
        private void UpdateNoteInFile()
        {
            ParentControl.NotesBrowser.Notes.Clear();
           Type inlineType;
            InlineUIContainer uic;
            System.Windows.Controls.Image replacementImage;
            new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, ParentControl.TextBox.MainControl.Document.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, System.Windows.Media.Brushes.White);

            byte[] flag = getJPGFromImageControl(Properties.Resources.noteFlag);

            var blocks = ParentControl.TextBox.MainControl.Document.Blocks.ToList();
            for (int b = 0; b < blocks.Count; b++)
            {
                Block block = blocks[b];
                var inlines = ((Paragraph)block).Inlines.ToList();
                for (int j = 0; j < inlines.Count; j++)
                {
                    if (inlines[j].GetType() == typeof(InlineUIContainer))
                    {
                        uic = ((InlineUIContainer)inlines[j]);
                        if (uic.Child.GetType() == typeof(System.Windows.Controls.Image))
                        {
                            replacementImage = (System.Windows.Controls.Image)uic.Child;
                            byte[] byt = getJPGFromImageControl(replacementImage.Source as BitmapImage);
                            //сравнивает картинки
                            if (byt.Length == flag.Length)
                            {
                                for (int t = 0; t < byt.Length; t++)
                                {
                                    if (byt[t] != flag[t]) { return; }
                                }
                                 ((Paragraph)ParentControl.TextBox.MainControl.Document.Blocks.ToList()[b]).Inlines.Remove(inlines[j]);
                            }
                        }
                    }
                    //else
                    //{
                    //  //  new TextRange(i.ContentStart, i.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty,  System.Windows.Media.Brushes.White);
                    //}
                }
            }

        }
        public static TextPointer GetTextPointAt(TextPointer from, int pos)
        {
            TextPointer ret = from;
            int i = 0;

            while ((i < pos) && (ret != null))
            {
                if ((ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text) || (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None))
                {
                    i++;
                }

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;
                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);

            }

            return ret;
        }
        private void AddNoteAfterOpenFile()
        {
            foreach (var note in ParentControl.NotesBrowser.Notes)
            {
                int start = note.Value.OffsetStart;
                int end = note.Value.OffsetEnd;
                int dif = 0;
                foreach (var not in ParentControl.NotesBrowser.Notes)
                {
                    if (not.Key == note.Key) { break; }
                    if (not.Value.OffsetStart < start) dif++;
                }
                start += dif;
                end += dif;
                TextPointer startPointer = ParentControl.TextBox.MainControl.Document.ContentStart;
                TextPointer startPos = GetTextPointAt(startPointer, start);
                TextPointer endPos = GetTextPointAt(startPointer, end);
                new TextRange(startPos, endPos).ApplyPropertyValue(TextElement.BackgroundProperty, System.Windows.Media.Brushes.PaleGreen);
                var tempImage = Properties.Resources.noteFlag;
                var ScreenCapture = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
          tempImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(20, 20));

                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = ScreenCapture;
                image.Stretch = Stretch.Fill;
                image.Cursor = Cursors.Hand;
                image.Height = 14;
                image.Width = 14;
                image.Tag = note.Key;
                image.MouseUp += NoteFlag_MouseUp;
                TextPointer p = startPos;
                ParentControl.TextBox.MainControl.BeginChange();
                InlineUIContainer imageContainer = new InlineUIContainer(image, p);
                ParentControl.TextBox.MainControl.EndChange();
                ParentControl.TextBox.MainControl.Focus();
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

        private void UpdateNoteBeforeClosing()
        {
            System.Windows.Controls.Image replacementImage;
            var blocks = ParentControl.TextBox.MainControl.Document.Blocks.ToList();
            for (int b = 0; b < blocks.Count; b++)
            {
                var paragraph = blocks[b] as Paragraph;
                if (paragraph == null) { continue; }
                var inlines = paragraph.Inlines.OfType<InlineUIContainer>().ToList();
                for (int j = 0; j < inlines.Count; j++)
                {
                    if (inlines[j] == null) { continue; }
                    var image = inlines[j].Child as System.Windows.Controls.Image;
                    if (image == null) { continue; }
                    replacementImage = image;
                    if (replacementImage.Tag == null) { continue; }
                    string key = replacementImage.Tag.ToString();
                    if (ParentControl.NotesBrowser.Notes.ContainsKey(key))
                    {
                        int start = new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, inlines[j].ContentStart).Text.Length;
                        if (ParentControl.NotesBrowser.Notes[key].OffsetStart != start)
                        {
                            ParentControl.NotesBrowser.Notes[key].OffsetStart = start;
                        }

                        if (j == inlines.Count - 1)
                        {
                            ParentControl.NotesBrowser.Notes[key].OffsetEnd = new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, inlines[j].ContentEnd).Text.Length;
                        }
                        else
                        {
                            int i = j + 1;
                            while (i < inlines.Count && inlines[i].Background == System.Windows.Media.Brushes.PaleGreen)
                            {
                                i++;
                            }
                            ParentControl.NotesBrowser.Notes[key].OffsetEnd = new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, inlines[i > 0 ? i-- : i].ContentEnd).Text.Length;
                        }
                    }
                }
            }
        }

        PropertiesForm propertForm;
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

        private void MainProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainProjectList.SelectedItem != null)
            {
                CurentProject = ((KeyValuePair<string, Project>)MainProjectList.SelectedItem).Value;
            }
        }

        private void FileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListBox;
            if (list == null || list.ItemsSource == null) return;
            CurentFile = ((LoadedFile)list.SelectedItem).Path;
            Project project = Notes.Where(x => x.Value.ListFiles == (List<LoadedFile>)list.ItemsSource).FirstOrDefault().Value;
            if (project == null) { return; }
            CurentProject = project;
        }

        private void TextNameFile_LostFocus(object sender, RoutedEventArgs e)
        {

            IsChangeFileName = false;
            TextBox t = sender as TextBox;
            if (t != null)
            {
                LoadedFile file = Notes[CurentProject.Name].Files.Where(item => item.Path == t.Tag.ToString()).FirstOrDefault();
                if (file == null) { return; }
                string directoryPath = Path.GetDirectoryName(file.Path) + "\\";
                RenameFileInProject(Directory.GetParent(directoryPath).Parent.Name, file, t.Text);
                t.IsEnabled = false;
                MainProjectList.Items.Refresh();
            }
        }

        private void buttonOpenFile_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab != null)
            {
                TextBox t = lab.Content as TextBox;
                if (t != null)
                {
                    IsChangeFileName = true;
                }
            }
        }

        private void TextNameFile_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ParentControl.TextBox.MainControl.Focus();
            }
        }

        private void ProjectName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab != null)
            {
                TextBox text = lab.Content as TextBox;
                if (text != null)
                {
                    BeginChangingDynamicItem(text);
                }
            }
        }

        protected override void AddDynamicControls()
        {
            BrowseContainer.Children.Add(CloneTextBox);
            BrowseContainer.PreviewMouseDown -= CloneTextBox_LostFocus;
            BrowseContainer.PreviewMouseDown += CloneTextBox_LostFocus;
            MainProjectList.IsEnabled = false;
        }

        protected override void RemoveDynamicControls()
        {
            BrowseContainer.PreviewMouseDown -= CloneTextBox_LostFocus;
            BrowseContainer.Children.Remove(CloneTextBox);
            MainProjectList.IsEnabled = true;
        }

        protected override void CloneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            base.CloneTextBox_LostFocus(sender, e);
            if (!IsValid) { return; }
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs == null) { return; }
            var positionCLick = mouseEventArgs.GetPosition(Application.Current.MainWindow);
            if (!CloneTextBoxLocation.Contains((int)positionCLick.X, (int)positionCLick.Y))
            {
                if (CloneTextBox.Tag.ToString() != CloneTextBox.Text)
                {
                    RenameProject(CloneTextBox.Tag.ToString(), CloneTextBox.Text);
                }
                MainProjectList.Items.Refresh();
                EndChangingDynamicItem();
            }
        }
    }
}
