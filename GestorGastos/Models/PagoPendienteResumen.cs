using GestorGastos.Helpers;
using SQLite;

namespace GestorGastos.Models;

public class PagoPendienteResumen
{
    public string Nombre { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public string Icono { get; set; } = "📅";

    public string Origen { get; set; } = string.Empty;

    [Ignore]
    public string MontoTexto => Formato.Moneda(Monto);

    [Ignore]
    public string VenceTexto => $"Vence {Formato.FechaCorta(FechaVencimiento)}";
}
