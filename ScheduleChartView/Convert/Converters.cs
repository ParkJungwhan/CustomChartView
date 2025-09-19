using System.Globalization;
using System.Windows.Data;

namespace ScheduleChartView.Convert;

// (StartTime, NowUtc, WindowSeconds, px/s) -> Canvas.Left
public class TimeWindowLeftConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 4 ||
            values[0] is not DateTime start ||
            values[1] is not DateTime now ||
            values[2] is not int windowSec ||
            values[3] is not double pxPerSec)
            return 0d;

        // 윈도우 시작점
        var windowStart = now.AddSeconds(-windowSec);
        var secFromWindowStart = (start - windowStart).TotalSeconds;
        if (secFromWindowStart < 0) secFromWindowStart = 0;                 // 왼쪽 잘림
        if (secFromWindowStart > windowSec) secFromWindowStart = windowSec; // 오른쪽 잘림
        return secFromWindowStart * pxPerSec;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// (Duration, px/s) -> Width
public class DurationToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not TimeSpan dur ||
            values[1] is not double pxPerSec)
            return 4d;

        var w = dur.TotalSeconds * pxPerSec;
        return Math.Max(3, w);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// (row, rowHeight) -> Top
public class RowToTopConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 ||
            values[0] is not int row ||
            values[1] is not double h)
            return 0d;
        return row * h + (h - Math.Max(12, h - 12)) / 2;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}