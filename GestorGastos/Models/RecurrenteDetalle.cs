using GestorGastos.Helpers;
using SQLite;

namespace GestorGastos.Models;

public class RecurrenteDetalle
{
    public int Id { get; set; }

    public string Tipo { get; set; } = TiposMovimiento.Gasto;

    public decimal Monto { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Frecuencia { get; set; } = Frecuencias.Mensual;

    public DateTime ProximaFecha { get; set; }

    public bool Activo { get; set; }

    public string CategoriaIcono { get; set; } = string.Empty;

    public string CuentaNombre { get; set; } = string.Empty;

    [Ignore]
    public string Icono => string.IsNullOrEmpty(CategoriaIcono) ? "🔁" : CategoriaIcono;

    [Ignore]
    public string MontoTexto => Formato.MontoConSigno(Monto, Tipo);

    [Ignore]
    public string Detalle => Activo
        ? $"{Frecuencia} · Próximo: {Formato.FechaCorta(ProximaFecha)}"
        : $"{Frecuencia} · Pausado";

    [Ignore]
    public string TextoBotonEstado => Activo ? "Pausar" : "Activar";

    [Ignore]
    public double Opacidad => Activo ? 1 : 0.5;
}
