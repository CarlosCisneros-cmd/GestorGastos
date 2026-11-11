using SQLite;

namespace GestorGastos.Models;

[Table("Presupuestos")]
public class Presupuesto
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Indexed]
    public int CategoriaId { get; set; }

    public decimal Monto { get; set; }

    public string Periodo { get; set; } = PeriodosPresupuesto.Mensual;

    public bool Activo { get; set; } = true;

    public string AlertaAdvertenciaPeriodo { get; set; } = string.Empty;

    public string AlertaExcedidoPeriodo { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
