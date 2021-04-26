using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RSI_Client.Converters
{
    class EmptyFieldsMultiValidationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string login = values[0] as string;
            if (string.IsNullOrEmpty(login)) return false;

            string password = values[1] as string;
            if (string.IsNullOrEmpty(password)) return false;

            string repeatPassword = values[2] as string;
            if (string.IsNullOrEmpty(repeatPassword)) return false;

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
