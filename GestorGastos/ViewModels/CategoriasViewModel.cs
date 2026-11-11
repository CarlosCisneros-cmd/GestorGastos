using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class CategoriasViewModel : BaseViewModel
{
    private readonly CategoriaRepository _categorias;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrandoGastos))]
    [NotifyPropertyChangedFor(nameof(MostrandoIngresos))]
    private string tipoActual = TiposCategoria.Gasto;

    public bool MostrandoGastos => TipoActual == TiposCategoria.Gasto;

    public bool MostrandoIngresos => TipoActual == TiposCategoria.Ingreso;

    public ObservableCollection<Categoria> Categorias { get; } = new();

    public CategoriasViewModel(CategoriaRepository categorias)
    {
        _categorias = categorias;
        Titulo = "Categorías";
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var lista = await _categorias.ObtenerPorTipoAsync(TipoActual);
            Categorias.ReemplazarCon(lista);
        });
    }

    [RelayCommand]
    private async Task CambiarTipoAsync(string tipo)
    {
        TipoActual = tipo;
        await CargarAsync();
    }

    [RelayCommand]
    private Task NuevaAsync()
    {
        return IrAAsync(Rutas.CategoriaForm, new Dictionary<string, object> { { "Tipo", TipoActual } });
    }

    [RelayCommand]
    private async Task EditarAsync(Categoria? categoria)
    {
        if (categoria is null)
        {
            return;
        }

        await IrAAsync(Rutas.CategoriaForm, new Dictionary<string, object> { { "Id", categoria.Id } });
    }
}
