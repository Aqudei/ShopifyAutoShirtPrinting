using ShopifyEasyShirtPrinting.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ShopifyEasyShirtPrinting.Converters
{
    public class MessageTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { 
            if (value is not DisplayMessage.MESSAGE_TYPE messageType) 
                return Brushes.Transparent; 
            var resourceKey = messageType switch 
            {
                DisplayMessage.MESSAGE_TYPE.Info => "Message.InfoBrush", 
                DisplayMessage.MESSAGE_TYPE.Warning => "Message.WarningBrush", 
                DisplayMessage.MESSAGE_TYPE.Error => "Message.ErrorBrush", 
                DisplayMessage.MESSAGE_TYPE.Success => "Message.SuccessBrushs", 
                _ => null 
            }; 
            if (resourceKey is null) 
                return Brushes.Transparent; 
            return Application.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Transparent; 
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
