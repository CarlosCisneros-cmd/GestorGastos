using SQLite;

namespace GestorGastos.Models;

[Table("MetasAhorro")]
public class MetaAhorro
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(60)]
    public string Nombre { get; set; } = string.Empty;

    public string Icono { get; set; } = "🎯";

    public decimal MontoObjetivo { get; set; }

    public decimal MontoActual { get; set; }

    public DateTime? FechaLimite { get; set; }

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
