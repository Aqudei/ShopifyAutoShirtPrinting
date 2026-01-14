using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ShopifyEasyShirtPrinting.Converters
{
    public class MessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DisplayMessage.MESSAGE_TYPE messageType)
                return Brushes.Transparent;

            switch (messageType)
            {
                case DisplayMessage.MESSAGE_TYPE.Info:
                    return Brushes.Blue;

                case DisplayMessage.MESSAGE_TYPE.Warning:
                    return Brushes. DarkOrange;

                case DisplayMessage.MESSAGE_TYPE.Error:
                    return Brushes.IndianRed;

                case DisplayMessage.MESSAGE_TYPE.Success:
                    return Brushes.Green;

                default:
                    return Brushes.Transparent;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
