using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace LogDashboard.Converters;

public class LogLevelToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var level = value as string;

        return level switch
        {
            "Fatal" => Brush.Parse("#B00020"),
            "Error" => Brush.Parse("#CF6679"),
            "Warning" => Brush.Parse("#FFCA28"),
            "Information" => Brush.Parse("#42A5F5"),
            "Debug" => Brush.Parse("#66BB6A"),
            "Verbose" => Brush.Parse("#9E9E9E"),
            _ => Brush.Parse("#9E9E9E"),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}