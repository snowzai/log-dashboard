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
            "Fatal" => Brush.Parse("#E9D5FF"),  // purple-200
            "Error" => Brush.Parse("#FCA5A5"),  // red-300
            "Warning" => Brush.Parse("#FDE68A"),  // amber-200
            "Information" => Brush.Parse("#93C5FD"),  // blue-300
            "Debug" => Brush.Parse("#67E8F9"),  // cyan-300
            "Verbose" => Brush.Parse("#94A3B8"),  // slate-400
            _ => Brush.Parse("#94A3B8"),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}