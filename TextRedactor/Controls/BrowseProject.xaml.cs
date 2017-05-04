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
        private bool IsChangeFileName = false;
        private bool ChangedFileName;

        public BrowseProject() : base()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            if (string.IsNullOrEmpty(LoadedFile)) { return; }
            ParentControl.BrowseProject.UpdateOffsetOnNotes();
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

        internal void DelFlag(Note note)
        {
            //  UpdateNoteAfterOpening();
        }
        public void OpenFirstFile()
        {
            if (Notes.Count > 0 && Notes.First().Value.Files.Count > 0)
            {
                if (!ParentControl.TextBox.MainControl.IsEnabled)
                {
                    ParentControl.TextBox.MainControl.IsEnabled = true;
                    ParentControl.Format.IsEnabled = true;
                }
                OpenFile(Notes.First().Value.Files[0].Path, Path.GetFileNameWithoutExtension(Notes.First().Value.Files[0].Path));
                if (!string.IsNullOrEmpty(LoadedFile))
                {
                    UpdateRangeOnNotes();
                    // UpdateTagOnFlags();
                }
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
            AddNewFileToProject(projName, GenerateName("Chapter", ProjectsPath + "\\" + projName + "\\Files", false));
            MainProjectList.Items.Refresh();
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
            ((ISettings)ParentControl.Parent).SaveSettings();
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
            if (!string.IsNullOrEmpty(LoadedFile) && LoadedFile.StartsWith(ProjectsPath + "\\" + project)) { LoadedFile = ""; }
            if (!string.IsNullOrEmpty(CurentFile) && CurentFile.StartsWith(ProjectsPath + "\\" + project))
            {
                // CurentProject = No
                CurentFile = "";
            }

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
            ((ISettings)ParentControl.Parent).SaveSettings();
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
            if (!string.IsNullOrEmpty(LoadedFile))
            {
                UpdateOffsetOnNotes();
            }
            foreach (var item in Notes.Values)
            {
                item.Files.ForEach(x => x.IsOpen = (x.Path == path) ? true : false);
            }
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

            }
            ParentControl.TextBox.MainControl.FilePath = path;
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
                ParentControl.Format.IsEnabled = true;
            }
            var textBlock = (System.Windows.Controls.Label)sender;
            if (CurentFile == textBlock.Tag.ToString()) { return; }
            OpenFile(textBlock.Tag.ToString(), Path.GetFileNameWithoutExtension(textBlock.Tag.ToString()));
            if (!string.IsNullOrEmpty(LoadedFile))
            {
                UpdateRangeOnNotes();
                // UpdateTagOnFlags();
            }
        }

        public void UpdateRangeOnNotes()
        {
            foreach (var note in ParentControl.NotesBrowser.Notes)
            {
                note.Value.Range = new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart.GetPositionAtOffset(note.Value.OffsetStart),
                    ParentControl.TextBox.MainControl.Document.ContentStart.GetPositionAtOffset(note.Value.OffsetEnd));
            }
        }
        private void UpdateOffsetOnNotes()
        {
            foreach (var note in ParentControl.NotesBrowser.Notes)
            {
                note.Value.OffsetStart = ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(note.Value.Range.Start);
                note.Value.OffsetEnd = ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(note.Value.Range.End);
            }
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



        //public static TextPointer GetTextPointAt(TextPointer from, int pos)
        //{
        //    TextPointer ret = from;
        //    int i = 0;

        //    while ((i < pos) && (ret != null))
        //    {
        //        if ((ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text) || (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None))
        //        {
        //            i++;
        //        }

        //        if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
        //            return ret;
        //        ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);

        //    }

        //    return ret;
        //}
        //private void AddNoteAfterOpenFile()
        //{
        //    foreach (var note in ParentControl.NotesBrowser.Notes)
        //    {
        //        int start = note.Value.OffsetStart;
        //        int end = note.Value.OffsetEnd;
        //        int dif = 0;
        //        foreach (var not in ParentControl.NotesBrowser.Notes)
        //        {
        //            if (not.Key == note.Key) { break; }
        //            if (not.Value.OffsetStart < start) dif++;
        //        }
        //        start += dif;
        //        end += dif;
        //        TextPointer startPointer = ParentControl.TextBox.MainControl.Document.ContentStart;
        //        TextPointer startPos = GetTextPointAt(startPointer, start);
        //        TextPointer endPos = GetTextPointAt(startPointer, end);
        //        new TextRange(startPos, endPos).ApplyPropertyValue(TextElement.BackgroundProperty, System.Windows.Media.Brushes.PaleGreen);
        //        var tempImage = Properties.Resources.noteFlag;
        //        var ScreenCapture = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //  tempImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(20, 20));

        //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
        //        image.Source = ScreenCapture;
        //        image.Stretch = Stretch.Fill;
        //        image.Cursor = Cursors.Hand;
        //        image.Height = 14;
        //        image.Width = 14;
        //        image.Tag = note.Key;
        //        image.MouseUp += NoteFlag_MouseUp;
        //        TextPointer p = startPos;
        //        ParentControl.TextBox.MainControl.BeginChange();
        //        InlineUIContainer imageContainer = new InlineUIContainer(image, p);
        //        ParentControl.TextBox.MainControl.EndChange();
        //        ParentControl.TextBox.MainControl.Focus();
        //    }
        //}

        private void NoteFlag_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as System.Windows.Controls.Image;
            if (item != null)
            {
                ParentControl.ClickOnFlag(item.Tag.ToString());
            }
        }

        internal void UpdateNoteAfterOpening()
        {
            var inlines = ParentControl.TextBox.MainControl.Document.Blocks.Where(item => item.GetType() == typeof(Paragraph)).
               SelectMany(item => ((Paragraph)item).Inlines.Where(x => x.GetType() == typeof(InlineUIContainer) || (x.Background != null && (x.Background as SolidColorBrush).Color == System.Windows.Media.Brushes.PaleGreen.Color))).ToList();
            //  byte[] flag = getJPGFromImageControl(Properties.Resources.noteFlag);
            int n = inlines.Count;
            for (int j = 0; j < n; j++)
            {
                var inline = inlines[j] as InlineUIContainer;
                if (inline == null) { continue; }
                var image = inline.Child as System.Windows.Controls.Image;
                if (image == null) { continue; }

                if (image.Tag == null) continue;

                var key = image.Tag.ToString();
                int i = j;
                int start = ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(inlines[j].ElementEnd) + 1;
                //new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, inlines[j].ContentStart).Text.Length;
                while (j < inlines.Count - 1 && inlines[j + 1].Background != null && (inlines[j + 1].Background as SolidColorBrush).Color == System.Windows.Media.Brushes.PaleGreen.Color)
                {
                    j++;
                }
                if (ParentControl.NotesBrowser.Notes.ContainsKey(key))
                {
                    if (ParentControl.NotesBrowser.Notes[key].OffsetStart != start)
                    {
                        ParentControl.NotesBrowser.Notes[key].OffsetStart = start;
                    }
                    ParentControl.NotesBrowser.Notes[key].OffsetEnd = ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(inlines[j].ContentEnd);// new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, inlines[j].ContentEnd).Text.Length;
                }
                else
                {
                    new TextRange(inlines[i].ContentStart, inlines[j].ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, System.Windows.Media.Brushes.White);
                    foreach (var block in ParentControl.TextBox.MainControl.Document.Blocks)
                    {
                        if (block is Paragraph)
                        {
                            var paragraph = block as Paragraph;

                            if (paragraph.Inlines.Contains(inline))
                            {
                                paragraph.Inlines.Remove(inline);
                                break;
                            }
                        }
                    }
                }
                // }
            }
        }

        public void UpdateTagOnFlags()
        {
            var inlines = ParentControl.TextBox.MainControl.Document.Blocks.Where(item => item.GetType() == typeof(Paragraph)).
               SelectMany(item => ((Paragraph)item).Inlines.Where(x => x.GetType() == typeof(InlineUIContainer))).ToList();
            byte[] flag = getJPGFromImageControl(Properties.Resources.noteFlag);
            int n = inlines.Count;
            foreach (var item in inlines)
            {
                var image = ((InlineUIContainer)item).Child as System.Windows.Controls.Image;
                if (image == null) { continue; }
                byte[] byt = getJPGFromImageControl(image.Source as BitmapImage);
                //сравнивает картинки
                if (byt.Length == flag.Length)
                {
                    bool isflag = true;
                    for (int t = 0; t < byt.Length; t++)
                    {
                        if (byt[t] != flag[t]) { isflag = false; break; }
                    }
                    if (!isflag) continue;
                    int start = ParentControl.TextBox.MainControl.Document.ContentStart.GetOffsetToPosition(item.ElementEnd) + 1;
                    //  new TextRange(ParentControl.TextBox.MainControl.Document.ContentStart, item.ContentStart).Text.Length;
                    var key = ParentControl.NotesBrowser.Notes.Where(x => x.Value.OffsetStart == start);
                    if (key.Any())
                    {
                        image.Tag = key.First().Key;
                    }
                    else
                    {

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


        }

        private void buttonOpenFile_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab != null)
            {
                TextBox t = lab.Content as TextBox;
                if (t != null)
                {
                    BeginChangingDynamicItem(t);
                    ChangedFileName = true;
                    IsChangeFileName = true;
                }
            }
        }

        private void TextNameFile_OnKeyDown(object sender, KeyEventArgs e)
        {

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
            CloneTextBox.Focus();
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
            if (e != null && mouseEventArgs == null) { return; }
            if (e != null)
            {
                var clickPosition = mouseEventArgs.GetPosition(MainProjectList);
                if (CloneTextBoxLocation.Contains((int)clickPosition.X, (int)clickPosition.Y)) { return; }
            }
            if (CloneTextBox.Tag.ToString() != CloneTextBox.Text)
            {
                if (!ChangedFileName)
                {
                    RenameProject(CloneTextBox.Tag.ToString(), CloneTextBox.Text);
                }
                else
                {
                    LoadedFile file = Notes[CurentProject.Name].Files.Where(item => item.Path == CloneTextBox.Tag.ToString()).FirstOrDefault();
                    if (file == null) { return; }
                    string directoryPath = Path.GetDirectoryName(file.Path) + "\\";
                    RenameFileInProject(Directory.GetParent(directoryPath).Parent.Name, file, CloneTextBox.Text);
                    IsChangeFileName = false;
                }
            }
            MainProjectList.Items.Refresh();
            EndChangingDynamicItem();
        }
    }
}
