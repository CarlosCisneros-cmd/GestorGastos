using System.Globalization;

namespace GestorGastos.Converters;

public class TextoAColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string texto && !string.IsNullOrWhiteSpace(texto))
        {
            try
            {
                return Color.FromArgb(texto);
            }
            catch (Exception)
            {
                return Colors.Gray;
            }
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
