using System;
using System.Windows;
using System.Windows.Input;

namespace UILayer
{
    internal class Command : IDisposable
    {
        private UIElement Control;

        public delegate void ExecuteDelegate();

        public ExecuteDelegate OnExecute;

        public Command(UIElement control)
        {
            if (control == null){ throw new ArgumentNullException(); }
            Control = control;
            AttachEventHandler();
        }

        public void Dispose()
        {
            DetachEventHandler();
            Control = null;
        }

        private void AttachEventHandler()
        {
            Control.KeyDown -= Control_KeyDown;
            Control.KeyDown += Control_KeyDown;
        }

        private void DetachEventHandler()
        {
            Control.KeyDown -= Control_KeyDown;
        }

        private void Control_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == KeyShortCut && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                Execute();
            }
        }

        public Key KeyShortCut;

        public string Name;

        public void Execute()
        {
            if (OnExecute != null)
            {
                OnExecute.Invoke();
            }
        }

    }
}
