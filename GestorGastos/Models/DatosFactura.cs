namespace GestorGastos.Models;

public class DatosFactura
{
    public bool Exito { get; set; }

    public decimal? Total { get; set; }

    public DateTime? Fecha { get; set; }

    public string Comercio { get; set; } = string.Empty;

    public string TextoCompleto { get; set; } = string.Empty;
}
