using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class RecurrentesViewModel : BaseViewModel
{
    private readonly RecurrenteRepository _recurrentes;

    public ObservableCollection<RecurrenteDetalle> Recurrentes { get; } = new();

    public RecurrentesViewModel(RecurrenteRepository recurrentes)
    {
        _recurrentes = recurrentes;
        Titulo = "Pagos recurrentes";
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            Recurrentes.ReemplazarCon(await _recurrentes.ObtenerTodosAsync());
        });
    }

    [RelayCommand]
    private Task NuevoAsync() => IrAAsync(Rutas.RecurrenteForm);

    [RelayCommand]
    private async Task EditarAsync(RecurrenteDetalle? recurrente)
    {
        if (recurrente is null)
        {
            return;
        }

        await IrAAsync(Rutas.RecurrenteForm, new Dictionary<string, object> { { "Id", recurrente.Id } });
    }

    [RelayCommand]
    private async Task CambiarEstadoAsync(RecurrenteDetalle? recurrente)
    {
        if (recurrente is null)
        {
            return;
        }

        await _recurrentes.CambiarEstadoAsync(recurrente.Id);
        await CargarAsync();
    }
}
