using GestorGastos.Data;
using GestorGastos.Helpers;
using GestorGastos.Models;

namespace GestorGastos.Repositories;

public class ResumenRepository
{
    private readonly BaseDatos _bd;
    private readonly TransaccionRepository _transacciones;

    public ResumenRepository(BaseDatos bd, TransaccionRepository transacciones)
    {
        _bd = bd;
        _transacciones = transacciones;
    }

    public async Task<ResumenInicio> ObtenerResumenAsync()
    {
        var conexion = await _bd.ConexionAsync();
        var hoy = DateTime.Today;
        var finPeriodo = Periodos.InicioMesSiguiente(hoy);

        var cuentas = await conexion.Table<Cuenta>().Where(c => c.Activa == true).ToListAsync();
        var saldoTotal = cuentas.Sum(c => c.Saldo);

        var pagos = await conexion.Table<PagoProximo>()
            .Where(p => p.Pagado == false && p.FechaVencimiento < finPeriodo)
            .ToListAsync();
        var comprometidoPagos = pagos.Where(p => p.Tipo == TiposMovimiento.Gasto).Sum(p => p.Monto);

        var cuotas = await conexion.Table<CuotaDeuda>()
            .Where(c => c.Estado != EstadosCuota.Pagada && c.FechaVencimiento < finPeriodo)
            .ToListAsync();
        var comprometidoCuotas = cuotas.Sum(c => c.Monto);

        var (ingresos, gastos) = await _transacciones.TotalesDelMesAsync(hoy);
        var comprometido = comprometidoPagos + comprometidoCuotas;

        return new ResumenInicio
        {
            SaldoTotal = saldoTotal,
            Comprometido = comprometido,
            Disponible = saldoTotal - comprometido,
            IngresosMes = ingresos,
            GastosMes = gastos
        };
    }

    public async Task<List<PagoPendienteResumen>> ObtenerProximosPagosAsync(int cantidad)
    {
        var conexion = await _bd.ConexionAsync();
        var resultado = new List<PagoPendienteResumen>();

        var pagos = await conexion.Table<PagoProximo>()
            .Where(p => p.Pagado == false)
            .OrderBy(p => p.FechaVencimiento)
            .Take(cantidad)
            .ToListAsync();

        foreach (var pago in pagos)
        {
            resultado.Add(new PagoPendienteResumen
            {
                Nombre = pago.Nombre,
                Monto = pago.Monto,
                FechaVencimiento = pago.FechaVencimiento,
                Icono = pago.Tipo == TiposMovimiento.Ingreso ? "💰" : "📅",
                Origen = "Pago"
            });
        }

        const string sqlCuotas = @"
SELECT d.Nombre || ' (cuota ' || c.Numero || '/' || d.NumeroCuotas || ')' AS Nombre,
       c.Monto AS Monto,
       c.FechaVencimiento AS FechaVencimiento,
       '💳' AS Icono,
       'Cuota' AS Origen
FROM CuotasDeuda c
INNER JOIN Deudas d ON d.Id = c.DeudaId
WHERE c.Estado <> ? AND d.Activa = 1
ORDER BY c.FechaVencimiento
LIMIT ?";
        var cuotas = await conexion.QueryAsync<PagoPendienteResumen>(sqlCuotas, EstadosCuota.Pagada, cantidad);
        resultado.AddRange(cuotas);

        return resultado
            .OrderBy(p => p.FechaVencimiento)
            .Take(cantidad)
            .ToList();
    }
}
