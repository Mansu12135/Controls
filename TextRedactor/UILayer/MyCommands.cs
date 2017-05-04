using System.Windows.Input;

namespace UILayer
{
	public static class MyCommands
	{
		static MyCommands()
		{
			SetFontSize = new RoutedCommand();
			SetFontColor = new RoutedCommand();
            SetFontFamily = new RoutedCommand();

        }

		public static RoutedCommand SetFontSize { get; private set; }
        public static RoutedCommand SetFontFamily { get; private set; }
        public static RoutedCommand SetFontColor { get; private set; }
	}
}
