using GestorGastos.Helpers;
using SQLite;

namespace GestorGastos.Models;

public class GastoPorCategoria
{
    public int? CategoriaId { get; set; }

    public string CategoriaNombre { get; set; } = string.Empty;

    public string CategoriaIcono { get; set; } = string.Empty;

    public string CategoriaColor { get; set; } = string.Empty;

    public decimal Total { get; set; }

    [Ignore]
    public double Porcentaje { get; set; }

    [Ignore]
    public string TotalTexto => Formato.Moneda(Total);

    [Ignore]
    public string Icono => string.IsNullOrEmpty(CategoriaIcono) ? "📦" : CategoriaIcono;

    [Ignore]
    public string Nombre => string.IsNullOrEmpty(CategoriaNombre) ? "Sin categoría" : CategoriaNombre;

    [Ignore]
    public string Color => string.IsNullOrEmpty(CategoriaColor) ? "#8A8294" : CategoriaColor;
}
