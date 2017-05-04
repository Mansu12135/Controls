using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Controls
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
            CloneTextBox.LostFocus -= CloneTextBox_LostFocus;
            CloneTextBox.LostFocus += CloneTextBox_LostFocus;
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

        public void Save(string Name)
        {
            OnSave(Name);
        }

        protected virtual void OnSave(string Name) { }

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

        protected virtual void CloneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            OnValidate();
        }

        private void DettachDynamicEvents()
        {
            CloneTextBox.KeyDown -= CloneTextBox_KeyDown;
            CloneTextBox.LostFocus -= CloneTextBox_LostFocus;
        }
        protected void BeginChangingDynamicItem(TextBox originalControl)
        {
            CloneTextBoxLocation = GetCloneControlLocation(originalControl);
            InitializeDynamicControls(originalControl);
            AddDynamicControls();
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
    }
}