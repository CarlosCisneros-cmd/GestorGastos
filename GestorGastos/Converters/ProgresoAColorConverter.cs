using System.Globalization;

namespace GestorGastos.Converters;

public class ProgresoAColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var progreso = value is double numero ? numero : 0;
        if (progreso >= 1)
        {
            return Color.FromArgb("#E0526B");
        }
        if (progreso >= 0.75)
        {
            return Color.FromArgb("#E59A2F");
        }
        return Color.FromArgb("#1F9D6B");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
