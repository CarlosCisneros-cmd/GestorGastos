using GestorGastos.Helpers;
using SQLite;

namespace GestorGastos.Models;

public class PagoProximoDetalle
{
    public int Id { get; set; }

    public int CuentaId { get; set; }

    public string Tipo { get; set; } = TiposMovimiento.Gasto;

    public decimal Monto { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int? CategoriaId { get; set; }

    public DateTime FechaVencimiento { get; set; }

    public bool Pagado { get; set; }

    public int? RecurrenteId { get; set; }

    public string CategoriaIcono { get; set; } = string.Empty;

    public string CuentaNombre { get; set; } = string.Empty;

    [Ignore]
    public string Icono => string.IsNullOrEmpty(CategoriaIcono) ? "📅" : CategoriaIcono;

    [Ignore]
    public string MontoTexto => Formato.MontoConSigno(Monto, Tipo);

    [Ignore]
    public bool EsRecurrente => RecurrenteId.HasValue;

    [Ignore]
    public bool EstaVencido => FechaVencimiento.Date < DateTime.Today;

    [Ignore]
    public string EstadoTexto
    {
        get
        {
            var dias = (FechaVencimiento.Date - DateTime.Today).Days;
            if (dias < 0) return $"Vencido hace {-dias} día(s)";
            if (dias == 0) return "Vence hoy";
            if (dias == 1) return "Vence mañana";
            return $"Vence en {dias} días · {Formato.FechaCorta(FechaVencimiento)}";
        }
    }

    [Ignore]
    public string ColorEstado => EstaVencido ? "#E0526B" : (FechaVencimiento.Date <= DateTime.Today.AddDays(3) ? "#E59A2F" : "#8A8294");

    [Ignore]
    public string Detalle => EsRecurrente ? $"{CuentaNombre} · Recurrente" : CuentaNombre;
}
