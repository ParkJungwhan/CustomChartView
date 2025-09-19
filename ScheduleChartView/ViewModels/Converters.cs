using System.Globalization;
using System.Windows.Data;

namespace ScheduleChartView.ViewModels;

public class TimeToPixelsConverter : IMultiValueConverter
{
    // values[0] = StartMs, values[1] = PixelsPerMs
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not double ms || values[1] is not double ppm) return 0d;
        return ms * ppm;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public class DurationToWidthConverter : IMultiValueConverter
{
    // values[0] = DurationMs, values[1] = PixelsPerMs
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not double d || values[1] is not double ppm) return 0d;
        return Math.Max(4, d * ppm);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

public class RowToTopConverter : IMultiValueConverter
{
    // values[0] = Row, values[1] = RowHeight
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not int row || values[1] is not double h) return 0d;
        return row * h + 0.5 * (h - Math.Max(12, h - 20)) / 2; // 약간의 상하 여백 조정
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}