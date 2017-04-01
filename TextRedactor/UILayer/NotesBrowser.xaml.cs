using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ApplicationLayer;
using Rectangle = System.Drawing.Rectangle;
using System;

namespace UILayer
{
    /// <summary>
    /// Логика взаимодействия для NotesBrowser.xaml
    /// </summary>
    public partial class NotesBrowser : BasicPanel<Note>
    {
        private BinaryFormatter serializer = new BinaryFormatter();
        public NotesBrowser()
        {
            InitializeComponent();
        }
        protected override object OnSave(Action action, string Name)
        {
            CloseNotes(Name);
            return Notes;
        }
        internal void CloseNotes(string path)
        {
            using (var stream = File.OpenWrite(path))
            {
                serializer.Serialize(stream, Notes);
            }
        }

        internal override string GenerateName(string name, string path = "", bool isProg = false)
        {
            string generattingName = name;
            int i = 0;
            while (true)
            {
                if (!ItemsCollection.ContainsKey(generattingName)) { return generattingName; }
                i++;
                generattingName = name + i;

            }
        }

        internal void LoadNotes(string path)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            if (!File.Exists(path))
            {
                Notes.Clear();
                return;
            }
            using (var stream = File.OpenRead(path))
            {
                RefreshNotes(serializer.Deserialize(stream) as Dictionary<string, Note>);
            }
            MainControl.Items.Refresh();
        }

        public void RefreshNotes(Dictionary<string, Note> collection)
        {
            Notes.Clear();
            //for (int i=0;i<Notes.Count;i++)
            //{
            //    RemoveItem(Notes.ElementAt(i).Value.Name);
            //    i--;
            //}

            foreach (var note in collection)
            {
                AddItem(note.Value);
            }
            MainControl.Items.Refresh();
        }
        public Image NotePicture { get; set; }
        private void DelNote_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var im = sender as System.Windows.Controls.Image;
            if (im != null)
            {
                Notes.Remove(im.Tag.ToString());
                //  ParentControl.BrowseProject.UpdateNoteAfterOpening();
                //   ParentControl.BrowseProject.DelFlag();
                // ParentControl.BrowseProject.OpenFile(ParentControl.BrowseProject.CurentFile, Path.GetFileNameWithoutExtension(ParentControl.BrowseProject.CurentFile));
                //  MainControl.Items.Refresh();
            }
        }

        protected override void AddDynamicControls()
        {
            NotesContainer.Children.Add(CloneTextBox);
            NotesContainer.PreviewMouseDown -= CloneTextBox_LostFocus;
            NotesContainer.PreviewMouseDown += CloneTextBox_LostFocus;
            MainControl.IsEnabled = false;
        }

        protected override void RemoveDynamicControls()
        {
            NotesContainer.PreviewMouseDown -= CloneTextBox_LostFocus;
            NotesContainer.Children.Remove(CloneTextBox);
            MainControl.IsEnabled = true;
        }

        protected override void CloneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            base.CloneTextBox_LostFocus(sender, e);
            if (!IsValid) { return; }
            var mouseEventArgs = e as MouseEventArgs;
            if (e != null && mouseEventArgs == null) { return; }
            if (e != null)
            {
                var clickPosition = mouseEventArgs.GetPosition(MainControl);
                if (CloneTextBoxLocation.Contains((int)clickPosition.X, (int)clickPosition.Y)) { return; }
            }
            if (CloneTextBox.Tag.ToString() != CloneTextBox.Text)
            {
                string name = CloneTextBox.Tag.ToString();
                //File.Move(file, Path.GetDirectoryName(file) + "\\" + CloneTextBox.Text + Path.GetExtension(file));
                Notes.Add(CloneTextBox.Text, Notes[name]);
                Notes[CloneTextBox.Text].Name = CloneTextBox.Text;
                Notes.Remove(name);
            }
            MainControl.Items.Refresh();
            EndChangingDynamicItem();
        }

        protected override System.Drawing.Rectangle GetCloneControlLocation(TextBox control)
        {
            var point = control.TranslatePoint(new System.Windows.Point(0, 0), MainControl);
            return new Rectangle((int)point.X, (int)point.Y, (int)control.ActualWidth, (int)control.ActualHeight);
        }

        protected override void OnValidate()
        {
            if (!Notes.ContainsKey(CloneTextBox.Tag.ToString()) || Notes.ContainsKey(CloneTextBox.Text) || !File.Exists(ParentControl.BrowseProject.LoadedFile))
            {
                IsValid = false;
                if (Binding == null) { Binding = CloneTextBox.GetBindingExpression(TextBox.TextProperty); }
                Validation.MarkInvalid(Binding, new ValidationError(new ExceptionValidationRule(), Binding));
                if (ErrorToolTip == null)
                {
                    ErrorToolTip = new ToolTip { Content = TextRedactor.PathErrorMessage };
                    CloneTextBox.ToolTip = ErrorToolTip;
                }
                ErrorToolTip.IsOpen = true;
            }
            else
            {
                IsValid = true;
            }
        }

        private void EditNoteName_MouseDown(object sender, RoutedEventArgs e)
        {
            var text = sender as Border;
            if (text == null) return;
            var panel = text.Parent as Panel;
            if (panel == null) return;
            var t = panel.Children[0] as TextBox;
            if (t != null)
            {
                BeginChangingDynamicItem(t);
            }
        }

        private void MainControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainControl.SelectedIndex > -1)
            {
                var par = (KeyValuePair<string, Note>)MainControl.SelectedValue;
                if (!string.IsNullOrEmpty(par.Key))
                {
                    ParentControl.TextBox.MainControl.Focus();
                    ParentControl.TextBox.MainControl.CaretPosition = par.Value.Range.Start;// ParentControl.TextBox.MainControl.GetTextPointAt(ParentControl.TextBox.MainControl.Document.ContentStart, par.Value.OffsetStart, System.Windows.Documents.LogicalDirection.Forward);
                }
            }
        }
    }
}
