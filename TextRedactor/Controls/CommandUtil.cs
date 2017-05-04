using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Controls
{
	public static class CommandUtil 
    {
		public static bool CanExecute (ICommandSource source)
		{
			return CanExecute(source.Command, source.CommandParameter, source.CommandTarget);
		}

		public static bool CanExecute (ICommandSource source, object parameter)
		{
			if (source == null
				|| source.Command == null)
				return false;

			IInputElement target = source.CommandTarget;

			if (target == null && Keyboard.FocusedElement == null)
				target = source as IInputElement;

			return CanExecute(source.Command, parameter, target);
		}

		public static bool CanExecute(ICommand command, object parameter, IInputElement target)
		{
			if (command == null)
				return false;

			var routedCommand = command as RoutedCommand;
			if (routedCommand == null)
			{
				return command.CanExecute(parameter);
			}

			return routedCommand.CanExecute(parameter, target);
		}

		public static void Execute (ICommandSource source)
		{
			Execute(source, source.CommandParameter);
		}

		public static void Execute(ICommandSource source, object parameter)
		{
			if (source == null)
				return;

			ICommand cmd = source.Command;
			if (cmd == null)
				return;

			IInputElement target = source.CommandTarget;

			if (target == null && Keyboard.FocusedElement == null)
				target = source as IInputElement;

			Execute(cmd, parameter, target);
		}

		public static void Execute(ICommand command, object parameter, IInputElement target)
		{
			if (command == null)
				return;

			RoutedCommand rcmd = command as RoutedCommand;
			if (rcmd == null)
			{
				command.Execute(parameter);
			}
			else
			{
				rcmd.Execute(parameter, target);
			}
            if (OnCommandExecute != null)
            {
                OnCommandExecute.Invoke(command, new EventArgs());
            }
        }

        public delegate void CommandExecuteHandler(object sender, EventArgs e);

        public static event CommandExecuteHandler OnCommandExecute;

        public static void SetCurrentValue (CanExecuteRoutedEventArgs e, object value)
		{
			if (e.Parameter is CommandCanExecuteParameter)
			{
				(e.Parameter as CommandCanExecuteParameter).CurrentValue = value;
			}
		}
	}
}
