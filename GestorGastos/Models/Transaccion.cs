using SQLite;

namespace GestorGastos.Models;

[Table("Transacciones")]
public class Transaccion
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CuentaId { get; set; }

    public string Tipo { get; set; } = TiposMovimiento.Gasto;

    public decimal Monto { get; set; }

    [MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    public string Nota { get; set; } = string.Empty;

    [Indexed]
    public int? CategoriaId { get; set; }

    [Indexed]
    public DateTime Fecha { get; set; } = DateTime.Today;

    public bool EsRecurrente { get; set; }

    public int? RecurrenteId { get; set; }

    public bool EsPagoDeuda { get; set; }

    public bool EsTransferencia { get; set; }

    public string GrupoTransferencia { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
