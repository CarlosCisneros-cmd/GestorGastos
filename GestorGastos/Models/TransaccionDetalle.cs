using GestorGastos.Helpers;
using SQLite;

namespace GestorGastos.Models;

public class TransaccionDetalle
{
    public int Id { get; set; }

    public int CuentaId { get; set; }

    public string Tipo { get; set; } = TiposMovimiento.Gasto;

    public decimal Monto { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Nota { get; set; } = string.Empty;

    public int? CategoriaId { get; set; }

    public DateTime Fecha { get; set; }

    public bool EsRecurrente { get; set; }

    public bool EsPagoDeuda { get; set; }

    public bool EsTransferencia { get; set; }

    public string GrupoTransferencia { get; set; } = string.Empty;

    public string CategoriaNombre { get; set; } = string.Empty;

    public string CategoriaIcono { get; set; } = string.Empty;

    public string CategoriaColor { get; set; } = string.Empty;

    public string CuentaNombre { get; set; } = string.Empty;

    public bool TieneComprobante { get; set; }

    [Ignore]
    public bool EsIngreso => Tipo == TiposMovimiento.Ingreso;

    [Ignore]
    public string Icono => string.IsNullOrEmpty(CategoriaIcono) ? (EsIngreso ? "💰" : "🧾") : CategoriaIcono;

    [Ignore]
    public string MontoTexto => Formato.MontoConSigno(Monto, Tipo);

    [Ignore]
    public string ColorMonto => EsTransferencia ? "#7C5CDB" : (EsIngreso ? "#1F9D6B" : "#E0526B");

    [Ignore]
    public string ColorFondoIcono => EsIngreso ? "#E2F6EC" : "#FDE7EB";

    [Ignore]
    public string NombreCategoriaVisible => string.IsNullOrEmpty(CategoriaNombre) ? (EsIngreso ? "Ingreso" : "Gasto") : CategoriaNombre;

    [Ignore]
    public string Subtitulo => $"{NombreCategoriaVisible} · {Formato.FechaCorta(Fecha)}";

    [Ignore]
    public string SubtituloCuenta => $"{NombreCategoriaVisible} · {CuentaNombre}";
}
