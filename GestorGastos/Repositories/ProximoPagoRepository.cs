using GestorGastos.Data;
using GestorGastos.Models;

namespace GestorGastos.Repositories;

public class ProximoPagoRepository
{
    private readonly BaseDatos _bd;
    private readonly TransaccionRepository _transacciones;
    private readonly RecurrenteRepository _recurrentes;

    public ProximoPagoRepository(BaseDatos bd, TransaccionRepository transacciones, RecurrenteRepository recurrentes)
    {
        _bd = bd;
        _transacciones = transacciones;
        _recurrentes = recurrentes;
    }

    public async Task<List<PagoProximoDetalle>> ObtenerPendientesAsync()
    {
        var conexion = await _bd.ConexionAsync();
        const string sql = @"
SELECT p.Id, p.CuentaId, p.Tipo, p.Monto, p.Nombre, p.CategoriaId, p.FechaVencimiento, p.Pagado, p.RecurrenteId,
       IFNULL(c.Icono, '') AS CategoriaIcono,
       IFNULL(a.Nombre, '') AS CuentaNombre
FROM PagosProximos p
LEFT JOIN Categorias c ON c.Id = p.CategoriaId
LEFT JOIN Cuentas a ON a.Id = p.CuentaId
WHERE p.Pagado = 0
ORDER BY p.FechaVencimiento";
        return await conexion.QueryAsync<PagoProximoDetalle>(sql);
    }

    public async Task<List<PagoProximoDetalle>> ObtenerEnRangoAsync(DateTime desde, DateTime hasta)
    {
        var conexion = await _bd.ConexionAsync();
        const string sql = @"
SELECT p.Id, p.CuentaId, p.Tipo, p.Monto, p.Nombre, p.CategoriaId, p.FechaVencimiento, p.Pagado, p.RecurrenteId,
       IFNULL(c.Icono, '') AS CategoriaIcono,
       IFNULL(a.Nombre, '') AS CuentaNombre
FROM PagosProximos p
LEFT JOIN Categorias c ON c.Id = p.CategoriaId
LEFT JOIN Cuentas a ON a.Id = p.CuentaId
WHERE p.Pagado = 0 AND p.FechaVencimiento >= ? AND p.FechaVencimiento < ?
ORDER BY p.FechaVencimiento";
        return await conexion.QueryAsync<PagoProximoDetalle>(sql, desde.Date, hasta.Date);
    }

    public async Task<PagoProximo?> ObtenerPorIdAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.FindAsync<PagoProximo>(id);
    }

    public async Task GuardarAsync(PagoProximo pago)
    {
        if (string.IsNullOrWhiteSpace(pago.Nombre))
        {
            throw new InvalidOperationException("Escribe un nombre para el pago.");
        }

        if (pago.Monto <= 0)
        {
            throw new InvalidOperationException("El monto debe ser mayor a cero.");
        }

        if (pago.CuentaId == 0)
        {
            throw new InvalidOperationException("Selecciona una cuenta.");
        }

        pago.Nombre = pago.Nombre.Trim();
        pago.Monto = Math.Round(pago.Monto, 2);
        pago.FechaVencimiento = pago.FechaVencimiento.Date;

        var conexion = await _bd.ConexionAsync();
        if (pago.Id == 0)
        {
            pago.FechaCreacion = DateTime.Now;
            await conexion.InsertAsync(pago);
        }
        else
        {
            await conexion.UpdateAsync(pago);
        }
    }

    public async Task EliminarAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        await conexion.DeleteAsync<PagoProximo>(id);
    }

    public async Task MarcarPagadoAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        var pago = await conexion.FindAsync<PagoProximo>(id);
        if (pago is null || pago.Pagado)
        {
            return;
        }

        if (pago.RecurrenteId.HasValue)
        {
            var recurrente = await _recurrentes.ObtenerPorIdAsync(pago.RecurrenteId.Value);
            if (recurrente is not null)
            {
                await _recurrentes.RegistrarOcurrenciaAsync(recurrente, pago.FechaVencimiento);
                return;
            }
        }

        var transaccion = new Transaccion
        {
            CuentaId = pago.CuentaId,
            Tipo = pago.Tipo,
            Monto = pago.Monto,
            Nombre = pago.Nombre,
            Nota = pago.Nota,
            CategoriaId = pago.CategoriaId,
            Fecha = DateTime.Today
        };
        await _transacciones.GuardarAsync(transaccion);

        pago.Pagado = true;
        pago.FechaPago = DateTime.Today;
        await conexion.UpdateAsync(pago);
    }
}
