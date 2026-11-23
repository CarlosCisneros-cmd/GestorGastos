using GestorGastos.Helpers;

namespace GestorGastos.Models;

public class GrupoMovimientos : List<TransaccionDetalle>
{
    public GrupoMovimientos(DateTime fecha, IEnumerable<TransaccionDetalle> movimientos) : base(movimientos)
    {
        Fecha = fecha;
        Titulo = Formato.DiaTitulo(fecha);
        var balance = this.Where(m => !m.EsTransferencia)
            .Sum(m => m.Tipo == TiposMovimiento.Ingreso ? m.Monto : -m.Monto);
        TotalTexto = balance >= 0 ? $"+{Formato.Moneda(balance)}" : $"−{Formato.Moneda(Math.Abs(balance))}";
    }

    public DateTime Fecha { get; }

    public string Titulo { get; }

    public string TotalTexto { get; }
}
