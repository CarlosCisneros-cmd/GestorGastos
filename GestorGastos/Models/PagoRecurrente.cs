using SQLite;

namespace GestorGastos.Models;

[Table("PagosRecurrentes")]
public class PagoRecurrente
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int CuentaId { get; set; }

    public string Tipo { get; set; } = TiposMovimiento.Gasto;

    public decimal Monto { get; set; }

    [MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    public string Nota { get; set; } = string.Empty;

    public int? CategoriaId { get; set; }

    public string Frecuencia { get; set; } = Frecuencias.Mensual;

    public DateTime FechaInicio { get; set; } = DateTime.Today;

    public DateTime ProximaFecha { get; set; } = DateTime.Today;

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
