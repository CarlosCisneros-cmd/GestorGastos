using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class MetasAhorroViewModel : BaseViewModel
{
    private readonly AhorroRepository _ahorros;
    private readonly CuentaRepository _cuentas;

    [ObservableProperty]
    private string totalAhorradoTexto = Formato.Moneda(0);

    public ObservableCollection<MetaAhorroItem> Metas { get; } = new();

    public MetasAhorroViewModel(AhorroRepository ahorros, CuentaRepository cuentas)
    {
        _ahorros = ahorros;
        _cuentas = cuentas;
        Titulo = "Metas de ahorro";
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        try
        {
            var metas = await _ahorros.ObtenerActivasAsync();
            Metas.ReemplazarCon(metas.Select(m => new MetaAhorroItem(m)));
            TotalAhorradoTexto = Formato.Moneda(metas.Sum(m => m.MontoActual));
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("No se pudieron cargar las metas", ex.Message);
        }
    }

    [RelayCommand]
    private Task NuevaAsync() => IrAAsync(Rutas.MetaForm);

    [RelayCommand]
    private async Task EditarAsync(MetaAhorroItem? meta)
    {
        if (meta is null)
        {
            return;
        }

        await IrAAsync(Rutas.MetaForm, new Dictionary<string, object> { { "Id", meta.Id } });
    }

    [RelayCommand]
    private async Task AportarAsync(MetaAhorroItem? meta)
    {
        if (meta is null)
        {
            return;
        }

        var cuentas = await _cuentas.ObtenerActivasAsync();
        if (cuentas.Count == 0)
        {
            await Dialogos.AlertaAsync("Aportar", "Primero crea una cuenta.");
            return;
        }

        var opciones = cuentas.Select(c => $"{c.NombreConIcono} ({c.SaldoTexto})").ToArray();
        var eleccion = await Dialogos.ElegirAsync("¿De qué cuenta sale el dinero?", opciones);
        if (eleccion is null)
        {
            return;
        }

        var cuenta = cuentas[Array.IndexOf(opciones, eleccion)];
        var texto = await Dialogos.PedirTextoAsync("Aportar a la meta", $"¿Cuánto quieres aportar a {meta.Meta.Nombre}?", string.Empty, Keyboard.Numeric);
        if (string.IsNullOrWhiteSpace(texto))
        {
            return;
        }

        if (!Formato.IntentarLeerMonto(texto, out var monto))
        {
            await Dialogos.AlertaAsync("Aportar", "El monto no es válido.");
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _ahorros.AportarAsync(meta.Id, cuenta.Id, monto);
        });
        await CargarAsync();
    }
}
