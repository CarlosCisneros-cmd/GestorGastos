using System.Globalization;

namespace GestorGastos.Converters;

public class InvertirBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool booleano ? !booleano : true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool booleano ? !booleano : false;
    }
}
