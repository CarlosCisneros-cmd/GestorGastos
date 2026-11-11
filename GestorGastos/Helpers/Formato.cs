using System.Globalization;
using GestorGastos.Models;

namespace GestorGastos.Helpers;

public static class Formato
{
    public static readonly CultureInfo Cultura = new("es-EC");

    private static readonly Dictionary<string, string> Simbolos = new()
    {
        { "USD", "$" },
        { "EUR", "€" },
        { "COP", "$" },
        { "PEN", "S/" },
        { "MXN", "$" }
    };

    public static List<string> MonedasDisponibles => Simbolos.Keys.ToList();

    public static string SimboloActual
    {
        get
        {
            var codigo = Preferencias.Moneda;
            return Simbolos.TryGetValue(codigo, out var simbolo) ? simbolo : "$";
        }
    }

    public static string Moneda(decimal valor)
    {
        var formato = (NumberFormatInfo)Cultura.NumberFormat.Clone();
        formato.CurrencySymbol = SimboloActual;
        formato.CurrencyNegativePattern = 1;
        formato.CurrencyPositivePattern = 0;
        return valor.ToString("C2", formato);
    }

    public static string MontoConSigno(decimal monto, string tipo)
    {
        var texto = Moneda(Math.Abs(monto));
        return tipo == TiposMovimiento.Ingreso ? $"+{texto}" : $"−{texto}";
    }

    public static string FechaCorta(DateTime fecha)
    {
        var hoy = DateTime.Today;
        if (fecha.Date == hoy) return "Hoy";
        if (fecha.Date == hoy.AddDays(-1)) return "Ayer";
        if (fecha.Date == hoy.AddDays(1)) return "Mañana";
        return fecha.ToString("d MMM", Cultura).Replace(".", string.Empty);
    }

    public static string FechaLarga(DateTime fecha)
    {
        return fecha.ToString("d 'de' MMMM 'de' yyyy", Cultura);
    }

    public static string MesAnio(DateTime fecha)
    {
        var texto = fecha.ToString("MMMM yyyy", Cultura);
        return char.ToUpper(texto[0]) + texto[1..];
    }

    public static string DiaTitulo(DateTime fecha)
    {
        var hoy = DateTime.Today;
        if (fecha.Date == hoy) return "Hoy";
        if (fecha.Date == hoy.AddDays(-1)) return "Ayer";
        return fecha.ToString("dddd d 'de' MMMM", Cultura);
    }

    public static bool IntentarLeerMonto(string? texto, out decimal monto)
    {
        monto = 0;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        var limpio = texto.Trim().Replace("$", string.Empty).Replace(" ", string.Empty);
        var ultimaComa = limpio.LastIndexOf(',');
        var ultimoPunto = limpio.LastIndexOf('.');
        if (ultimaComa > ultimoPunto)
        {
            limpio = limpio.Replace(".", string.Empty).Replace(',', '.');
        }
        else
        {
            limpio = limpio.Replace(",", string.Empty);
        }
        return decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out monto);
    }

    public static string MontoParaEditar(decimal monto)
    {
        return monto.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
