using Android.Widget;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;
using System.Collections.ObjectModel;

namespace GestorGastos.ViewModels;

public partial class MovimientosViewModel : BaseViewModel
{
    private readonly TransaccionRepository _transacciones;
    private DateTime _mes = Periodos.InicioMes(DateTime.Today);

    [ObservableProperty]
    private string mesTexto = Formato.MesAnio(DateTime.Today);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FiltroTodos))]
    [NotifyPropertyChangedFor(nameof(FiltroGastos))]
    [NotifyPropertyChangedFor(nameof(FiltroIngresos))]
    private string filtro = "Todos";

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private string ingresosTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string gastosTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string balanceTexto = Formato.Moneda(0);

    [ObservableProperty]
    private bool estaRefrescando;

    public bool FiltroTodos => Filtro == "Todos";

    public bool FiltroGastos => Filtro == "Gastos";

    public bool FiltroIngresos => Filtro == "Ingresos";

    public ObservableCollection<GrupoMovimientos> Grupos { get; } = new();

    public MovimientosViewModel(TransaccionRepository transacciones)
    {
        _transacciones = transacciones;
        Titulo = "Movimientos";
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        try
        {
            MesTexto = Formato.MesAnio(_mes);
            var filtroConsulta = new FiltroTransacciones
            {
                Desde = _mes,
                Hasta = _mes.AddMonths(1),
                Texto = TextoBusqueda,
                Tipo = Filtro switch
                {
                    "Gastos" => TiposMovimiento.Gasto,
                    "Ingresos" => TiposMovimiento.Ingreso,
                    _ => null
                }
            };

            var lista = await _transacciones.ObtenerAsync(filtroConsulta);
            var sinTransferencias = lista.Where(m => !m.EsTransferencia).ToList();
            var ingresos = sinTransferencias.Where(m => m.EsIngreso).Sum(m => m.Monto);
            var gastos = sinTransferencias.Where(m => !m.EsIngreso).Sum(m => m.Monto);
            IngresosTexto = $"+{Formato.Moneda(ingresos)}";
            GastosTexto = $"−{Formato.Moneda(gastos)}";
            BalanceTexto = Formato.Moneda(ingresos - gastos);

            var grupos = lista
                .GroupBy(m => m.Fecha.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new GrupoMovimientos(g.Key, g));
            Grupos.ReemplazarCon(grupos);
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("No se pudieron cargar los movimientos", ex.Message);
        }
        finally
        {
            EstaRefrescando = false;
        }
    }

    [RelayCommand]
    private async Task MesAnteriorAsync()
    {
        _mes = _mes.AddMonths(-1);
        await CargarAsync();
    }

    [RelayCommand]
    private async Task MesSiguienteAsync()
    {
        _mes = _mes.AddMonths(1);
        await CargarAsync();
    }

    [RelayCommand]
    private async Task CambiarFiltroAsync(string nuevoFiltro)
    {
        Filtro = nuevoFiltro;
        await CargarAsync();
    }

    [RelayCommand]
    private Task BuscarAsync() => CargarAsync();

    [RelayCommand]
    private async Task AbrirAsync(TransaccionDetalle? movimiento)
    {
        if (movimiento is null)
        {
            return;
        }

        await IrAAsync(Rutas.EditarMovimiento, new Dictionary<string, object> { { "Id", movimiento.Id } });
    }

    [RelayCommand]
    private Task VerCalendarioAsync() => IrAAsync(Rutas.Calendario);
}
