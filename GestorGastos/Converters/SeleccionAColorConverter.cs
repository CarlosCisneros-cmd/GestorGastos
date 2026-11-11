using System.Globalization;

namespace GestorGastos.Converters;

public class SeleccionAColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var seleccionado = value is bool booleano && booleano;
        if (seleccionado)
        {
            return Color.FromArgb("#7C5CDB");
        }

        var oscuro = Application.Current?.RequestedTheme == AppTheme.Dark;
        return oscuro ? Color.FromArgb("#2A2533") : Color.FromArgb("#FFFFFF");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
