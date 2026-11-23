using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class ProximosPagosViewModel : BaseViewModel
{
    private readonly ProximoPagoRepository _pagos;

    [ObservableProperty]
    private string totalPendienteTexto = Formato.Moneda(0);

    [ObservableProperty]
    private int cantidadVencidos;

    public ObservableCollection<PagoProximoDetalle> Pagos { get; } = new();

    public ProximosPagosViewModel(ProximoPagoRepository pagos)
    {
        _pagos = pagos;
        Titulo = "Próximos pagos";
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var lista = await _pagos.ObtenerPendientesAsync();
            Pagos.ReemplazarCon(lista);
            TotalPendienteTexto = Formato.Moneda(lista.Where(p => p.Tipo == TiposMovimiento.Gasto).Sum(p => p.Monto));
            CantidadVencidos = lista.Count(p => p.EstaVencido);
        });
    }

    [RelayCommand]
    private Task NuevoAsync() => IrAAsync(Rutas.ProximoPagoForm);

    [RelayCommand]
    private Task VerRecurrentesAsync() => IrAAsync(Rutas.Recurrentes);

    [RelayCommand]
    private async Task EditarAsync(PagoProximoDetalle? pago)
    {
        if (pago is null)
        {
            return;
        }

        if (pago.EsRecurrente)
        {
            await Dialogos.AlertaAsync("Pago recurrente", "Este pago lo genera un pago recurrente. Para cambiarlo, edita el pago recurrente.");
            return;
        }

        await IrAAsync(Rutas.ProximoPagoForm, new Dictionary<string, object> { { "Id", pago.Id } });
    }

    [RelayCommand]
    private async Task MarcarPagadoAsync(PagoProximoDetalle? pago)
    {
        if (pago is null)
        {
            return;
        }

        var confirmar = await Dialogos.ConfirmarAsync("Marcar como pagado", $"Se registrará {pago.MontoTexto} en {pago.CuentaNombre}. ¿Continuar?", "Sí, pagar");
        if (!confirmar)
        {
            return;
        }

        await _pagos.MarcarPagadoAsync(pago.Id);
        await CargarAsync();
    }
}
