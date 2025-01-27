using AngleSharp.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShopifyEasyShirtPrinting.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        private readonly Dictionary<string, SolidColorBrush> _colorLookup;

        public StatusToColorConverter()
        {
            _colorLookup = new Dictionary<string, SolidColorBrush>()
            {
                ["Pending"] = new SolidColorBrush(Color.FromArgb(50, 0xff, 0xff, 0xff)),
                ["Processed"] = new SolidColorBrush(Color.FromArgb(50, 0, 0xde, 0xff)),
                ["LabelPrinted"] = new SolidColorBrush(Color.FromArgb(50, 0x0c, 0, 0xff)),
                ["Shipped"] = new SolidColorBrush(Color.FromArgb(50, 0, 0xff, 0x22)),
                ["Archived"] = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                ["Need To Order From Supplier"] = new SolidColorBrush(Color.FromArgb(50, 0xff, 0, 0)),
                ["Have Ordered From Supplier"] = new SolidColorBrush(Color.FromArgb(50, 0, 0xf0, 0)),
                ["Issue Needs Resolving"] = new SolidColorBrush(Color.FromArgb(50, 0xff, 0, 0xff)),
            };

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value.ToString();
            return _colorLookup.GetOrDefault(status, Brushes.Blue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

}
