using SQLite;

namespace GestorGastos.Models;

[Table("Usuarios")]
public class Usuario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string GoogleId { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string FotoUrl { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public DateTime UltimoIngreso { get; set; } = DateTime.Now;

    [Ignore]
    public string Inicial => string.IsNullOrWhiteSpace(Nombre) ? "U" : Nombre.Trim()[0].ToString().ToUpper();
}
