using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace LogDashboard.Converters
{
    public class LogLevelToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var level = value as string;

            return level switch
            {
                "Fatal" => Brush.Parse("#3B0764"),   // purple-950
                "Error" => Brush.Parse("#450A0A"),   // red-950
                "Warning" => Brush.Parse("#451A03"),   // amber-950
                "Information" => Brush.Parse("#0C1A3B"),   // blue-950
                "Debug" => Brush.Parse("#083344"),   // cyan-950
                "Verbose" => Brush.Parse("#0F172A"),   // slate-900
                _ => Brush.Parse("#0F172A"),
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
