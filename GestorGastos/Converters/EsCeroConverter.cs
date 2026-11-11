using System.Globalization;

namespace GestorGastos.Converters;

public class EsCeroConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var esCero = value is int numero && numero == 0;
        var invertir = parameter is string texto && texto == "invertir";
        return invertir ? !esCero : esCero;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
