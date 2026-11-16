using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class DeudaDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly DeudaRepository _deudas;
    private readonly CuentaRepository _cuentas;
    private Deuda? _deuda;
    private int _id;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string pendienteTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string resumenTexto = string.Empty;

    [ObservableProperty]
    private double progreso;

    [ObservableProperty]
    private string notas = string.Empty;

    public ObservableCollection<CuotaItem> Cuotas { get; } = new();

    public DeudaDetalleViewModel(DeudaRepository deudas, CuentaRepository cuentas)
    {
        _deudas = deudas;
        _cuentas = cuentas;
        Titulo = "Detalle de la deuda";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var valor) && valor is int id)
        {
            _id = id;
        }
    }

    public async Task CargarAsync()
    {
        _deuda = await _deudas.ObtenerPorIdAsync(_id);
        if (_deuda is null)
        {
            await VolverAsync();
            return;
        }

        var cuotas = await _deudas.ObtenerCuotasAsync(_id);
        Cuotas.ReemplazarCon(cuotas.Select(c => new CuotaItem(c, _deuda.NumeroCuotas)));

        var total = cuotas.Sum(c => c.Monto);
        var pagadas = cuotas.Count(c => c.Estado == EstadosCuota.Pagada);
        Nombre = _deuda.Nombre;
        PendienteTexto = Formato.Moneda(_deuda.MontoPendiente);
        Progreso = total <= 0 ? 0 : (double)((total - _deuda.MontoPendiente) / total);
        ResumenTexto = $"{pagadas} de {_deuda.NumeroCuotas} cuotas pagadas · Total {Formato.Moneda(total)}";
        Notas = _deuda.Notas;
    }

    [RelayCommand]
    private async Task PagarCuotaAsync(CuotaItem? cuota)
    {
        if (cuota is null || cuota.EstaPagada || _deuda is null)
        {
            return;
        }

        var cuentas = await _cuentas.ObtenerActivasAsync();
        if (cuentas.Count == 0)
        {
            await Dialogos.AlertaAsync("Pagar cuota", "Primero crea una cuenta.");
            return;
        }

        var cuenta = cuentas.FirstOrDefault(c => c.Id == _deuda.CuentaId);
        if (cuenta is null || cuentas.Count > 1)
        {
            var opciones = cuentas.Select(c => $"{c.NombreConIcono} ({c.SaldoTexto})").ToArray();
            var eleccion = await Dialogos.ElegirAsync($"Pagar {cuota.Titulo.ToLower()} ({cuota.MontoTexto}) desde:", opciones);
            if (eleccion is null)
            {
                return;
            }

            cuenta = cuentas[Array.IndexOf(opciones, eleccion)];
        }

        await EjecutarAsync(async () =>
        {
            await _deudas.PagarCuotaAsync(cuota.Id, cuenta.Id);
        });
        await CargarAsync();
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Eliminar deuda", "Se eliminarán las cuotas pendientes. Los pagos ya realizados se conservan en tus movimientos.", "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await _deudas.EliminarAsync(_id);
        await VolverAsync();
    }
}
