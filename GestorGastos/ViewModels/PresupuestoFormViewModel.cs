using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;
using Java.Time;
using System.Collections.ObjectModel;

namespace GestorGastos.ViewModels;

public partial class PresupuestoFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly PresupuestoRepository _presupuestos;
    private readonly CategoriaRepository _categorias;
    private Presupuesto? _original;
    private int _id;
    private bool _cargado;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string montoTexto = string.Empty;

    [ObservableProperty]
    private string periodo = PeriodosPresupuesto.Mensual;

    [ObservableProperty]
    private Categoria? categoriaSeleccionada;

    [ObservableProperty]
    private bool esEdicion;

    public List<string> PeriodosDisponibles { get; } = PeriodosPresupuesto.Todos;

    public ObservableCollection<Categoria> Categorias { get; } = new();

    public PresupuestoFormViewModel(PresupuestoRepository presupuestos, CategoriaRepository categorias)
    {
        _presupuestos = presupuestos;
        _categorias = categorias;
        Titulo = "Nuevo presupuesto";
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
        if (_cargado)
        {
            return;
        }

        _cargado = true;
        var lista = await _categorias.ObtenerPorTipoAsync(TiposCategoria.Gasto);
        Categorias.ReemplazarCon(lista.Where(c => c.Nombre != CategoriasEspeciales.Ahorro && c.Nombre != CategoriasEspeciales.PagoDeudas));

        if (_id == 0)
        {
            return;
        }

        _original = await _presupuestos.ObtenerPorIdAsync(_id);
        if (_original is null)
        {
            return;
        }

        EsEdicion = true;
        Titulo = "Editar presupuesto";
        Nombre = _original.Nombre;
        MontoTexto = Formato.MontoParaEditar(_original.Monto);
        Periodo = _original.Periodo;
        CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == _original.CategoriaId);
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (!Formato.IntentarLeerMonto(MontoTexto, out var monto))
            {
                throw new InvalidOperationException("Ingresa un límite válido.");
            }

            var presupuesto = _original ?? new Presupuesto();
            presupuesto.CategoriaId = CategoriaSeleccionada?.Id ?? 0;
            presupuesto.Nombre = string.IsNullOrWhiteSpace(Nombre) ? CategoriaSeleccionada?.Nombre ?? string.Empty : Nombre;
            presupuesto.Monto = monto;
            presupuesto.Periodo = Periodo;

            await _presupuestos.GuardarAsync(presupuesto);
            await VolverAsync();
        });
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Eliminar presupuesto", "¿Eliminar este presupuesto? Tus gastos no se borran.", "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _presupuestos.EliminarAsync(_id);
            await VolverAsync();
        });
    }
}
