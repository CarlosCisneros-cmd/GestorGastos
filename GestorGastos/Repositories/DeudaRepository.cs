using GestorGastos.Data;
using GestorGastos.Helpers;
using GestorGastos.Models;

namespace GestorGastos.Repositories;

public class DeudaRepository
{
    private readonly BaseDatos _bd;
    private readonly TransaccionRepository _transacciones;

    public DeudaRepository(BaseDatos bd, TransaccionRepository transacciones)
    {
        _bd = bd;
        _transacciones = transacciones;
    }

    public async Task<List<DeudaResumen>> ObtenerResumenesAsync()
    {
        var conexion = await _bd.ConexionAsync();
        var deudas = await conexion.Table<Deuda>()
            .Where(d => d.Activa == true)
            .OrderBy(d => d.FechaInicio)
            .ToListAsync();

        var resultado = new List<DeudaResumen>();
        foreach (var deuda in deudas)
        {
            var deudaId = deuda.Id;
            var cuotas = await conexion.Table<CuotaDeuda>()
                .Where(c => c.DeudaId == deudaId)
                .OrderBy(c => c.Numero)
                .ToListAsync();

            resultado.Add(new DeudaResumen
            {
                Deuda = deuda,
                CuotasPagadas = cuotas.Count(c => c.Estado == EstadosCuota.Pagada),
                ProximaCuota = cuotas.FirstOrDefault(c => c.Estado != EstadosCuota.Pagada),
                TotalAPagar = cuotas.Sum(c => c.Monto)
            });
        }

        return resultado.OrderBy(r => r.EstaPagada).ToList();
    }

    public async Task<Deuda?> ObtenerPorIdAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.FindAsync<Deuda>(id);
    }

    public async Task<List<CuotaDeuda>> ObtenerCuotasAsync(int deudaId)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.Table<CuotaDeuda>()
            .Where(c => c.DeudaId == deudaId)
            .OrderBy(c => c.Numero)
            .ToListAsync();
    }

    public static (decimal TotalFinal, decimal Interes, List<CuotaDeuda> Cuotas) CalcularPlan(
        decimal montoTotal, decimal tasaInteres, bool interesIncluido, int numeroCuotas, DateTime fechaInicio)
    {
        if (numeroCuotas <= 0)
        {
            throw new InvalidOperationException("El número de cuotas debe ser mayor a cero.");
        }

        if (montoTotal <= 0)
        {
            throw new InvalidOperationException("El monto de la deuda debe ser mayor a cero.");
        }

        var interes = interesIncluido ? 0 : Math.Round(montoTotal * tasaInteres / 100m, 2);
        var totalFinal = Math.Round(montoTotal + interes, 2);

        var totalCentavos = (long)Math.Round(totalFinal * 100);
        var baseCentavos = totalCentavos / numeroCuotas;
        var sobrante = totalCentavos % numeroCuotas;

        var cuotas = new List<CuotaDeuda>();
        for (var i = 0; i < numeroCuotas; i++)
        {
            var centavos = baseCentavos + (i < sobrante ? 1 : 0);
            var vencimiento = Periodos.SumarMeses(fechaInicio, i, fechaInicio.Day);
            cuotas.Add(new CuotaDeuda
            {
                Numero = i + 1,
                Monto = centavos / 100m,
                FechaVencimiento = vencimiento,
                Estado = vencimiento.Date <= DateTime.Today ? EstadosCuota.Pendiente : EstadosCuota.Programada
            });
        }

        return (totalFinal, interes, cuotas);
    }

    public async Task CrearAsync(Deuda deuda)
    {
        if (string.IsNullOrWhiteSpace(deuda.Nombre))
        {
            throw new InvalidOperationException("Escribe un nombre para la deuda.");
        }

        var (totalFinal, _, cuotas) = CalcularPlan(deuda.MontoTotal, deuda.TasaInteres, deuda.InteresIncluido, deuda.NumeroCuotas, deuda.FechaInicio);

        deuda.Nombre = deuda.Nombre.Trim();
        deuda.MontoTotal = Math.Round(deuda.MontoTotal, 2);
        deuda.MontoPendiente = totalFinal;
        deuda.Activa = true;
        deuda.FechaCreacion = DateTime.Now;

        var conexion = await _bd.ConexionAsync();
        await conexion.RunInTransactionAsync(db =>
        {
            db.Insert(deuda);
            foreach (var cuota in cuotas)
            {
                cuota.DeudaId = deuda.Id;
                db.Insert(cuota);
            }
        });
    }

    public async Task ActualizarDatosAsync(Deuda deuda)
    {
        var conexion = await _bd.ConexionAsync();
        var actual = await conexion.FindAsync<Deuda>(deuda.Id)
            ?? throw new InvalidOperationException("La deuda ya no existe.");

        actual.Nombre = deuda.Nombre.Trim();
        actual.Notas = deuda.Notas;
        actual.CuentaId = deuda.CuentaId;
        await conexion.UpdateAsync(actual);
    }

    public async Task PagarCuotaAsync(int cuotaId, int cuentaId)
    {
        var conexion = await _bd.ConexionAsync();
        var cuota = await conexion.FindAsync<CuotaDeuda>(cuotaId)
            ?? throw new InvalidOperationException("La cuota ya no existe.");

        if (cuota.Estado == EstadosCuota.Pagada)
        {
            throw new InvalidOperationException("Esta cuota ya está pagada.");
        }

        var deuda = await conexion.FindAsync<Deuda>(cuota.DeudaId)
            ?? throw new InvalidOperationException("La deuda ya no existe.");

        var categoria = await conexion.Table<Categoria>()
            .Where(c => c.Nombre == CategoriasEspeciales.PagoDeudas)
            .FirstOrDefaultAsync();

        var transaccion = new Transaccion
        {
            CuentaId = cuentaId,
            Tipo = TiposMovimiento.Gasto,
            Monto = cuota.Monto,
            Nombre = $"{deuda.Nombre} · cuota {cuota.Numero}/{deuda.NumeroCuotas}",
            CategoriaId = categoria?.Id,
            Fecha = DateTime.Today,
            EsPagoDeuda = true
        };
        var transaccionId = await _transacciones.GuardarAsync(transaccion);

        cuota.Estado = EstadosCuota.Pagada;
        cuota.FechaPago = DateTime.Today;
        cuota.TransaccionId = transaccionId;
        await conexion.UpdateAsync(cuota);

        deuda.MontoPendiente = Math.Max(0, Math.Round(deuda.MontoPendiente - cuota.Monto, 2));
        await conexion.UpdateAsync(deuda);
    }

    public async Task EliminarAsync(int deudaId)
    {
        var conexion = await _bd.ConexionAsync();
        var deuda = await conexion.FindAsync<Deuda>(deudaId);
        if (deuda is null)
        {
            return;
        }

        await conexion.ExecuteAsync("DELETE FROM CuotasDeuda WHERE DeudaId = ? AND Estado <> ?", deudaId, EstadosCuota.Pagada);
        deuda.Activa = false;
        await conexion.UpdateAsync(deuda);
    }

    public async Task ActualizarCuotasVencidasAsync()
    {
        var conexion = await _bd.ConexionAsync();
        await conexion.ExecuteAsync(
            "UPDATE CuotasDeuda SET Estado = ? WHERE Estado = ? AND FechaVencimiento <= ?",
            EstadosCuota.Pendiente, EstadosCuota.Programada, DateTime.Today);
    }
}
