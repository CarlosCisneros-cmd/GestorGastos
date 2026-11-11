using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class CategoriaFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly CategoriaRepository _categorias;
    private Categoria? _original;
    private int _id;
    private bool _cargado;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string tipo = TiposCategoria.Gasto;

    [ObservableProperty]
    private string iconoSeleccionado = Paleta.IconosCategoria[0];

    [ObservableProperty]
    private string colorSeleccionado = Paleta.Colores[0];

    [ObservableProperty]
    private bool puedeEliminar;

    public List<string> Tipos { get; } = new() { TiposCategoria.Gasto, TiposCategoria.Ingreso };

    public List<string> Iconos { get; } = Paleta.IconosCategoria;

    public List<string> Colores { get; } = Paleta.Colores;

    public CategoriaFormViewModel(CategoriaRepository categorias)
    {
        _categorias = categorias;
        Titulo = "Nueva categoría";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var valor) && valor is int id)
        {
            _id = id;
        }

        if (query.TryGetValue("Tipo", out var tipoRecibido) && tipoRecibido is string texto)
        {
            Tipo = texto;
        }
    }

    public async Task CargarAsync()
    {
        if (_cargado)
        {
            return;
        }

        _cargado = true;
        if (_id == 0)
        {
            return;
        }

        _original = await _categorias.ObtenerPorIdAsync(_id);
        if (_original is null)
        {
            return;
        }

        Titulo = "Editar categoría";
        Nombre = _original.Nombre;
        Tipo = _original.Tipo;
        IconoSeleccionado = _original.Icono;
        ColorSeleccionado = _original.Color;
        PuedeEliminar = !CategoriaRepository.EsProtegida(_original);
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var categoria = _original ?? new Categoria();
            categoria.Nombre = Nombre;
            categoria.Tipo = Tipo;
            categoria.Icono = IconoSeleccionado;
            categoria.Color = ColorSeleccionado;
            await _categorias.GuardarAsync(categoria);
            await VolverAsync();
        });
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        if (_original is null)
        {
            return;
        }

        var confirmar = await Dialogos.ConfirmarAsync("Eliminar categoría", $"¿Eliminar la categoría {_original.Nombre}? Los movimientos que ya la usan no se borran.", "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _categorias.DesactivarAsync(_original);
            await VolverAsync();
        });
    }
}
