using SQLite;

namespace GestorGastos.Models;

[Table("Categorias")]
public class Categoria
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(40)]
    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = TiposCategoria.Gasto;

    public string Icono { get; set; } = "📦";

    public string Color { get; set; } = "#8A8294";

    public bool EsPredeterminada { get; set; }

    public bool Activa { get; set; } = true;

    [Ignore]
    public string NombreConIcono => $"{Icono} {Nombre}";

    public override string ToString() => NombreConIcono;
}
