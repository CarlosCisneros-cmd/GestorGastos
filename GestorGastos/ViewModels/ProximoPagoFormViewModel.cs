using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;
using System.Collections.ObjectModel;
using static Android.Provider.ContactsContract.CommonDataKinds;

namespace GestorGastos.ViewModels;

public partial class ProximoPagoFormViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ProximoPagoRepository _pagos;
    private readonly CuentaRepository _cuentas;
    private readonly CategoriaRepository _categorias;
    private PagoProximo? _original;
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
    private DateTime fechaVencimiento = DateTime.Today.AddDays(7);

    [ObservableProperty]
    private Cuenta? cuentaSeleccionada;

    [ObservableProperty]
    private Categoria? categoriaSeleccionada;

    [ObservableProperty]
    private bool esEdicion;

    public List<string> Tipos { get; } = new() { TiposMovimiento.Gasto, TiposMovimiento.Ingreso };

    public ObservableCollection<Cuenta> Cuentas { get; } = new();

    public ObservableCollection<Categoria> Categorias { get; } = new();

    public ProximoPagoFormViewModel(ProximoPagoRepository pagos, CuentaRepository cuentas, CategoriaRepository categorias)
    {
        _pagos = pagos;
        _cuentas = cuentas;
        _categorias = categorias;
        Titulo = "Nuevo pago";
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
            _original = await _pagos.ObtenerPorIdAsync(_id);
            if (_original is not null)
            {
                EsEdicion = true;
                Titulo = "Editar pago";
                Tipo = _original.Tipo;
                MontoTexto = Formato.MontoParaEditar(_original.Monto);
                Nombre = _original.Nombre;
                Nota = _original.Nota;
                FechaVencimiento = _original.FechaVencimiento;
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

            var pago = _original ?? new PagoProximo();
            pago.Tipo = Tipo;
            pago.Monto = monto;
            pago.Nombre = Nombre;
            pago.Nota = Nota;
            pago.FechaVencimiento = FechaVencimiento;
            pago.CuentaId = CuentaSeleccionada?.Id ?? 0;
            pago.CategoriaId = CategoriaSeleccionada?.Id;

            await _pagos.GuardarAsync(pago);
            await VolverAsync();
        });
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Eliminar pago", "¿Eliminar este pago pendiente?", "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _pagos.EliminarAsync(_id);
            await VolverAsync();
        });
    }
}
