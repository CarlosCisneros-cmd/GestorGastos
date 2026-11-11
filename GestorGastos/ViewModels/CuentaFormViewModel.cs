using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class CuentaFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly CuentaRepository _cuentas;
    private int _id;
    private bool _cargado;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string tipo = TiposCuenta.Banco;

    [ObservableProperty]
    private string saldoInicialTexto = string.Empty;

    [ObservableProperty]
    private string saldoActualTexto = string.Empty;

    [ObservableProperty]
    private string moneda = Preferencias.Moneda;

    [ObservableProperty]
    private string colorSeleccionado = Paleta.Colores[0];

    [ObservableProperty]
    private string iconoSeleccionado = "🏦";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsEdicion))]
    private bool esNueva = true;

    public bool EsEdicion => !EsNueva;

    public List<string> Tipos { get; } = TiposCuenta.Todos;

    public List<string> Monedas { get; } = Formato.MonedasDisponibles;

    public List<string> Colores { get; } = Paleta.Colores;

    public List<string> Iconos { get; } = Paleta.IconosCuenta;

    public CuentaFormViewModel(CuentaRepository cuentas)
    {
        _cuentas = cuentas;
        Titulo = "Nueva cuenta";
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
        if (EsNueva)
        {
            IconoSeleccionado = TiposCuenta.IconoPorDefecto(value);
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

        var cuenta = await _cuentas.ObtenerPorIdAsync(_id);
        if (cuenta is null)
        {
            return;
        }

        EsNueva = false;
        Titulo = "Editar cuenta";
        Nombre = cuenta.Nombre;
        Tipo = cuenta.Tipo;
        Moneda = cuenta.Moneda;
        ColorSeleccionado = cuenta.Color;
        IconoSeleccionado = cuenta.Icono;
        SaldoActualTexto = cuenta.SaldoTexto;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            decimal saldoInicial = 0;
            if (EsNueva && !string.IsNullOrWhiteSpace(SaldoInicialTexto) && !Formato.IntentarLeerMonto(SaldoInicialTexto, out saldoInicial))
            {
                throw new InvalidOperationException("El saldo inicial no es un número válido.");
            }

            var cuenta = new Cuenta
            {
                Id = _id,
                Nombre = Nombre,
                Tipo = Tipo,
                Saldo = saldoInicial,
                Moneda = Moneda,
                Color = ColorSeleccionado,
                Icono = IconoSeleccionado
            };

            await _cuentas.GuardarAsync(cuenta);
            await VolverAsync();
        });
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Eliminar cuenta", "La cuenta dejará de mostrarse, pero sus movimientos se conservan en el historial.", "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _cuentas.DesactivarAsync(_id);
            await VolverAsync();
        });
    }
}
