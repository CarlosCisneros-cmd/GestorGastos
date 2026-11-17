using GestorGastos.Helpers;
using SQLite;

namespace GestorGastos.Models;

public class ComprobanteDetalle
{
    public int Id { get; set; }

    public int TransaccionId { get; set; }

    public string RutaImagen { get; set; } = string.Empty;

    public string Comercio { get; set; } = string.Empty;

    public string TextoDetectado { get; set; } = string.Empty;

    public DateTime FechaCaptura { get; set; }

    public string NombreMovimiento { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    public int? CategoriaId { get; set; }

    public string CategoriaNombre { get; set; } = string.Empty;

    public string CategoriaIcono { get; set; } = string.Empty;

    [Ignore]
    public string Titulo => string.IsNullOrWhiteSpace(Comercio) ? NombreMovimiento : Comercio;

    [Ignore]
    public string MontoTexto => Formato.Moneda(Monto);

    [Ignore]
    public string FechaTexto => Formato.FechaCorta(Fecha);

    [Ignore]
    public bool ImagenExiste => File.Exists(RutaImagen);
}
