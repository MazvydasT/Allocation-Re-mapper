using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shell;

namespace AllocationRerouter
{
    public class ProgressBarColourConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (TaskbarItemProgressState)values[0] == TaskbarItemProgressState.Error ? Brushes.Red : values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
