using GestorGastos.Data;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;
using SQLite;

namespace GestorGastos.Repositories;

public class TransaccionRepository
{
    private const string ConsultaBase = @"
SELECT t.Id, t.CuentaId, t.Tipo, t.Monto, t.Nombre, t.Nota, t.CategoriaId, t.Fecha,
       t.EsRecurrente, t.EsPagoDeuda, t.EsTransferencia, t.GrupoTransferencia,
       IFNULL(c.Nombre, '') AS CategoriaNombre,
       IFNULL(c.Icono, '') AS CategoriaIcono,
       IFNULL(c.Color, '') AS CategoriaColor,
       IFNULL(a.Nombre, '') AS CuentaNombre,
       EXISTS(SELECT 1 FROM Comprobantes p WHERE p.TransaccionId = t.Id) AS TieneComprobante
FROM Transacciones t
LEFT JOIN Categorias c ON c.Id = t.CategoriaId
LEFT JOIN Cuentas a ON a.Id = t.CuentaId";

    private readonly BaseDatos _bd;
    private readonly IAlertaPresupuestoService _alertas;
    private readonly NotificacionService _notificaciones;

    public TransaccionRepository(BaseDatos bd, IAlertaPresupuestoService alertas, NotificacionService notificaciones)
    {
        _bd = bd;
        _alertas = alertas;
        _notificaciones = notificaciones;
    }

    public async Task<List<TransaccionDetalle>> ObtenerRecientesAsync(int cantidad)
    {
        var conexion = await _bd.ConexionAsync();
        var sql = ConsultaBase + " ORDER BY t.Fecha DESC, t.Id DESC LIMIT ?";
        return await conexion.QueryAsync<TransaccionDetalle>(sql, cantidad);
    }

    public async Task<List<TransaccionDetalle>> ObtenerAsync(FiltroTransacciones filtro)
    {
        var conexion = await _bd.ConexionAsync();
        var condiciones = new List<string> { "t.Fecha >= ?", "t.Fecha < ?" };
        var parametros = new List<object> { filtro.Desde.Date, filtro.Hasta.Date };

        if (filtro.CuentaId.HasValue)
        {
            condiciones.Add("t.CuentaId = ?");
            parametros.Add(filtro.CuentaId.Value);
        }

        if (filtro.CategoriaId.HasValue)
        {
            condiciones.Add("t.CategoriaId = ?");
            parametros.Add(filtro.CategoriaId.Value);
        }

        if (!string.IsNullOrEmpty(filtro.Tipo))
        {
            condiciones.Add("t.Tipo = ?");
            parametros.Add(filtro.Tipo);
        }

        if (!filtro.IncluirTransferencias)
        {
            condiciones.Add("t.EsTransferencia = 0");
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add("(t.Nombre LIKE ? OR t.Nota LIKE ?)");
            var patron = $"%{filtro.Texto.Trim()}%";
            parametros.Add(patron);
            parametros.Add(patron);
        }

        var sql = ConsultaBase + " WHERE " + string.Join(" AND ", condiciones) + " ORDER BY t.Fecha DESC, t.Id DESC";
        return await conexion.QueryAsync<TransaccionDetalle>(sql, parametros.ToArray());
    }

