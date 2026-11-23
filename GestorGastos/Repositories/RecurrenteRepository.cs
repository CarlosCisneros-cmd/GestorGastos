using GestorGastos.Data;
using GestorGastos.Helpers;
using GestorGastos.Models;

namespace GestorGastos.Repositories;

public class RecurrenteRepository
{
    private readonly BaseDatos _bd;
    private readonly TransaccionRepository _transacciones;

    public RecurrenteRepository(BaseDatos bd, TransaccionRepository transacciones)
    {
        _bd = bd;
        _transacciones = transacciones;
    }

    public async Task<List<RecurrenteDetalle>> ObtenerTodosAsync()
    {
        var conexion = await _bd.ConexionAsync();
        const string sql = @"
SELECT r.Id, r.Tipo, r.Monto, r.Nombre, r.Frecuencia, r.ProximaFecha, r.Activo,
       IFNULL(c.Icono, '') AS CategoriaIcono,
       IFNULL(a.Nombre, '') AS CuentaNombre
FROM PagosRecurrentes r
LEFT JOIN Categorias c ON c.Id = r.CategoriaId
LEFT JOIN Cuentas a ON a.Id = r.CuentaId
ORDER BY r.Activo DESC, r.ProximaFecha";
        return await conexion.QueryAsync<RecurrenteDetalle>(sql);
    }

    public async Task<PagoRecurrente?> ObtenerPorIdAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.FindAsync<PagoRecurrente>(id);
    }

    public async Task GuardarAsync(PagoRecurrente recurrente)
    {
        if (string.IsNullOrWhiteSpace(recurrente.Nombre))
        {
            throw new InvalidOperationException("Escribe un nombre para el pago recurrente.");
        }

        if (recurrente.Monto <= 0)
        {
            throw new InvalidOperationException("El monto debe ser mayor a cero.");
        }

        if (recurrente.CuentaId == 0)
        {
            throw new InvalidOperationException("Selecciona una cuenta.");
        }

        if (recurrente.FechaFin.HasValue && recurrente.FechaFin.Value.Date < recurrente.FechaInicio.Date)
        {
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        }

        recurrente.Nombre = recurrente.Nombre.Trim();
        recurrente.Monto = Math.Round(recurrente.Monto, 2);
        var conexion = await _bd.ConexionAsync();

        if (recurrente.Id == 0)
        {
            recurrente.ProximaFecha = recurrente.FechaInicio.Date;
            recurrente.Activo = true;
            recurrente.FechaCreacion = DateTime.Now;
            await conexion.InsertAsync(recurrente);
        }
        else
        {
            await conexion.UpdateAsync(recurrente);
        }

        await RegenerarPagoPendienteAsync(recurrente);
    }

    public async Task EliminarAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        await conexion.ExecuteAsync("DELETE FROM PagosProximos WHERE RecurrenteId = ? AND Pagado = 0", id);
        await conexion.DeleteAsync<PagoRecurrente>(id);
    }

    public async Task CambiarEstadoAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        var recurrente = await conexion.FindAsync<PagoRecurrente>(id);
        if (recurrente is null)
        {
            return;
        }

        recurrente.Activo = !recurrente.Activo;
        if (recurrente.Activo && recurrente.ProximaFecha.Date < DateTime.Today)
        {
            recurrente.ProximaFecha = DateTime.Today;
        }

        await conexion.UpdateAsync(recurrente);
        await RegenerarPagoPendienteAsync(recurrente);
    }

    public async Task<int> ProcesarVencidosAsync()
    {
        var conexion = await _bd.ConexionAsync();
        var hoy = DateTime.Today;
        var vencidos = await conexion.Table<PagoRecurrente>()
            .Where(r => r.Activo == true && r.ProximaFecha <= hoy)
            .ToListAsync();

        var generados = 0;
        foreach (var recurrente in vencidos)
        {
            var vueltas = 0;
            while (recurrente.Activo && recurrente.ProximaFecha.Date <= hoy && vueltas < 60)
            {
                await RegistrarOcurrenciaAsync(recurrente, recurrente.ProximaFecha);
                generados++;
                vueltas++;
            }
        }
        return generados;
    }

    public async Task RegistrarOcurrenciaAsync(PagoRecurrente recurrente, DateTime fecha)
    {
        var conexion = await _bd.ConexionAsync();

        var transaccion = new Transaccion
        {
            CuentaId = recurrente.CuentaId,
            Tipo = recurrente.Tipo,
            Monto = recurrente.Monto,
            Nombre = recurrente.Nombre,
            Nota = recurrente.Nota,
            CategoriaId = recurrente.CategoriaId,
            Fecha = fecha.Date,
            EsRecurrente = true,
            RecurrenteId = recurrente.Id
        };
        await _transacciones.GuardarAsync(transaccion);

        await conexion.ExecuteAsync(
            "UPDATE PagosProximos SET Pagado = 1, FechaPago = ? WHERE RecurrenteId = ? AND Pagado = 0",
            DateTime.Today, recurrente.Id);

        recurrente.ProximaFecha = SiguienteFecha(fecha, recurrente.Frecuencia, recurrente.FechaInicio.Day);
        if (recurrente.FechaFin.HasValue && recurrente.ProximaFecha.Date > recurrente.FechaFin.Value.Date)
        {
            recurrente.Activo = false;
        }

        await conexion.UpdateAsync(recurrente);
        await RegenerarPagoPendienteAsync(recurrente);
    }

    public static DateTime SiguienteFecha(DateTime fecha, string frecuencia, int diaOriginal)
    {
        return frecuencia switch
        {
            Frecuencias.Diaria => fecha.Date.AddDays(1),
            Frecuencias.Semanal => fecha.Date.AddDays(7),
            Frecuencias.Quincenal => fecha.Date.AddDays(15),
            Frecuencias.Anual => Periodos.SumarMeses(fecha, 12, diaOriginal),
            _ => Periodos.SumarMeses(fecha, 1, diaOriginal)
        };
    }

    private async Task RegenerarPagoPendienteAsync(PagoRecurrente recurrente)
    {
        var conexion = await _bd.ConexionAsync();
        await conexion.ExecuteAsync("DELETE FROM PagosProximos WHERE RecurrenteId = ? AND Pagado = 0", recurrente.Id);

        if (!recurrente.Activo)
        {
            return;
        }

        var pago = new PagoProximo
        {
            CuentaId = recurrente.CuentaId,
            Tipo = recurrente.Tipo,
            Monto = recurrente.Monto,
            Nombre = recurrente.Nombre,
            Nota = recurrente.Nota,
            CategoriaId = recurrente.CategoriaId,
            FechaVencimiento = recurrente.ProximaFecha.Date,
            RecurrenteId = recurrente.Id
        };
        await conexion.InsertAsync(pago);
    }
}
