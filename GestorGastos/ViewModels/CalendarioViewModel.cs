using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class CalendarioViewModel : BaseViewModel
{
    private readonly TransaccionRepository _transacciones;
    private readonly ProximoPagoRepository _pagos;
    private DateTime _mes = Periodos.InicioMes(DateTime.Today);
    private List<TransaccionDetalle> _movimientosMes = new();
    private List<PagoProximoDetalle> _pagosMes = new();

    [ObservableProperty]
    private string mesTexto = Formato.MesAnio(DateTime.Today);

    [ObservableProperty]
    private string diaTexto = string.Empty;

    [ObservableProperty]
    private DiaCalendario? diaSeleccionado;

    // Se usan listas que se reemplazan completas (una sola actualización de pantalla)
    // en lugar de ObservableCollection, que redibuja el calendario día por día y lo vuelve lento.
    [ObservableProperty]
    private List<DiaCalendario> dias = new();

    [ObservableProperty]
    private List<TransaccionDetalle> movimientosDia = new();

    [ObservableProperty]
    private List<PagoProximoDetalle> pagosDia = new();

    public List<string> NombresDias { get; } = new() { "L", "M", "M", "J", "V", "S", "D" };

    public CalendarioViewModel(TransaccionRepository transacciones, ProximoPagoRepository pagos)
    {
        _transacciones = transacciones;
        _pagos = pagos;
        Titulo = "Calendario";
    }

    public async Task CargarAsync()
    {
        try
        {
            MesTexto = Formato.MesAnio(_mes);
            var fin = _mes.AddMonths(1);
            _movimientosMes = await _transacciones.ObtenerAsync(new FiltroTransacciones { Desde = _mes, Hasta = fin });
            _pagosMes = await _pagos.ObtenerEnRangoAsync(_mes, fin);

            // Se agrupa una sola vez por fecha en lugar de recorrer todos los movimientos por cada día
            var ingresosPorDia = _movimientosMes.Where(m => m.EsIngreso && !m.EsTransferencia).Select(m => m.Fecha.Date).ToHashSet();
            var gastosPorDia = _movimientosMes.Where(m => !m.EsIngreso && !m.EsTransferencia).Select(m => m.Fecha.Date).ToHashSet();
            var pagosPorDia = _pagosMes.Select(p => p.FechaVencimiento.Date).ToHashSet();

            var dias = new List<DiaCalendario>();
            var inicio = Periodos.InicioSemana(_mes);
            for (var i = 0; i < 42; i++)
            {
                var fecha = inicio.AddDays(i);
                var delMes = fecha.Month == _mes.Month && fecha.Year == _mes.Year;
                dias.Add(new DiaCalendario
                {
                    Fecha = fecha,
                    Fila = i / 7,
                    Columna = i % 7,
                    EsDelMes = delMes,
                    EsHoy = fecha == DateTime.Today,
                    TieneIngresos = delMes && ingresosPorDia.Contains(fecha),
                    TieneGastos = delMes && gastosPorDia.Contains(fecha),
                    TienePagos = delMes && pagosPorDia.Contains(fecha)
                });
            }

            Dias = dias;

            var seleccion = DiaSeleccionado?.Fecha;
            var aSeleccionar = Dias.FirstOrDefault(d => d.EsDelMes && d.Fecha == seleccion)
                ?? Dias.FirstOrDefault(d => d.EsHoy && d.EsDelMes)
                ?? Dias.First(d => d.EsDelMes);
            SeleccionarDia(aSeleccionar);
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("No se pudo cargar el calendario", ex.Message);
        }
    }

    [RelayCommand]
    private void SeleccionarDia(DiaCalendario? dia)
    {
        if (dia is null || !dia.EsDelMes)
        {
            return;
        }

        if (DiaSeleccionado is not null)
        {
            DiaSeleccionado.Seleccionado = false;
        }

        dia.Seleccionado = true;
        DiaSeleccionado = dia;
        DiaTexto = Formato.DiaTitulo(dia.Fecha);
        MovimientosDia = _movimientosMes.Where(m => m.Fecha.Date == dia.Fecha).ToList();
        PagosDia = _pagosMes.Where(p => p.FechaVencimiento.Date == dia.Fecha).ToList();
    }

    [RelayCommand]
    private async Task MesAnteriorAsync()
    {
        _mes = _mes.AddMonths(-1);
        DiaSeleccionado = null;
        await CargarAsync();
    }

    [RelayCommand]
    private async Task MesSiguienteAsync()
    {
        _mes = _mes.AddMonths(1);
        DiaSeleccionado = null;
        await CargarAsync();
    }

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
