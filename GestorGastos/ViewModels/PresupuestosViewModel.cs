using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class PresupuestosViewModel : BaseViewModel
{
    private readonly PresupuestoRepository _presupuestos;

    [ObservableProperty]
    private string totalGastadoTexto = Formato.Moneda(0);

    [ObservableProperty]
    private string totalLimiteTexto = Formato.Moneda(0);

    [ObservableProperty]
    private double progresoTotal;

    [ObservableProperty]
    private string restanteTotalTexto = string.Empty;

    [ObservableProperty]
    private bool hayExcedidos;

    [ObservableProperty]
    private string mensajeExcedidos = string.Empty;

    [ObservableProperty]
    private string mesTexto = Formato.MesAnio(DateTime.Today);

    [ObservableProperty]
    private bool estaRefrescando;

    public ObservableCollection<PresupuestoProgreso> Presupuestos { get; } = new();

    public PresupuestosViewModel(PresupuestoRepository presupuestos)
    {
        _presupuestos = presupuestos;
        Titulo = "Presupuestos";
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        try
        {
            var lista = await _presupuestos.ObtenerConProgresoAsync(DateTime.Today);
            Presupuestos.ReemplazarCon(lista);

            var mensuales = lista.Where(p => p.Presupuesto.Periodo == PeriodosPresupuesto.Mensual).ToList();
            var gastado = mensuales.Sum(p => p.Gastado);
            var limite = mensuales.Sum(p => p.Limite);
            MesTexto = Formato.MesAnio(DateTime.Today);
            TotalGastadoTexto = Formato.Moneda(gastado);
            TotalLimiteTexto = Formato.Moneda(limite);
            ProgresoTotal = limite <= 0 ? 0 : Math.Min((double)(gastado / limite), 1);
            RestanteTotalTexto = limite <= 0
                ? "Crea un presupuesto mensual para ver tu resumen"
                : gastado > limite ? $"Excedido {Formato.Moneda(gastado - limite)}" : $"Quedan {Formato.Moneda(limite - gastado)}";

            var excedidos = lista.Where(p => p.EstaExcedido).ToList();
            HayExcedidos = excedidos.Count > 0;
            MensajeExcedidos = excedidos.Count switch
            {
                0 => string.Empty,
                1 => $"{excedidos[0].Nombre} superó su presupuesto ({excedidos[0].RestanteTexto.ToLower()})",
                _ => $"{excedidos.Count} presupuestos superaron su límite"
            };
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("No se pudieron cargar los presupuestos", ex.Message);
        }
        finally
        {
            EstaRefrescando = false;
        }
    }

    [RelayCommand]
    private Task NuevoAsync() => IrAAsync(Rutas.PresupuestoForm);

    [RelayCommand]
    private async Task EditarAsync(PresupuestoProgreso? presupuesto)
    {
        if (presupuesto is null)
        {
            return;
        }

        await IrAAsync(Rutas.PresupuestoForm, new Dictionary<string, object> { { "Id", presupuesto.Id } });
    }
}
