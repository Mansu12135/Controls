using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;


namespace ApplicationLayer
{
    public class RombConverter : MarkupExtension, IValueConverter
    {
        private static RombConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          //  var val = value as LoadedFile;
            return System.Convert.ToDouble(value)==1?System.Convert.ToDouble(parameter):0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new RombConverter());
        }
    }
}
