namespace GestorGastos.Models;

public class FiltroTransacciones
{
    public DateTime Desde { get; set; } = DateTime.Today.AddMonths(-1);

    public DateTime Hasta { get; set; } = DateTime.Today.AddDays(1);

    public int? CuentaId { get; set; }

    public int? CategoriaId { get; set; }

    public string? Tipo { get; set; }

    public bool IncluirTransferencias { get; set; } = true;

    public string? Texto { get; set; }
}
