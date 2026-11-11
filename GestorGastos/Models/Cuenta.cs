using GestorGastos.Helpers;
using SQLite;

namespace GestorGastos.Models;

[Table("Cuentas")]
public class Cuenta
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(60)]
    public string Nombre { get; set; } = string.Empty;

    public string Tipo { get; set; } = TiposCuenta.Banco;

    public decimal Saldo { get; set; }

    public string Moneda { get; set; } = "USD";

    public string Color { get; set; } = "#7C5CDB";

    public string Icono { get; set; } = "🏦";

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Ignore]
    public string SaldoTexto => Formato.Moneda(Saldo);

    [Ignore]
    public string NombreConIcono => $"{Icono} {Nombre}";

    public override string ToString() => NombreConIcono;
}
