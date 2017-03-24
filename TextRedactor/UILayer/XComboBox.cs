using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ApplicationLayer;

namespace UILayer
{
	public class XComboBox : ComboBox, ICommandSource
	{
		#region ICommandSource Members

		#region Command dependency property and routed event

		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		public readonly static DependencyProperty CommandProperty = DependencyProperty.Register(
			"Command",
			typeof(ICommand),
			typeof(XComboBox),
			new PropertyMetadata(default(ICommand), new PropertyChangedCallback(OnCommandChanged)));

		private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XComboBox owner = (XComboBox)d;
			owner.OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
		}

		public static readonly RoutedEvent CommandChangedEvent = EventManager.RegisterRoutedEvent(
			"CommandChangedEvent",
			RoutingStrategy.Bubble,
			typeof(RoutedPropertyChangedEventHandler<ICommand>),
			typeof(XComboBox));

		public event RoutedPropertyChangedEventHandler<ICommand> CommandChanged
		{
			add { AddHandler(CommandChangedEvent, value); }
			remove { RemoveHandler(CommandChangedEvent, value); }
		}

		protected virtual void OnCommandChanged(ICommand oldValue, ICommand newValue)
		{
			RoutedPropertyChangedEventArgs<ICommand> args = new RoutedPropertyChangedEventArgs<ICommand>(oldValue, newValue);
			args.RoutedEvent = XComboBox.CommandChangedEvent;
			RaiseEvent(args);

			if (cmd_CanExecuteChangedHandler == null)
				cmd_CanExecuteChangedHandler = cmd_CanExecuteChanged;
			if (oldValue != null)
				oldValue.CanExecuteChanged -= cmd_CanExecuteChangedHandler;
			if (newValue != null)
				newValue.CanExecuteChanged += cmd_CanExecuteChangedHandler;
			else
				cmd_CanExecuteChangedHandler = null;

			UpdateCanExecute();
		}
		// hold a reference to it, it might me stored as a weak reference and never be called otherwise...
		EventHandler cmd_CanExecuteChangedHandler;
		void cmd_CanExecuteChanged(object sender, EventArgs e)
		{
			UpdateCanExecute();
		}

		protected virtual void UpdateCanExecute()
		{
			if (IsCommandExecuting)
				return;

			ICommand cmd = Command;
			if (cmd == null)
			{
				IsEnabled = true;
			}
			else
			{
				try
				{
					IsCommandExecuting = true;

					IsEnabled = CommandUtil.CanExecute(this, CommandParameter);
					var ca = new CommandCanExecuteParameter(CommandParameter);
					CommandUtil.CanExecute(this, ca);
					object value = ca.CurrentValue;

					//if (value is RangedValue)
					//	value = ((RangedValue) value).Value;

					object o = FindItemByData(value);

					if (o == null && value == null)
					{
						Text = null;
						SelectedItem = null;
					}
					else if (o != null && !Equals(SelectedItem, o))
					{
						SelectedItem = o;
					}
					else if (IsEditable && !IsReadOnly
						&& value != null && Text != value.ToString())
					{
						Text = value.ToString();
					}
				}
				finally { IsCommandExecuting = false; }
			}
		}

		bool IsCommandExecuting { get; set; }

		#endregion

		#region CommandTarget dependency property and routed event

		public IInputElement CommandTarget
		{
			get { return (IInputElement)GetValue(CommandTargetProperty); }
			set { SetValue(CommandTargetProperty, value); }
		}

		public readonly static DependencyProperty CommandTargetProperty = DependencyProperty.Register(
			"CommandTarget",
			typeof(IInputElement),
			typeof(XComboBox),
			new PropertyMetadata(default(IInputElement), new PropertyChangedCallback(OnCommandTargetChanged)));

		private static void OnCommandTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XComboBox owner = (XComboBox)d;
			owner.UpdateCanExecute();
		}

		#endregion

		public object CommandParameter
		{
			get { return GetItemData(SelectedItem); }
		}

		#endregion

		public object FindItemByData(object data)
		{
			foreach (object o in Items)
				if (Equals(data, GetItemData(o)))
					return o;
			return null;
		}

		public static object GetItemData(object obj)
		{
			ComboBoxItem cbi = obj as ComboBoxItem;
			if (cbi != null)
			{
				if (cbi.ReadLocalValue(FrameworkElement.TagProperty) != DependencyProperty.UnsetValue)
					return cbi.GetValue(FrameworkElement.TagProperty);
				return cbi.Content;
			}
			return obj;
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			base.OnSelectionChanged(e);
			if (!IsCommandExecuting)
				try
				{
					IsCommandExecuting = true;
					CommandUtil.Execute(this);
				}
				finally { IsCommandExecuting = false; }
		}
	}
}
