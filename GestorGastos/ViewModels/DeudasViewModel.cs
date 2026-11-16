using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class DeudasViewModel : BaseViewModel
{
    private readonly DeudaRepository _deudas;

    [ObservableProperty]
    private string totalPendienteTexto = Formato.Moneda(0);

    public ObservableCollection<DeudaResumen> Deudas { get; } = new();

    public DeudasViewModel(DeudaRepository deudas)
    {
        _deudas = deudas;
        Titulo = "Deudas";
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        try
        {
            var lista = await _deudas.ObtenerResumenesAsync();
            Deudas.ReemplazarCon(lista);
            TotalPendienteTexto = Formato.Moneda(lista.Sum(d => d.Deuda.MontoPendiente));
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("No se pudieron cargar las deudas", ex.Message);
        }
    }

    [RelayCommand]
    private Task NuevaAsync() => IrAAsync(Rutas.DeudaForm);

    [RelayCommand]
    private async Task AbrirAsync(DeudaResumen? deuda)
    {
        if (deuda is null)
        {
            return;
        }

        await IrAAsync(Rutas.DeudaDetalle, new Dictionary<string, object> { { "Id", deuda.Id } });
    }
}
