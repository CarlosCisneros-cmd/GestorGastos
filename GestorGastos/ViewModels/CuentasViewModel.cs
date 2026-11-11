using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class CuentasViewModel : BaseViewModel
{
    private readonly CuentaRepository _cuentas;

    [ObservableProperty]
    private string totalTexto = Formato.Moneda(0);

    public ObservableCollection<Cuenta> Cuentas { get; } = new();

    public CuentasViewModel(CuentaRepository cuentas)
    {
        _cuentas = cuentas;
        Titulo = "Cuentas";
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var lista = await _cuentas.ObtenerActivasAsync();
            Cuentas.ReemplazarCon(lista);
            TotalTexto = Formato.Moneda(lista.Sum(c => c.Saldo));
        });
    }

    [RelayCommand]
    private Task NuevaAsync() => IrAAsync(Rutas.CuentaForm);

    [RelayCommand]
    private async Task EditarAsync(Cuenta? cuenta)
    {
        if (cuenta is null)
        {
            return;
        }

        await IrAAsync(Rutas.CuentaForm, new Dictionary<string, object> { { "Id", cuenta.Id } });
    }
}