    public async Task<Transaccion?> ObtenerPorIdAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.FindAsync<Transaccion>(id);
    }

    public async Task<int> GuardarAsync(Transaccion transaccion)
    {
        if (transaccion.Monto <= 0)
        {
            throw new InvalidOperationException("El monto debe ser mayor a cero.");
        }

        if (transaccion.CuentaId == 0)
        {
            throw new InvalidOperationException("Selecciona una cuenta.");
        }

        if (transaccion.Tipo != TiposMovimiento.Gasto && transaccion.Tipo != TiposMovimiento.Ingreso)
        {
            throw new InvalidOperationException("El tipo de movimiento no es válido.");
        }

        if (transaccion.EsTransferencia)
        {
            throw new InvalidOperationException("Las transferencias no se editan. Elimínala y créala de nuevo.");
        }

        transaccion.Monto = Math.Round(transaccion.Monto, 2);
        transaccion.Nombre = string.IsNullOrWhiteSpace(transaccion.Nombre)
            ? await NombrePorDefectoAsync(transaccion)
            : transaccion.Nombre.Trim();

        var conexion = await _bd.ConexionAsync();

        // Validar que la cuenta tenga saldo suficiente
        var cuenta = await conexion.FindAsync<Cuenta>(transaccion.CuentaId)
            ?? throw new InvalidOperationException("La cuenta seleccionada ya no existe.");
        var saldoAntes = cuenta.Saldo;
        if (transaccion.Id != 0)
        {
            var guardada = await conexion.FindAsync<Transaccion>(transaccion.Id);
            if (guardada is not null && guardada.CuentaId == transaccion.CuentaId)
            {
                saldoAntes -= Efecto(guardada);
            }
        }
        var saldoDespues = saldoAntes + Efecto(transaccion);
        ValidarSaldoSuficiente(cuenta, saldoAntes, saldoDespues, transaccion.Monto);

        await conexion.RunInTransactionAsync(db =>
        {
            if (transaccion.Id == 0)
            {
                transaccion.FechaCreacion = DateTime.Now;
                db.Insert(transaccion);
            }
            else
            {
                var anterior = db.Find<Transaccion>(transaccion.Id);
                if (anterior is not null)
                {
                    AjustarSaldo(db, anterior.CuentaId, -Efecto(anterior));
                }
                db.Update(transaccion);
            }

            AjustarSaldo(db, transaccion.CuentaId, Efecto(transaccion));
        });

        if (transaccion.Tipo == TiposMovimiento.Gasto && transaccion.CategoriaId.HasValue)
        {
            try
            {
                await _alertas.VerificarAsync(transaccion.CategoriaId.Value, transaccion.Fecha);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al verificar presupuesto: {ex.Message}");
            }
        }

        await AvisarSaldoBajoAsync(cuenta, saldoAntes, saldoDespues);

        return transaccion.Id;
    }

    public async Task EliminarAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        var transaccion = await conexion.FindAsync<Transaccion>(id);
        if (transaccion is null)
        {
            return;
        }

        var aEliminar = new List<Transaccion> { transaccion };
        if (transaccion.EsTransferencia && !string.IsNullOrEmpty(transaccion.GrupoTransferencia))
        {
            var grupo = transaccion.GrupoTransferencia;
            aEliminar = await conexion.Table<Transaccion>()
                .Where(t => t.GrupoTransferencia == grupo)
                .ToListAsync();
        }

        var ids = aEliminar.Select(t => t.Id).ToList();
        var comprobantes = new List<Comprobante>();
        foreach (var transaccionId in ids)
        {
            var encontrados = await conexion.Table<Comprobante>()
                .Where(c => c.TransaccionId == transaccionId)
                .ToListAsync();
            comprobantes.AddRange(encontrados);
        }

        await conexion.RunInTransactionAsync(db =>
        {
            foreach (var item in aEliminar)
            {
                AjustarSaldo(db, item.CuentaId, -Efecto(item));
                db.Delete<Transaccion>(item.Id);
            }

            foreach (var comprobante in comprobantes)
            {
                db.Delete<Comprobante>(comprobante.Id);
            }
        });

        foreach (var comprobante in comprobantes)
        {
            try
            {
                if (File.Exists(comprobante.RutaImagen))
                {
                    File.Delete(comprobante.RutaImagen);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudo borrar la imagen: {ex.Message}");
            }
        }
    }

    public async Task RegistrarTransferenciaAsync(int cuentaOrigenId, int cuentaDestinoId, decimal monto, DateTime fecha, string nota)
    {
        if (cuentaOrigenId == 0 || cuentaDestinoId == 0)
        {
            throw new InvalidOperationException("Selecciona la cuenta de origen y la de destino.");
        }

        if (cuentaOrigenId == cuentaDestinoId)
        {
            throw new InvalidOperationException("La cuenta de origen y la de destino deben ser diferentes.");
        }

        if (monto <= 0)
        {
            throw new InvalidOperationException("El monto debe ser mayor a cero.");
        }

        var conexion = await _bd.ConexionAsync();
        var origen = await conexion.FindAsync<Cuenta>(cuentaOrigenId);
        var destino = await conexion.FindAsync<Cuenta>(cuentaDestinoId);
        if (origen is null || destino is null)
        {
            throw new InvalidOperationException("Una de las cuentas ya no existe.");
        }

        monto = Math.Round(monto, 2);
        ValidarSaldoSuficiente(origen, origen.Saldo, origen.Saldo - monto, monto);

        var categoria = await conexion.Table<Categoria>()
            .Where(c => c.Nombre == CategoriasEspeciales.Transferencia)
            .FirstOrDefaultAsync();

        var grupo = Guid.NewGuid().ToString("N");

        var salida = new Transaccion
        {
            CuentaId = origen.Id,
            Tipo = TiposMovimiento.Gasto,
            Monto = monto,
            Nombre = $"Transferencia a {destino.Nombre}",
            Nota = nota ?? string.Empty,
            CategoriaId = categoria?.Id,
            Fecha = fecha.Date,
            EsTransferencia = true,
            GrupoTransferencia = grupo
        };

        var entrada = new Transaccion
        {
            CuentaId = destino.Id,
            Tipo = TiposMovimiento.Ingreso,
            Monto = monto,
            Nombre = $"Transferencia desde {origen.Nombre}",
            Nota = nota ?? string.Empty,
            CategoriaId = categoria?.Id,
            Fecha = fecha.Date,
            EsTransferencia = true,
            GrupoTransferencia = grupo
        };

        await conexion.RunInTransactionAsync(db =>
        {
            db.Insert(salida);
            db.Insert(entrada);
            AjustarSaldo(db, origen.Id, -monto);
            AjustarSaldo(db, destino.Id, monto);
        });

        await AvisarSaldoBajoAsync(origen, origen.Saldo, origen.Saldo - monto);
    }

    public async Task<(decimal Ingresos, decimal Gastos)> TotalesDelMesAsync(DateTime mes)
    {
        var conexion = await _bd.ConexionAsync();
        var desde = new DateTime(mes.Year, mes.Month, 1);
        var hasta = desde.AddMonths(1);
        var ingresos = await SumarAsync(conexion, TiposMovimiento.Ingreso, desde, hasta);
        var gastos = await SumarAsync(conexion, TiposMovimiento.Gasto, desde, hasta);
        return (ingresos, gastos);
    }

    public async Task<List<GastoPorCategoria>> GastosPorCategoriaAsync(DateTime desde, DateTime hasta)
    {
        var conexion = await _bd.ConexionAsync();
        const string sql = @"
SELECT t.CategoriaId AS CategoriaId,
       IFNULL(c.Nombre, '') AS CategoriaNombre,
       IFNULL(c.Icono, '') AS CategoriaIcono,
       IFNULL(c.Color, '') AS CategoriaColor,
       SUM(t.Monto) AS Total
FROM Transacciones t
LEFT JOIN Categorias c ON c.Id = t.CategoriaId
WHERE t.Tipo = ? AND t.EsTransferencia = 0 AND t.Fecha >= ? AND t.Fecha < ?
GROUP BY t.CategoriaId
ORDER BY Total DESC";
        var lista = await conexion.QueryAsync<GastoPorCategoria>(sql, TiposMovimiento.Gasto, desde.Date, hasta.Date);
        foreach (var item in lista)
        {
            item.Total = Math.Round(item.Total, 2);
        }
        return lista;
    }

    public static async Task<decimal> SumarGastosCategoriaAsync(SQLiteAsyncConnection conexion, int categoriaId, DateTime desde, DateTime hasta)
    {
        const string sql = @"
SELECT IFNULL(SUM(Monto), 0) FROM Transacciones
WHERE Tipo = ? AND EsTransferencia = 0 AND CategoriaId = ? AND Fecha >= ? AND Fecha < ?";
        var total = await conexion.ExecuteScalarAsync<double>(sql, TiposMovimiento.Gasto, categoriaId, desde.Date, hasta.Date);
        return Math.Round((decimal)total, 2);
    }

    private static async Task<decimal> SumarAsync(SQLiteAsyncConnection conexion, string tipo, DateTime desde, DateTime hasta)
    {
        const string sql = @"
SELECT IFNULL(SUM(Monto), 0) FROM Transacciones
WHERE Tipo = ? AND EsTransferencia = 0 AND Fecha >= ? AND Fecha < ?";
        var total = await conexion.ExecuteScalarAsync<double>(sql, tipo, desde.Date, hasta.Date);
        return Math.Round((decimal)total, 2);
    }

    private async Task<string> NombrePorDefectoAsync(Transaccion transaccion)
    {
        if (transaccion.CategoriaId.HasValue)
        {
            var conexion = await _bd.ConexionAsync();
            var categoria = await conexion.FindAsync<Categoria>(transaccion.CategoriaId.Value);
            if (categoria is not null)
            {
                return categoria.Nombre;
            }
        }
        return transaccion.Tipo;
    }

    // Las tarjetas de crédito pueden quedar en negativo (es lo que se debe)
    private static bool PermiteSaldoNegativo(Cuenta cuenta) => cuenta.Tipo == TiposCuenta.TarjetaCredito;

    private static void ValidarSaldoSuficiente(Cuenta cuenta, decimal saldoAntes, decimal saldoDespues, decimal monto)
    {
        if (PermiteSaldoNegativo(cuenta) || saldoDespues >= 0 || saldoDespues >= saldoAntes)
        {
            return;
        }

        var disponible = Math.Max(saldoAntes, 0);
        throw new SaldoInsuficienteException(
            $"No tienes saldo suficiente en {cuenta.Nombre}.\n\n" +
            $"Disponible: {Formato.Moneda(disponible)}\n" +
            $"Monto que intentas usar: {Formato.Moneda(monto)}\n\n" +
            "Registra un ingreso, haz una transferencia a esta cuenta o elige otra cuenta.");
    }

    private async Task AvisarSaldoBajoAsync(Cuenta cuenta, decimal saldoAntes, decimal saldoDespues)
    {
        if (PermiteSaldoNegativo(cuenta) || saldoDespues >= saldoAntes)
        {
            return;
        }

        var umbral = Preferencias.SaldoMinimoAviso;
        if (saldoAntes >= umbral && saldoDespues < umbral)
        {
            await _notificaciones.MostrarAsync(
                $"⚠️ Saldo bajo en {cuenta.Nombre}",
                $"Te quedan {Formato.Moneda(saldoDespues)}. Estás gastando mucho, revisa tus gastos antes de seguir.");
        }
    }

    private static decimal Efecto(Transaccion transaccion)
    {
        return transaccion.Tipo == TiposMovimiento.Ingreso ? transaccion.Monto : -transaccion.Monto;
    }

    private static void AjustarSaldo(SQLiteConnection db, int cuentaId, decimal diferencia)
    {
        var cuenta = db.Find<Cuenta>(cuentaId);
        if (cuenta is null)
        {
            return;
        }

        cuenta.Saldo = Math.Round(cuenta.Saldo + diferencia, 2);
        db.Update(cuenta);
    }
}

public class SaldoInsuficienteException : InvalidOperationException
{
    public SaldoInsuficienteException(string mensaje) : base(mensaje)
    {
    }
}
