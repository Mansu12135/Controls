using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using Rectangle = System.Drawing.Rectangle;
using System;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Drawing;
using Gma.UserActivityMonitor;
using System.Windows.Input;

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
        public static byte[] getJPGFromImageControl(System.Windows.Media.ImageSource imageC)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            byte[] bytes = null;
            var bitmapSource = imageC as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }
        public static byte[] getJPGFromImageControl(Bitmap tempImage)
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
        public System.Windows.Controls.Image NotePicture { get; set; }
        private void DelNote_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var im = sender as System.Windows.Controls.Image;
            if (im == null) return;
            var key = im.Tag.ToString();
            if (key == null) return;
            byte[] flag = getJPGFromImageControl(Properties.Resources.noteFlag);
            Notes[key].Range.ApplyPropertyValue(TextElement.BackgroundProperty, System.Windows.Media.Brushes.White);
            for (TextPointer position = Notes[key].Range.Start; position != null && position.CompareTo(Notes[key].Range.End) != 1; position = position.GetNextContextPosition(LogicalDirection.Forward))
            {
                InlineUIContainer element = position.Parent as InlineUIContainer;
                if (element != null && element.Child is System.Windows.Controls.Image)
                {
                    var image = element.Child as System.Windows.Controls.Image;
                    if (image == null) continue;
                    var imageSourse = image.Source as System.Windows.Media.ImageSource;
                    if (imageSourse == null) continue;
                    byte[] byt = getJPGFromImageControl(imageSourse);
                    //сравнивает картинки
                    if (byt.Length == flag.Length)
                    {
                        bool isflag = true;
                        for (int t = 0; t < byt.Length; t++)
                        {
                            if (byt[t] != flag[t]) { isflag = false; break; }
                        }
                        if (!isflag) continue;
                        element.SiblingInlines.Remove(element);
                        //new TextRange(element.ContentStart, element.ContentEnd).Text = string.Empty;
                    }
                }
            }
            Notes.Remove(im.Tag.ToString());
            MainControl.Items.Refresh();
        }

        protected override void AddDynamicControls()
        {
            NotesContainer.Children.Add(CloneTextBox);
            CloneTextBox.Focus();
            MainControl.IsEnabled = false;
        }

        protected override void RemoveDynamicControls()
        {
            NotesContainer.Children.Remove(CloneTextBox);
            MainControl.IsEnabled = true;
        }

        protected override void CloneTextBox_LostFocus(object sender, EventArgs e)
        {
            base.CloneTextBox_LostFocus(sender, e);
            if (!IsValid) { return; }
            var mouseEventArgs = e as MouseEventExtArgs;
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
                string name = CloneTextBox.Tag.ToString();
                //File.Move(file, Path.GetDirectoryName(file) + "\\" + CloneTextBox.Text + Path.GetExtension(file));
                Notes.Add(CloneTextBox.Text, Notes[name]);
                Notes[CloneTextBox.Text].Name = CloneTextBox.Text;
                updageTagOnFlag(Notes[CloneTextBox.Text]);
                Notes.Remove(name);
            }
            MainControl.Items.Refresh();
            EndChangingDynamicItem();
        }
        private void updageTagOnFlag(Note note)
        {
            byte[] flag = NotesBrowser.getJPGFromImageControl(Properties.Resources.noteFlag);
            for (TextPointer position = note.Range.Start; position != null && position.CompareTo(note.Range.End) != 1; position = position.GetNextContextPosition(LogicalDirection.Forward))
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
                            if (byt[t] != flag[t]) { isflag = false; break; }
                        }
                        if (!isflag) continue;

                        image.Tag = note.Name;
                    }
                }
            }
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
                MainControl.SelectedIndex = -1;
            }
        }

       
      
        private void HookManager_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (noteText == null) return;
            var location = GetCloneControlLocation(noteText);
            var mouseEventArgs = e as MouseEventExtArgs;
            if (e != null && mouseEventArgs == null) { return; }
            System.Windows.Point absolutePoint = noteText.PointToScreen(new System.Windows.Point(0d, 0d));
            var absoluteRectangle = new Rectangle((int)absolutePoint.X, (int)absolutePoint.Y, location.Width, location.Height);
            if (absoluteRectangle.Contains(mouseEventArgs.X, mouseEventArgs.Y)) { return; }
            HookManager.MouseDown -= HookManager_MouseDown;
           
          //  textBox.MoveFocus(ParentControl.TextBox.MainControl);
        }
        TextBox noteText;
        private void TextValue_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var text = sender as TextBox;
            if (text == null) return;
            noteText = text;
            HookManager.MouseDown -= HookManager_MouseDown;
            HookManager.MouseDown += HookManager_MouseDown;
        }
    }
}
