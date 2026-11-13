using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class ComprobanteDetalleViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ComprobanteRepository _comprobantes;
    private int _id;

    [ObservableProperty]
    private ComprobanteDetalle? comprobante;

    [ObservableProperty]
    private bool mostrarTexto;

    public ComprobanteDetalleViewModel(ComprobanteRepository comprobantes)
    {
        _comprobantes = comprobantes;
        Titulo = "Comprobante";
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
        Comprobante = await _comprobantes.ObtenerDetalleAsync(_id);
        if (Comprobante is null)
        {
            await Dialogos.AlertaAsync("Comprobante", "El comprobante ya no existe.");
            await VolverAsync();
        }
    }

    [RelayCommand]
    private void AlternarTexto()
    {
        MostrarTexto = !MostrarTexto;
    }

    [RelayCommand]
    private async Task VerMovimientoAsync()
    {
        if (Comprobante is null)
        {
            return;
        }

        await IrAAsync(Rutas.EditarMovimiento, new Dictionary<string, object> { { "Id", Comprobante.TransaccionId } });
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Eliminar comprobante", "Se borrará la foto, pero el movimiento se conserva.", "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await _comprobantes.EliminarAsync(_id);
        await VolverAsync();
    }
}
