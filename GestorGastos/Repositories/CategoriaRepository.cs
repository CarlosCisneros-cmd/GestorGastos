using GestorGastos.Data;
using GestorGastos.Models;

namespace GestorGastos.Repositories;

public class CategoriaRepository
{
    private readonly BaseDatos _bd;

    public CategoriaRepository(BaseDatos bd)
    {
        _bd = bd;
    }

    public async Task<List<Categoria>> ObtenerActivasAsync()
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.Table<Categoria>()
            .Where(c => c.Activa == true)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<List<Categoria>> ObtenerPorTipoAsync(string tipo)
    {
        var activas = await ObtenerActivasAsync();
        return activas
            .Where(c => c.Tipo == tipo && c.Nombre != CategoriasEspeciales.Transferencia)
            .ToList();
    }

    public async Task<Categoria?> ObtenerPorIdAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.FindAsync<Categoria>(id);
    }

    public async Task<Categoria?> ObtenerPorNombreAsync(string nombre)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.Table<Categoria>()
            .Where(c => c.Nombre == nombre)
            .FirstOrDefaultAsync();
    }

    public async Task GuardarAsync(Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
        {
            throw new InvalidOperationException("La categoría necesita un nombre.");
        }

        categoria.Nombre = categoria.Nombre.Trim();
        var conexion = await _bd.ConexionAsync();

        var repetida = await conexion.Table<Categoria>()
            .Where(c => c.Nombre == categoria.Nombre && c.Tipo == categoria.Tipo && c.Id != categoria.Id && c.Activa == true)
            .CountAsync();
        if (repetida > 0)
        {
            throw new InvalidOperationException("Ya existe una categoría con ese nombre.");
        }

        if (categoria.Id == 0)
        {
            await conexion.InsertAsync(categoria);
        }
        else
        {
            await conexion.UpdateAsync(categoria);
        }
    }

    public async Task DesactivarAsync(Categoria categoria)
    {
        if (EsProtegida(categoria))
        {
            throw new InvalidOperationException("Esta categoría la usa el sistema y no se puede eliminar.");
        }

        var conexion = await _bd.ConexionAsync();
        categoria.Activa = false;
        await conexion.UpdateAsync(categoria);
    }

    public static bool EsProtegida(Categoria categoria)
    {
        return categoria.Nombre == CategoriasEspeciales.Transferencia
            || categoria.Nombre == CategoriasEspeciales.PagoDeudas
            || categoria.Nombre == CategoriasEspeciales.Ahorro;
    }
}
