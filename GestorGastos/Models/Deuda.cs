using SQLite;

namespace GestorGastos.Models;

[Table("Deudas")]
public class Deuda
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(60)]
    public string Nombre { get; set; } = string.Empty;

    public decimal MontoTotal { get; set; }

    public decimal MontoPendiente { get; set; }

    public decimal TasaInteres { get; set; }

    public bool InteresIncluido { get; set; } = true;

    public int NumeroCuotas { get; set; } = 1;

    public DateTime FechaInicio { get; set; } = DateTime.Today;

    public int? CuentaId { get; set; }

    public string Notas { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
