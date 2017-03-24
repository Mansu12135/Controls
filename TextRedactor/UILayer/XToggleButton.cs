using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ApplicationLayer;

namespace UILayer
{
	public class XToggleButton : ToggleButton
	{
		static XToggleButton()
		{
			CommandProperty.OverrideMetadata(typeof(XToggleButton), new FrameworkPropertyMetadata(OnCommandChanged));
		}

		private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((XToggleButton)d).OnCommandChanged(e.OldValue as ICommand, e.NewValue as ICommand);
		}

		private void OnCommandChanged(ICommand oldValue, ICommand newValue)
		{
			if (oldValue != null)
				oldValue.CanExecuteChanged -= OnCanExecuteChanged;

			if (newValue != null)
			{
				//the command system uses WeakReferences internally,
				//so we have to hold a reference to the canExecuteChanged handler ourselves

				if (canExecuteChangedHandler == null)
					canExecuteChangedHandler = OnCanExecuteChanged;

				newValue.CanExecuteChanged += canExecuteChangedHandler;
			}
			else
				canExecuteChangedHandler = null;

		}

		//hold a reference to the canExecuteChangedHandler so that it is not garbage collected
		private EventHandler canExecuteChangedHandler;

		private void OnCanExecuteChanged(object sender, EventArgs e)
		{
			UpdateCanExecute();
		}

		private void UpdateCanExecute()
		{
			if (IsCommandExecuting)
				return;

			IsCommandExecuting = true;
			try
			{
				//use our custom class as the parameter
				var parameter = new CommandCanExecuteParameter(null);
				CommandUtil.CanExecute(this, parameter);
				//we set the current status independent on whether the command can execute
				{
					if (parameter.CurrentValue is bool)
					{
						IsChecked = (bool)parameter.CurrentValue;
					}
					else
					{
						IsChecked = false;
					}
				}
			}
			finally
			{
				IsCommandExecuting = false;
			}

		}

		private bool IsCommandExecuting { get; set; }

		protected override void OnClick()
		{
			IsCommandExecuting = true;
			try
			{
				base.OnClick();
			}
			finally
			{
				IsCommandExecuting = false;
			}

		}
	}
}
