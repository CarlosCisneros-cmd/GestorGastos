using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class MetaAhorroFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly AhorroRepository _ahorros;
    private MetaAhorro? _original;
    private int _id;
    private bool _cargado;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string montoObjetivoTexto = string.Empty;

    [ObservableProperty]
    private string iconoSeleccionado = "🎯";

    [ObservableProperty]
    private bool tieneFechaLimite = true;

    [ObservableProperty]
    private DateTime fechaLimite = DateTime.Today.AddMonths(6);

    [ObservableProperty]
    private string ahorradoTexto = string.Empty;

    [ObservableProperty]
    private bool esEdicion;

    public List<string> Iconos { get; } = new() { "🎯", "💻", "✈️", "🚗", "🏠", "🎓", "📱", "🎁", "🏖️", "💍", "🐶", "🎮", "🚲", "🛋️", "💰", "🩺" };

    public MetaAhorroFormViewModel(AhorroRepository ahorros)
    {
        _ahorros = ahorros;
        Titulo = "Nueva meta";
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
        if (_id == 0)
        {
            return;
        }

        _original = await _ahorros.ObtenerPorIdAsync(_id);
        if (_original is null)
        {
            return;
        }

        EsEdicion = true;
        Titulo = "Editar meta";
        Nombre = _original.Nombre;
        MontoObjetivoTexto = Formato.MontoParaEditar(_original.MontoObjetivo);
        IconoSeleccionado = _original.Icono;
        TieneFechaLimite = _original.FechaLimite.HasValue;
        FechaLimite = _original.FechaLimite ?? DateTime.Today.AddMonths(6);
        AhorradoTexto = $"Llevas ahorrado {Formato.Moneda(_original.MontoActual)}";
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (!Formato.IntentarLeerMonto(MontoObjetivoTexto, out var objetivo))
            {
                throw new InvalidOperationException("Ingresa un monto objetivo válido.");
            }

            if (TieneFechaLimite && FechaLimite.Date <= DateTime.Today)
            {
                throw new InvalidOperationException("La fecha límite debe ser una fecha futura.");
            }

            var meta = _original ?? new MetaAhorro();
            meta.Nombre = Nombre;
            meta.MontoObjetivo = objetivo;
            meta.Icono = IconoSeleccionado;
            meta.FechaLimite = TieneFechaLimite ? FechaLimite.Date : null;

            await _ahorros.GuardarAsync(meta);
            await VolverAsync();
        });
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Eliminar meta", "La meta dejará de mostrarse. Los aportes ya registrados se conservan en tus movimientos.", "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _ahorros.EliminarAsync(_id);
            await VolverAsync();
        });
    }
}
