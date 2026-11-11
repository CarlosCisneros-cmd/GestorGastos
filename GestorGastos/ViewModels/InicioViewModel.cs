using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class InicioViewModel : BaseViewModel
{
    private readonly SesionService _sesion;
    private readonly ResumenRepository _resumen;
    private readonly CuentaRepository _cuentas;
    private readonly TransaccionRepository _transacciones;

    [ObservableProperty]
    private string saludo = "Hola";

    [ObservableProperty]
    private string inicial = "U";

    [ObservableProperty]
    private string disponibleTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string saldoTotalTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string comprometidoTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string ingresosTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string gastosTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string mesActual = Formato.MesAnio(DateTime.Today);

    [ObservableProperty]
    private bool estaRefrescando;

    public ObservableCollection<Cuenta> Cuentas { get; } = new();

    public ObservableCollection<TransaccionDetalle> Recientes { get; } = new();

    public ObservableCollection<PagoPendienteResumen> ProximosPagos { get; } = new();

    public ObservableCollection<GastoPorCategoria> GastosPorCategoria { get; } = new();

    public InicioViewModel(SesionService sesion, ResumenRepository resumen, CuentaRepository cuentas, TransaccionRepository transacciones)
    {
        _sesion = sesion;
        _resumen = resumen;
        _cuentas = cuentas;
        _transacciones = transacciones;
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        if (!_sesion.SesionIniciada)
        {
            return;
        }

        try
        {
            var usuario = _sesion.UsuarioActual!;
            var primerNombre = usuario.Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Usuario";
            Saludo = $"Hola, {primerNombre}";
            Inicial = usuario.Inicial;
            MesActual = Formato.MesAnio(DateTime.Today);

            var resumen = await _resumen.ObtenerResumenAsync();
            DisponibleTexto = Formato.Moneda(resumen.Disponible);
            SaldoTotalTexto = Formato.Moneda(resumen.SaldoTotal);
            ComprometidoTexto = Formato.Moneda(resumen.Comprometido);
            IngresosTexto = Formato.Moneda(resumen.IngresosMes);
            GastosTexto = Formato.Moneda(resumen.GastosMes);

            Cuentas.ReemplazarCon(await _cuentas.ObtenerActivasAsync());
            Recientes.ReemplazarCon(await _transacciones.ObtenerRecientesAsync(5));
            ProximosPagos.ReemplazarCon(await _resumen.ObtenerProximosPagosAsync(3));

            var hoy = DateTime.Today;
            var gastos = await _transacciones.GastosPorCategoriaAsync(Periodos.InicioMes(hoy), Periodos.InicioMesSiguiente(hoy));
            var maximo = gastos.Count == 0 ? 0 : gastos.Max(g => g.Total);
            foreach (var gasto in gastos)
            {
                gasto.Porcentaje = maximo == 0 ? 0 : (double)(gasto.Total / maximo);
            }
            GastosPorCategoria.ReemplazarCon(gastos.Take(5));
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("No se pudo cargar el inicio", ex.Message);
        }
        finally
        {
            EstaRefrescando = false;
        }
    }

    [RelayCommand]
    private Task VerCuentasAsync() => IrAAsync(Rutas.Cuentas);

    [RelayCommand]
    private Task NuevaCuentaAsync() => IrAAsync(Rutas.CuentaForm);

    [RelayCommand]
    private Task VerMovimientosAsync() => Shell.Current.GoToAsync($"//{Rutas.Movimientos}");

    [RelayCommand]
    private Task RegistrarAsync() => Shell.Current.GoToAsync($"//{Rutas.Registrar}");

    [RelayCommand]
    private Task VerProximosPagosAsync() => IrAAsync(Rutas.ProximosPagos);

    [RelayCommand]
    private Task VerAjustesAsync() => IrAAsync(Rutas.Ajustes);

    [RelayCommand]
    private async Task AbrirMovimientoAsync(TransaccionDetalle? movimiento)
    {
        if (movimiento is null)
        {
            return;
        }

        await IrAAsync(Rutas.EditarMovimiento, new Dictionary<string, object> { { "Id", movimiento.Id } });
    }
}
