using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;
using System.Collections.ObjectModel;
using static Android.Provider.ContactsContract.CommonDataKinds;

namespace GestorGastos.ViewModels;

public partial class RecurrenteFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly RecurrenteRepository _recurrentes;
    private readonly CuentaRepository _cuentas;
    private readonly CategoriaRepository _categorias;
    private PagoRecurrente? _original;
    private int _id;
    private bool _cargado;

    [ObservableProperty]
    private string tipo = TiposMovimiento.Gasto;

    [ObservableProperty]
    private string montoTexto = string.Empty;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string nota = string.Empty;

    [ObservableProperty]
    private Cuenta? cuentaSeleccionada;

    [ObservableProperty]
    private Categoria? categoriaSeleccionada;

    [ObservableProperty]
    private string frecuencia = Frecuencias.Mensual;

    [ObservableProperty]
    private DateTime fechaInicio = DateTime.Today;

    [ObservableProperty]
    private bool tieneFechaFin;

    [ObservableProperty]
    private DateTime fechaFin = DateTime.Today.AddYears(1);

    [ObservableProperty]
    private bool esEdicion;

    public List<string> Tipos { get; } = new() { TiposMovimiento.Gasto, TiposMovimiento.Ingreso };

    public List<string> FrecuenciasDisponibles { get; } = Frecuencias.Todas;

    public ObservableCollection<Cuenta> Cuentas { get; } = new();

    public ObservableCollection<Categoria> Categorias { get; } = new();

    public RecurrenteFormViewModel(RecurrenteRepository recurrentes, CuentaRepository cuentas, CategoriaRepository categorias)
    {
        _recurrentes = recurrentes;
        _cuentas = cuentas;
        _categorias = categorias;
        Titulo = "Nuevo pago recurrente";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var valor) && valor is int id)
        {
            _id = id;
        }
    }

    partial void OnTipoChanged(string value)
    {
        if (_cargado)
        {
            _ = CargarCategoriasAsync(null);
        }
    }

    public async Task CargarAsync()
    {
        if (_cargado)
        {
            return;
        }

        Cuentas.ReemplazarCon(await _cuentas.ObtenerActivasAsync());
        CuentaSeleccionada = Cuentas.FirstOrDefault();

        if (_id != 0)
        {
            _original = await _recurrentes.ObtenerPorIdAsync(_id);
            if (_original is not null)
            {
                EsEdicion = true;
                Titulo = "Editar pago recurrente";
                Tipo = _original.Tipo;
                MontoTexto = Formato.MontoParaEditar(_original.Monto);
                Nombre = _original.Nombre;
                Nota = _original.Nota;
                Frecuencia = _original.Frecuencia;
                FechaInicio = _original.FechaInicio;
                TieneFechaFin = _original.FechaFin.HasValue;
                FechaFin = _original.FechaFin ?? DateTime.Today.AddYears(1);
                CuentaSeleccionada = Cuentas.FirstOrDefault(c => c.Id == _original.CuentaId);
            }
        }

        await CargarCategoriasAsync(_original?.CategoriaId);
        _cargado = true;
    }

    private async Task CargarCategoriasAsync(int? seleccionarId)
    {
        var tipoCategoria = Tipo == TiposMovimiento.Ingreso ? TiposCategoria.Ingreso : TiposCategoria.Gasto;
        Categorias.ReemplazarCon(await _categorias.ObtenerPorTipoAsync(tipoCategoria));
        CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == seleccionarId);
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (!Formato.IntentarLeerMonto(MontoTexto, out var monto))
            {
                throw new InvalidOperationException("Ingresa un monto válido.");
            }

            var recurrente = _original ?? new PagoRecurrente();
            recurrente.Tipo = Tipo;
            recurrente.Monto = monto;
            recurrente.Nombre = Nombre;
            recurrente.Nota = Nota;
            recurrente.CuentaId = CuentaSeleccionada?.Id ?? 0;
            recurrente.CategoriaId = CategoriaSeleccionada?.Id;
            recurrente.Frecuencia = Frecuencia;
            recurrente.FechaInicio = FechaInicio.Date;
            recurrente.FechaFin = TieneFechaFin ? FechaFin.Date : null;

            if (EsEdicion && _original is not null && _original.ProximaFecha.Date < FechaInicio.Date)
            {
                recurrente.ProximaFecha = FechaInicio.Date;
            }

            await _recurrentes.GuardarAsync(recurrente);
            await VolverAsync();
        });
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Eliminar", "¿Eliminar este pago recurrente? Los movimientos que ya se registraron se conservan.", "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _recurrentes.EliminarAsync(_id);
            await VolverAsync();
        });
    }
}
