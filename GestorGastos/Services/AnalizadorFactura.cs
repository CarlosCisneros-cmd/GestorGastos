using System.Globalization;
using System.Text.RegularExpressions;
using GestorGastos.Models;

namespace GestorGastos.Services;

public static class AnalizadorFactura
{
    private static readonly Regex PatronMonto = new(@"(\d{1,3}(?:[.,]\d{3})*[.,]\d{2}|\d+[.,]\d{2})", RegexOptions.Compiled);
    private static readonly Regex PatronFecha = new(@"\b(\d{1,2})[/\-.](\d{1,2})[/\-.](\d{2,4})\b", RegexOptions.Compiled);

    private static readonly string[] PalabrasTotalFuerte = { "VALOR A PAGAR", "TOTAL A PAGAR", "IMPORTE TOTAL", "TOTAL PAGADO", "VALOR TOTAL" };
    private static readonly string[] PalabrasIgnorar = { "RUC", "FACTURA", "AUTORIZACION", "AUTORIZACIÓN", "FECHA", "DIRECCION", "DIRECCIÓN", "TELF", "TELEFONO", "TELÉFONO", "CLAVE", "NO.", "N°" };

    public static DatosFactura Analizar(string texto)
    {
        var datos = new DatosFactura { TextoCompleto = texto ?? string.Empty };
        if (string.IsNullOrWhiteSpace(texto))
        {
            return datos;
        }

        var lineas = texto
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        datos.Total = BuscarTotal(lineas);
        datos.Fecha = BuscarFecha(texto);
        datos.Comercio = BuscarComercio(lineas);
        datos.Exito = datos.Total.HasValue;
        return datos;
    }

    private static decimal? BuscarTotal(List<string> lineas)
    {
        for (var i = 0; i < lineas.Count; i++)
        {
            var mayus = lineas[i].ToUpperInvariant();
            if (PalabrasTotalFuerte.Any(p => mayus.Contains(p)))
            {
                var monto = MontoEnLinea(lineas, i);
                if (monto.HasValue)
                {
                    return monto;
                }
            }
        }

        for (var i = lineas.Count - 1; i >= 0; i--)
        {
            var mayus = lineas[i].ToUpperInvariant();
            if (mayus.Contains("TOTAL") && !mayus.Contains("SUBTOTAL") && !mayus.Contains("SUB TOTAL"))
            {
                var monto = MontoEnLinea(lineas, i);
                if (monto.HasValue)
                {
                    return monto;
                }
            }
        }

        var todos = lineas
            .SelectMany(l => PatronMonto.Matches(l).Select(m => ConvertirMonto(m.Value)))
            .Where(m => m.HasValue && m.Value > 0)
            .Select(m => m!.Value)
            .ToList();

        return todos.Count > 0 ? todos.Max() : null;
    }

    private static decimal? MontoEnLinea(List<string> lineas, int indice)
    {
        var coincidencias = PatronMonto.Matches(lineas[indice]);
        if (coincidencias.Count > 0)
        {
            return ConvertirMonto(coincidencias[^1].Value);
        }

        if (indice + 1 < lineas.Count)
        {
            var siguiente = PatronMonto.Matches(lineas[indice + 1]);
            if (siguiente.Count > 0)
            {
                return ConvertirMonto(siguiente[0].Value);
            }
        }

        return null;
    }

    private static decimal? ConvertirMonto(string valor)
    {
        var limpio = valor.Trim();
        var separadorDecimal = limpio[^3];
        var sinMiles = separadorDecimal == ','
            ? limpio.Replace(".", string.Empty).Replace(',', '.')
            : limpio.Replace(",", string.Empty);

        return decimal.TryParse(sinMiles, NumberStyles.Number, CultureInfo.InvariantCulture, out var monto)
            ? monto
            : null;
    }

    private static DateTime? BuscarFecha(string texto)
    {
        foreach (Match coincidencia in PatronFecha.Matches(texto))
        {
            var dia = int.Parse(coincidencia.Groups[1].Value);
            var mes = int.Parse(coincidencia.Groups[2].Value);
            var anio = int.Parse(coincidencia.Groups[3].Value);
            if (anio < 100)
            {
                anio += 2000;
            }

            if (mes is >= 1 and <= 12 && dia >= 1 && dia <= DateTime.DaysInMonth(Math.Clamp(anio, 1, 9999), mes) && anio >= 2000 && anio <= DateTime.Today.Year + 1)
            {
                return new DateTime(anio, mes, dia);
            }
        }

        return null;
    }

    private static string BuscarComercio(List<string> lineas)
    {
        foreach (var linea in lineas.Take(6))
        {
            var mayus = linea.ToUpperInvariant();
            var letras = linea.Count(char.IsLetter);
            if (letras < 3 || PalabrasIgnorar.Any(p => mayus.StartsWith(p)))
            {
                continue;
            }

            var texto = CultureInfo.GetCultureInfo("es-EC").TextInfo.ToTitleCase(linea.ToLowerInvariant());
            return texto.Length > 60 ? texto[..60] : texto;
        }

        return string.Empty;
    }
}
