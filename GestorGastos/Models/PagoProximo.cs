using SQLite;

namespace GestorGastos.Models;

[Table("PagosProximos")]
public class PagoProximo
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

    [Indexed]
    public DateTime FechaVencimiento { get; set; } = DateTime.Today;

    public bool Pagado { get; set; }

    public DateTime? FechaPago { get; set; }

    public int? RecurrenteId { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
