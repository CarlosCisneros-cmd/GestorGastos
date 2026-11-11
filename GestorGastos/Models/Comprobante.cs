using SQLite;

namespace GestorGastos.Models;

[Table("Comprobantes")]
public class Comprobante
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int TransaccionId { get; set; }

    public string RutaImagen { get; set; } = string.Empty;

    public string TextoDetectado { get; set; } = string.Empty;

    public string Comercio { get; set; } = string.Empty;

    public decimal? MontoDetectado { get; set; }

    public DateTime FechaCaptura { get; set; } = DateTime.Now;
}
