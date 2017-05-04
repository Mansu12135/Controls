using System.Windows.Input;

namespace UILayer
{
    class BaseCommandBinding : CommandBinding
    {
        private BaseRichTextBox Element { get; set; }

        public BaseCommandBinding(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute, BaseRichTextBox richTextBox)
            :base(command,executed,canExecute)
        {
            Element = richTextBox;
            Executed -= Element.OnCommandExecuted;
            Executed += Element.OnCommandExecuted;
        }

    }
}
