using GestorGastos.Data;
using GestorGastos.Models;

namespace GestorGastos.Repositories;

public class CuentaRepository
{
    private readonly BaseDatos _bd;

    public CuentaRepository(BaseDatos bd)
    {
        _bd = bd;
    }

    public async Task<List<Cuenta>> ObtenerActivasAsync()
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.Table<Cuenta>()
            .Where(c => c.Activa == true)
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<Cuenta?> ObtenerPorIdAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.FindAsync<Cuenta>(id);
    }

    public async Task<decimal> SaldoTotalAsync()
    {
        var cuentas = await ObtenerActivasAsync();
        return cuentas.Sum(c => c.Saldo);
    }

    public async Task GuardarAsync(Cuenta cuenta)
    {
        if (string.IsNullOrWhiteSpace(cuenta.Nombre))
        {
            throw new InvalidOperationException("La cuenta necesita un nombre.");
        }

        cuenta.Nombre = cuenta.Nombre.Trim();
        var conexion = await _bd.ConexionAsync();

        if (cuenta.Id == 0)
        {
            cuenta.FechaCreacion = DateTime.Now;
            await conexion.InsertAsync(cuenta);
        }
        else
        {
            var actual = await conexion.FindAsync<Cuenta>(cuenta.Id);
            if (actual is null)
            {
                throw new InvalidOperationException("La cuenta ya no existe.");
            }

            actual.Nombre = cuenta.Nombre;
            actual.Tipo = cuenta.Tipo;
            actual.Moneda = cuenta.Moneda;
            actual.Color = cuenta.Color;
            actual.Icono = cuenta.Icono;
            await conexion.UpdateAsync(actual);
        }
    }

    public async Task DesactivarAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        var cuenta = await conexion.FindAsync<Cuenta>(id);
        if (cuenta is null)
        {
            return;
        }

        cuenta.Activa = false;
        await conexion.UpdateAsync(cuenta);
    }
}
