using SQLite;

namespace GestorGastos.Models;

[Table("AportesAhorro")]
public class AporteAhorro
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int MetaId { get; set; }

    public int CuentaId { get; set; }

    public int TransaccionId { get; set; }

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;
}
