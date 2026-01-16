using AngleSharp.Common;
using ShopifyEasyShirtPrinting.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Windows.Data;
using System.Windows.Media;

namespace ShopifyEasyShirtPrinting.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        private readonly Dictionary<string, SolidColorBrush> _colorLookup;
        private readonly Dictionary<string, SolidColorBrush> _customColorLookup = new Dictionary<string, SolidColorBrush>(StringComparer.OrdinalIgnoreCase);

        public StatusToColorConverter()
        {
            _colorLookup = new Dictionary<string, SolidColorBrush>()
            {
                ["Pending"] = new SolidColorBrush(Color.FromArgb(50, 0xff, 0xff, 0xff)),
                ["Printed"] = new SolidColorBrush(Color.FromArgb(50, 0xff, 0xff, 0xff)),
                ["Processed"] = new SolidColorBrush(Color.FromArgb(50, 0, 0xde, 0xff)),
                ["LabelPrinted"] = new SolidColorBrush(Color.FromArgb(50, 0x0c, 0, 0xff)),
                ["Shipped"] = new SolidColorBrush(Color.FromArgb(50, 0, 0xff, 0x22)),
                ["Archived"] = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                ["Need To Order From Supplier"] = new SolidColorBrush(Color.FromArgb(50, 0xff, 0, 0)),
                ["Have Ordered From Supplier"] = new SolidColorBrush(Color.FromArgb(50, 0, 0xf0, 0)),
                ["Issue Needs Resolving"] = new SolidColorBrush(Color.FromArgb(50, 0xff, 0, 0xff)),
            };

            var settingJson = Properties.Settings.Default.LineColor;
            if (!string.IsNullOrWhiteSpace(settingJson))
            {
                var colorSettings = JsonSerializer.Deserialize<IEnumerable<ColorSetting>>(settingJson);
                _customColorLookup = new Dictionary<string, SolidColorBrush>(StringComparer.OrdinalIgnoreCase);

                if (!string.IsNullOrWhiteSpace(settingJson) && colorSettings != null)
                {
                    foreach (var setting in colorSettings)
                    {
                        try
                        {
                            _customColorLookup[setting.Key] = new SolidColorBrush(setting.Color);
                        }
                        catch
                        {
                            // Log or ignore invalid color formats
                        }

                    }
                }
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value?.ToString() ?? "";

            if (_customColorLookup.TryGetValue(status, out var customBrush))
                return customBrush;

            if (_colorLookup.TryGetValue(status, out var defaultBrush))
                return defaultBrush;

            return Brushes.Blue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

}
