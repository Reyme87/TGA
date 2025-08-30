using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TGA
{
    public class DateTimeToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                // Преобразуем в локальное время пользователя
                DateTime localTime = dateTime.ToLocalTime();
                return localTime.ToString("dd.MM.yyyy HH:mm:ss", culture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}