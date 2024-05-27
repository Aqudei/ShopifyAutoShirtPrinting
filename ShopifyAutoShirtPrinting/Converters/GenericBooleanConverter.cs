using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ShopifyEasyShirtPrinting.Converters
{
    public class GenericBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), "Converter parameter cannot be null");

            var parameters = parameter.ToString().Split(';');
            if (parameters.Length != 2)
                throw new ArgumentException("Converter parameter must contain two values separated by a semicolon");

            var trueValue = parameters[0];
            var falseValue = parameters[1];

            if (value is bool boolValue)
            {
                return boolValue ? trueValue : falseValue;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter), "Converter parameter cannot be null");

            var parameters = parameter.ToString().Split(';');
            if (parameters.Length != 2)
                throw new ArgumentException("Converter parameter must contain two values separated by a semicolon");

            var trueValue = parameters[0];
            var falseValue = parameters[1];

            if (value != null && value.ToString() == trueValue)
            {
                return true;
            }

            if (value != null && value.ToString() == falseValue)
            {
                return false;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
