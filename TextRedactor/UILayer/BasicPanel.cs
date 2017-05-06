using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ApplicationLayer;
using Gma.UserActivityMonitor;
using Binding = System.Windows.Data.Binding;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;
using ToolTip = System.Windows.Controls.ToolTip;
using UserControl = System.Windows.Controls.UserControl;

namespace UILayer
{
    public abstract class BasicPanel<T> : UserControl, IBasicPanel<T> where T : Item
    {
        protected ToolTip ErrorToolTip;
        protected BindingExpression Binding { get; set; }
        protected bool IsValid { get; set; }
        private char[] WrongSymbols = System.IO.Path.GetInvalidFileNameChars();
        protected TextBox CloneTextBox { get; private set; }
        protected Rectangle CloneTextBoxLocation { get; private set; }

        protected Dictionary<string, T> ItemsCollection = new Dictionary<string, T>();

        internal SuperTextRedactor ParentControl;

        public event EventHandler<ProjectArgs> ProjectChanged;
        public event EventHandler<FileArgs> FileChanged;
        public event EventHandler<ProjectArgs> ProjectRenamed;
        public event EventHandler<FileArgs> FileRenamed;
        public event EventHandler<ProjectArgs> ProjectCreated;
        public event EventHandler<FileArgs> FileCreated;
        public event EventHandler<ProjectArgs> ProjectDeleted;
        public event EventHandler<FileArgs> FileDeleted;

        public Dictionary<string, T> Notes
        {
            get { return ItemsCollection; }
        }

        public double PreviousWidth { get; protected set; }

        private void DisposeDynamicItems()
        {
            DettachDynamicEvents();
            CloneTextBox = null;
        }
        protected string BindingPath { get; set; }

        public SaveItemManager<T> SaveItemManager
        {
            get
            {
                return saveItemManager ?? new SaveItemManager<T>(this);
            }
        }
        private SaveItemManager<T> saveItemManager;

        protected void OnCreateProject(object sender, ProjectArgs e)
        {
            ProjectCreated.Invoke(sender, e);
        }

        protected void OnRenamedProject(object sender, ProjectArgs e)
        {
            ProjectChanged.Invoke(sender, e);
        }

        protected void OnCreateFiles(object sender, FileArgs e)
        {
            FileCreated.Invoke(sender, e);
        }

        protected void OnRenamedFiles(object sender, FileArgs e)
        {
            FileChanged.Invoke(sender, e);
        }

        FileSystemWorker<Project> IFileSystemControl.FSWorker
        {
            get { return fsWorker ?? new FileSystemWorker<Project>(this as IBasicPanel<Project>); }
        }
        private FileSystemWorker<Project> fsWorker;

        protected void OnInitialize()
        {
            fsWorker = new FileSystemWorker<Project>(this as IBasicPanel<Project>);
        }

        private void InitializeDynamicControls(TextBox originalControl)
        {
            CloneTextBox = new TextBox();
            Grid.SetRow(CloneTextBox, 1);
            CloneTextBox.Height = originalControl.ActualHeight;
            CloneTextBox.Width = originalControl.ActualWidth;
            InitializeBinding();
            CloneTextBox.Text = originalControl.Text;
            CloneTextBox.FontSize = originalControl.FontSize;
            CloneTextBox.VerticalAlignment = VerticalAlignment.Top;
            CloneTextBox.HorizontalAlignment = HorizontalAlignment.Left;
            CloneTextBox.Margin = new Thickness(CloneTextBoxLocation.X, CloneTextBoxLocation.Y, 0, 0);
            CloneTextBox.Padding = originalControl.Padding;
            CloneTextBox.Tag = originalControl.Tag;
            AttachDynamicEvents();
        }

        private void InitializeBinding()
        {
            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath("BindingPath");
            binding.Mode = BindingMode.OneWayToSource;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            binding.ValidatesOnDataErrors = true;
            binding.NotifyOnValidationError = true;
            CloneTextBox.SetBinding(TextBox.TextProperty, binding);
        }

        private void AttachDynamicEvents()
        {
            CloneTextBox.KeyDown -= CloneTextBox_KeyDown;
            CloneTextBox.KeyDown += CloneTextBox_KeyDown;
        }

