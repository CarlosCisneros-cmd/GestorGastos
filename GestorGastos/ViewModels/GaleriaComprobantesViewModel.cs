using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class GaleriaComprobantesViewModel : BaseViewModel
{
    private readonly ComprobanteRepository _comprobantes;
    private readonly CategoriaRepository _categorias;
    private DateTime _mes = Periodos.InicioMes(DateTime.Today);

    [ObservableProperty]
    private string mesTexto = Formato.MesAnio(DateTime.Today);

    [ObservableProperty]
    private string resumenTexto = string.Empty;

    [ObservableProperty]
    private Categoria? categoriaFiltro;

    public ObservableCollection<ComprobanteDetalle> Comprobantes { get; } = new();

    public ObservableCollection<Categoria> CategoriasFiltro { get; } = new();

    public GaleriaComprobantesViewModel(ComprobanteRepository comprobantes, CategoriaRepository categorias)
    {
        _comprobantes = comprobantes;
        _categorias = categorias;
        Titulo = "Comprobantes";
    }

    partial void OnCategoriaFiltroChanged(Categoria? value)
    {
        _ = CargarAsync();
    }

    public async Task CargarAsync()
    {
        try
        {
            MesTexto = Formato.MesAnio(_mes);

            if (CategoriasFiltro.Count == 0)
            {
                var todas = new Categoria { Id = 0, Nombre = "Todas", Icono = "📂" };
                var lista = await _categorias.ObtenerPorTipoAsync(TiposCategoria.Gasto);
                CategoriasFiltro.ReemplazarCon(new[] { todas }.Concat(lista));
            }

            int? categoriaId = CategoriaFiltro is null || CategoriaFiltro.Id == 0 ? null : CategoriaFiltro.Id;
            var comprobantes = await _comprobantes.ObtenerGaleriaAsync(_mes, _mes.AddMonths(1), categoriaId);
            Comprobantes.ReemplazarCon(comprobantes);
            ResumenTexto = comprobantes.Count == 0
                ? "Sin comprobantes este mes"
                : $"{comprobantes.Count} comprobante(s) · {Formato.Moneda(comprobantes.Sum(c => c.Monto))}";
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("No se pudo cargar la galería", ex.Message);
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
    private async Task AbrirAsync(ComprobanteDetalle? comprobante)
    {
        if (comprobante is null)
        {
            return;
        }

        await IrAAsync(Rutas.ComprobanteDetalle, new Dictionary<string, object> { { "Id", comprobante.Id } });
    }

    [RelayCommand]
    private Task EscanearAsync() => Shell.Current.GoToAsync($"//{Rutas.Registrar}");
}
