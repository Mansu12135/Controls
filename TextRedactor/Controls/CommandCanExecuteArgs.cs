using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Controls
{
	public class CommandCanExecuteParameter
	{
		public CommandCanExecuteParameter(object parameter)
		{
			Parameter = parameter;
		}

		public object Parameter { get; private set; }
		public object CurrentValue { get; set; }
	}
}
