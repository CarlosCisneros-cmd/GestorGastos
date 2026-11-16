using GestorGastos.Data;
using GestorGastos.Models;

namespace GestorGastos.Repositories;

public class AhorroRepository
{
    private readonly BaseDatos _bd;
    private readonly TransaccionRepository _transacciones;

    public AhorroRepository(BaseDatos bd, TransaccionRepository transacciones)
    {
        _bd = bd;
        _transacciones = transacciones;
    }

    public async Task<List<MetaAhorro>> ObtenerActivasAsync()
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.Table<MetaAhorro>()
            .Where(m => m.Activa == true)
            .OrderBy(m => m.FechaCreacion)
            .ToListAsync();
    }

    public async Task<MetaAhorro?> ObtenerPorIdAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.FindAsync<MetaAhorro>(id);
    }

    public async Task GuardarAsync(MetaAhorro meta)
    {
        if (string.IsNullOrWhiteSpace(meta.Nombre))
        {
            throw new InvalidOperationException("Escribe un nombre para la meta.");
        }

        if (meta.MontoObjetivo <= 0)
        {
            throw new InvalidOperationException("El monto objetivo debe ser mayor a cero.");
        }

        meta.Nombre = meta.Nombre.Trim();
        meta.MontoObjetivo = Math.Round(meta.MontoObjetivo, 2);

        var conexion = await _bd.ConexionAsync();
        if (meta.Id == 0)
        {
            meta.FechaCreacion = DateTime.Now;
            await conexion.InsertAsync(meta);
        }
        else
        {
            await conexion.UpdateAsync(meta);
        }
    }

    public async Task EliminarAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        var meta = await conexion.FindAsync<MetaAhorro>(id);
        if (meta is null)
        {
            return;
        }

        meta.Activa = false;
        await conexion.UpdateAsync(meta);
    }

    public async Task AportarAsync(int metaId, int cuentaId, decimal monto)
    {
        if (monto <= 0)
        {
            throw new InvalidOperationException("El aporte debe ser mayor a cero.");
        }

        var conexion = await _bd.ConexionAsync();
        var meta = await conexion.FindAsync<MetaAhorro>(metaId)
            ?? throw new InvalidOperationException("La meta ya no existe.");
        var cuenta = await conexion.FindAsync<Cuenta>(cuentaId)
            ?? throw new InvalidOperationException("Selecciona una cuenta válida.");

        if (cuenta.Saldo < monto)
        {
            throw new InvalidOperationException($"La cuenta {cuenta.Nombre} no tiene saldo suficiente.");
        }

        var categoria = await conexion.Table<Categoria>()
            .Where(c => c.Nombre == CategoriasEspeciales.Ahorro)
            .FirstOrDefaultAsync();

        var transaccion = new Transaccion
        {
            CuentaId = cuentaId,
            Tipo = TiposMovimiento.Gasto,
            Monto = monto,
            Nombre = $"Ahorro: {meta.Nombre}",
            CategoriaId = categoria?.Id,
            Fecha = DateTime.Today
        };
        var transaccionId = await _transacciones.GuardarAsync(transaccion);

        await conexion.InsertAsync(new AporteAhorro
        {
            MetaId = metaId,
            CuentaId = cuentaId,
            TransaccionId = transaccionId,
            Monto = Math.Round(monto, 2),
            Fecha = DateTime.Today
        });

        meta.MontoActual = Math.Round(meta.MontoActual + monto, 2);
        await conexion.UpdateAsync(meta);
    }
}