        private void CloneTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CloneTextBox_LostFocus(null, null);
            }
            else if (e.Key == Key.Escape)
            {
                EndChangingDynamicItem();
            }
        }

        public virtual string ProjectsPath { get; set; }

        string IFileSystemControl.CurrentProjectsPath {
            get { return ProjectsPath; }
        }

        protected virtual object OnSave(Action action, string project) { return null; }

        public void RemoveItem(string caption)
        {
            if (string.IsNullOrEmpty(caption) || !Notes.ContainsKey(caption)) { return; }
            Notes.Remove(caption);
        }

        private bool IsValidName()
        {
            foreach (var symbol in WrongSymbols)
            {
                if (CloneTextBox.Text.Contains(symbol.ToString())) { return false; }
            }
            return true;
        }

        protected virtual void OnValidate()
        {
            if (Binding == null) { Binding = CloneTextBox.GetBindingExpression(TextBox.TextProperty); }
            IsValid = IsValidName();
            if (!IsValid)
            {
                Validation.MarkInvalid(Binding, new ValidationError(new ExceptionValidationRule(), Binding));
                if (ErrorToolTip == null)
                {
                    ErrorToolTip = new ToolTip {Content = TextRedactor.PathErrorMessage};
                    CloneTextBox.ToolTip = ErrorToolTip;
                }
                ErrorToolTip.IsOpen = true;
            }
        }

        protected virtual void CloneTextBox_LostFocus(object sender, EventArgs e)
        {
            OnValidate();
        }

        private void DettachDynamicEvents()
        {
            HookManager.MouseDown -= CloneTextBox_LostFocus;
            CloneTextBox.KeyDown -= CloneTextBox_KeyDown;
        }
        protected void BeginChangingDynamicItem(TextBox originalControl)
        {
            CloneTextBoxLocation = GetCloneControlLocation(originalControl);
            InitializeDynamicControls(originalControl);
            AddDynamicControls();
            HookManager.MouseDown -= CloneTextBox_LostFocus;
            HookManager.MouseDown += CloneTextBox_LostFocus;
        }

        protected virtual Rectangle GetCloneControlLocation(TextBox control)
        {
            throw new NotImplementedException();
        }
        protected virtual void AddDynamicControls() { throw new NotImplementedException(); }
        protected virtual void RemoveDynamicControls() { throw new NotImplementedException(); }
        protected void EndChangingDynamicItem()
        {
            Validation.ClearInvalid(CloneTextBox.GetBindingExpression(TextBox.TextProperty));
            if (ErrorToolTip != null)
            {
                ErrorToolTip.IsOpen = false;
                ErrorToolTip = null;
            }
            Binding = null;
            RemoveDynamicControls();
            DisposeDynamicItems();
        }

        internal abstract string GenerateName(string name, string path = "", bool isProg = true);

        public void AddItem(T item)
        {
            if (item == null || Notes.ContainsKey(item.Name)) { return; }
            Notes.Add(item.Name, item);
        }

        public virtual void Callback(bool rezult, string message, EventArgs args)
        {

        }

        object IBasicPanel<T>.Save(string project, Action action)
        {
            return OnSave(action, project);
        }



        //event EventHandler<ProjectArgs> IFileSystemControl.ProjectChanged
        //{
        //    add { ProjectChanged += value; }
        //    remove { ProjectChanged -= value; }
        //}
        //private EventHandler<ProjectArgs> ProjectChanged;

        //public event EventHandler<FileArgs> FileChanged
        //{
        //    add { FileChanged += value; }
        //    remove { FileChanged -= value; }
        //}
        //public EventHandler<FileArgs> FileChanged;

        //event EventHandler<ProjectArgs> IFileSystemControl.ProjectDeleted
        //{
        //    add { ProjectDeleted += value; }
        //    remove { ProjectDeleted -= value; }
        //}
        //private EventHandler<ProjectArgs> ProjectDeleted;

        //event EventHandler<FileArgs> IFileSystemControl.FileDeleted
        //{
        //    add { FileDeleted += value; }
        //    remove { FileDeleted -= value; }
        //}
        //private EventHandler<FileArgs> FileDeleted;

        //event EventHandler<ProjectArgs> IFileSystemControl.ProjectRenamed
        //{
        //    add { ProjectRenamed += value; }
        //    remove { ProjectRenamed -= value; }
        //}
        //private EventHandler<ProjectArgs> ProjectRenamed;

        //event EventHandler<FileArgs> IFileSystemControl.FileRenamed
        //{
        //    add { FileRenamed += value; }
        //    remove { FileRenamed -= value; }
        //}
        //private EventHandler<FileArgs> FileRenamed;

        //event EventHandler<ProjectArgs> IFileSystemControl.ProjectCreated
        //{
        //    add { ProjectCreated += value; }
        //    remove { ProjectCreated -= value; }
        //}
        //private EventHandler<ProjectArgs> ProjectCreated;

        //event EventHandler<FileArgs> IFileSystemControl.FileCreated
        //{
        //    add { CreatedFile += value; }
        //    remove { CreatedFile -= value; }
        //}
        //private EventHandler<FileArgs> CreatedFile;


    }
}