using GestorGastos.Data;
using GestorGastos.Helpers;
using GestorGastos.Models;
using Plugin.LocalNotification;

namespace GestorGastos.Services;

public class AlertaPresupuestoService : IAlertaPresupuestoService
{
    private const double UmbralAdvertencia = 0.75;

    private readonly BaseDatos _bd;
    private readonly NotificacionService _notificaciones;

    public AlertaPresupuestoService(BaseDatos bd, NotificacionService notificaciones)
    {
        _bd = bd;
        _notificaciones = notificaciones;
    }

    public async Task VerificarAsync(int categoriaId, DateTime fecha)
    {
        if (!Preferencias.AlertasPresupuestoActivas)
        {
            return;
        }

        var conexion = await _bd.ConexionAsync();
        var presupuestos = await conexion.Table<Presupuesto>()
            .Where(p => p.Activo == true && p.CategoriaId == categoriaId)
            .ToListAsync();

        if (presupuestos.Count == 0)
        {
            return;
        }

        var categoria = await conexion.FindAsync<Categoria>(categoriaId);
        var nombreCategoria = categoria?.Nombre ?? "esta categoría";

        foreach (var presupuesto in presupuestos)
        {
            var (desde, hasta) = PresupuestoRepository.Rango(presupuesto.Periodo, DateTime.Today);
            if (fecha.Date < desde || fecha.Date >= hasta || presupuesto.Monto <= 0)
            {
                continue;
            }

            var gastado = await TransaccionRepository.SumarGastosCategoriaAsync(conexion, presupuesto.CategoriaId, desde, hasta);
            var porcentaje = (double)(gastado / presupuesto.Monto);
            var clave = PresupuestoRepository.ClavePeriodo(presupuesto.Periodo, DateTime.Today);
            var nombre = string.IsNullOrWhiteSpace(presupuesto.Nombre) ? nombreCategoria : presupuesto.Nombre;

            if (porcentaje >= 1 && gastado > presupuesto.Monto && presupuesto.AlertaExcedidoPeriodo != clave)
            {
                presupuesto.AlertaExcedidoPeriodo = clave;
                presupuesto.AlertaAdvertenciaPeriodo = clave;
                await conexion.UpdateAsync(presupuesto);

                await _notificaciones.MostrarAsync(
                    $"🚨 Presupuesto de {nombre} excedido",
                    $"Gastaste {Formato.Moneda(gastado)} de {Formato.Moneda(presupuesto.Monto)}. Te pasaste por {Formato.Moneda(gastado - presupuesto.Monto)}.");
            }
            else if (porcentaje >= UmbralAdvertencia && porcentaje < 1 && presupuesto.AlertaAdvertenciaPeriodo != clave)
            {
                presupuesto.AlertaAdvertenciaPeriodo = clave;
                await conexion.UpdateAsync(presupuesto);

                await _notificaciones.MostrarAsync(
                    $"⚠️ Has usado el {Math.Round(porcentaje * 100):0}% de {nombre}",
                    $"Llevas {Formato.Moneda(gastado)} de {Formato.Moneda(presupuesto.Monto)}. Te quedan {Formato.Moneda(presupuesto.Monto - gastado)}.");
            }
        }
    }
}
