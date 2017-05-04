using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Controls
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
