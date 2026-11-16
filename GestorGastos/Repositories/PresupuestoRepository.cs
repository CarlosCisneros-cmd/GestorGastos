using GestorGastos.Data;
using GestorGastos.Helpers;
using GestorGastos.Models;

namespace GestorGastos.Repositories;

public class PresupuestoRepository
{
    private readonly BaseDatos _bd;

    public PresupuestoRepository(BaseDatos bd)
    {
        _bd = bd;
    }

    public async Task<List<PresupuestoProgreso>> ObtenerConProgresoAsync(DateTime fecha)
    {
        var conexion = await _bd.ConexionAsync();
        var presupuestos = await conexion.Table<Presupuesto>()
            .Where(p => p.Activo == true)
            .ToListAsync();
        var categorias = await conexion.Table<Categoria>().ToListAsync();

        var resultado = new List<PresupuestoProgreso>();
        foreach (var presupuesto in presupuestos)
        {
            var (desde, hasta) = Rango(presupuesto.Periodo, fecha);
            var gastado = await TransaccionRepository.SumarGastosCategoriaAsync(conexion, presupuesto.CategoriaId, desde, hasta);
            var categoria = categorias.FirstOrDefault(c => c.Id == presupuesto.CategoriaId);
            resultado.Add(new PresupuestoProgreso
            {
                Presupuesto = presupuesto,
                CategoriaNombre = categoria?.Nombre ?? "Sin categoría",
                CategoriaIcono = categoria?.Icono ?? "📦",
                Gastado = gastado
            });
        }

        return resultado.OrderByDescending(p => p.Porcentaje).ToList();
    }

    public async Task<Presupuesto?> ObtenerPorIdAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.FindAsync<Presupuesto>(id);
    }

    public async Task GuardarAsync(Presupuesto presupuesto)
    {
        if (presupuesto.CategoriaId == 0)
        {
            throw new InvalidOperationException("Elige la categoría del presupuesto.");
        }

        if (presupuesto.Monto <= 0)
        {
            throw new InvalidOperationException("El límite debe ser mayor a cero.");
        }

        var conexion = await _bd.ConexionAsync();
        var categoriaId = presupuesto.CategoriaId;
        var periodo = presupuesto.Periodo;
        var id = presupuesto.Id;
        var repetido = await conexion.Table<Presupuesto>()
            .Where(p => p.Activo == true && p.CategoriaId == categoriaId && p.Periodo == periodo && p.Id != id)
            .CountAsync();
        if (repetido > 0)
        {
            throw new InvalidOperationException("Ya existe un presupuesto para esa categoría y período.");
        }

        presupuesto.Monto = Math.Round(presupuesto.Monto, 2);
        presupuesto.Nombre = presupuesto.Nombre?.Trim() ?? string.Empty;

        if (presupuesto.Id == 0)
        {
            presupuesto.FechaCreacion = DateTime.Now;
            await conexion.InsertAsync(presupuesto);
        }
        else
        {
            presupuesto.AlertaAdvertenciaPeriodo = string.Empty;
            presupuesto.AlertaExcedidoPeriodo = string.Empty;
            await conexion.UpdateAsync(presupuesto);
        }
    }

    public async Task EliminarAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        await conexion.DeleteAsync<Presupuesto>(id);
    }

    public static (DateTime Desde, DateTime Hasta) Rango(string periodo, DateTime fecha)
    {
        if (periodo == PeriodosPresupuesto.Semanal)
        {
            var inicio = Periodos.InicioSemana(fecha);
            return (inicio, inicio.AddDays(7));
        }

        var mes = Periodos.InicioMes(fecha);
        return (mes, mes.AddMonths(1));
    }

    public static string ClavePeriodo(string periodo, DateTime fecha)
    {
        var (desde, _) = Rango(periodo, fecha);
        return periodo == PeriodosPresupuesto.Semanal ? $"S{desde:yyyyMMdd}" : $"M{desde:yyyyMM}";
    }
}
