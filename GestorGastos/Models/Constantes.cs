namespace GestorGastos.Models;

public static class TiposMovimiento
{
    public const string Gasto = "Gasto";
    public const string Ingreso = "Ingreso";
    public const string Transferencia = "Transferencia";
}

public static class TiposCategoria
{
    public const string Gasto = "Gasto";
    public const string Ingreso = "Ingreso";
    public const string Ambos = "Ambos";
}

public static class TiposCuenta
{
    public const string Banco = "Banco";
    public const string Efectivo = "Efectivo";
    public const string Ahorros = "Ahorros";
    public const string TarjetaCredito = "Tarjeta de crédito";
    public const string Otra = "Otra";

    public static readonly List<string> Todos = new() { Banco, Efectivo, Ahorros, TarjetaCredito, Otra };

    public static string IconoPorDefecto(string tipo) => tipo switch
    {
        Banco => "🏦",
        Efectivo => "💵",
        Ahorros => "🐷",
        TarjetaCredito => "💳",
        _ => "👛"
    };
}

public static class Frecuencias
{
    public const string Diaria = "Diaria";
    public const string Semanal = "Semanal";
    public const string Quincenal = "Quincenal";
    public const string Mensual = "Mensual";
    public const string Anual = "Anual";

    public static readonly List<string> Todas = new() { Diaria, Semanal, Quincenal, Mensual, Anual };
}

public static class PeriodosPresupuesto
{
    public const string Mensual = "Mensual";
    public const string Semanal = "Semanal";

    public static readonly List<string> Todos = new() { Mensual, Semanal };
}

public static class EstadosCuota
{
    public const string Programada = "Programada";
    public const string Pendiente = "Pendiente";
    public const string Pagada = "Pagada";
}

public static class CategoriasEspeciales
{
    public const string Transferencia = "Transferencia";
    public const string PagoDeudas = "Pago de deudas";
    public const string Ahorro = "Ahorro";
}
