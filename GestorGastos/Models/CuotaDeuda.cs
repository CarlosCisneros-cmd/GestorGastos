using SQLite;

namespace GestorGastos.Models;

[Table("CuotasDeuda")]
public class CuotaDeuda
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int DeudaId { get; set; }

    public int Numero { get; set; }

    public decimal Monto { get; set; }

    [Indexed]
    public DateTime FechaVencimiento { get; set; }

    public string Estado { get; set; } = EstadosCuota.Programada;

    public DateTime? FechaPago { get; set; }

    public int? TransaccionId { get; set; }
}
