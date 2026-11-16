using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class DeudaFormViewModel : BaseViewModel
{
    private readonly DeudaRepository _deudas;
    private readonly CuentaRepository _cuentas;
    private bool _cargado;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string montoTexto = string.Empty;

    [ObservableProperty]
    private string tasaTexto = "0";

    [ObservableProperty]
    private bool interesIncluido = true;

    [ObservableProperty]
    private string numeroCuotasTexto = "12";

    [ObservableProperty]
    private DateTime fechaPrimeraCuota = DateTime.Today.AddMonths(1);

    [ObservableProperty]
    private Cuenta? cuentaSeleccionada;

    [ObservableProperty]
    private string notas = string.Empty;

    [ObservableProperty]
    private string vistaPrevia = "Completa el monto y el número de cuotas para ver el plan.";

    public ObservableCollection<Cuenta> Cuentas { get; } = new();

    public DeudaFormViewModel(DeudaRepository deudas, CuentaRepository cuentas)
    {
        _deudas = deudas;
        _cuentas = cuentas;
        Titulo = "Nueva deuda";
    }

    public async Task CargarAsync()
    {
        if (_cargado)
        {
            return;
        }

        _cargado = true;
        Cuentas.ReemplazarCon(await _cuentas.ObtenerActivasAsync());
        CuentaSeleccionada = Cuentas.FirstOrDefault();
        ActualizarVistaPrevia();
    }

    partial void OnMontoTextoChanged(string value) => ActualizarVistaPrevia();

    partial void OnTasaTextoChanged(string value) => ActualizarVistaPrevia();

    partial void OnInteresIncluidoChanged(bool value) => ActualizarVistaPrevia();

    partial void OnNumeroCuotasTextoChanged(string value) => ActualizarVistaPrevia();

    private void ActualizarVistaPrevia()
    {
        if (!Formato.IntentarLeerMonto(MontoTexto, out var monto) || monto <= 0 || !int.TryParse(NumeroCuotasTexto, out var cuotas) || cuotas <= 0)
        {
            VistaPrevia = "Completa el monto y el número de cuotas para ver el plan.";
            return;
        }

        Formato.IntentarLeerMonto(TasaTexto, out var tasa);
        var (total, interes, lista) = DeudaRepository.CalcularPlan(monto, tasa, InteresIncluido, cuotas, FechaPrimeraCuota);
        var textoInteres = interes > 0 ? $" (incluye {Formato.Moneda(interes)} de interés)" : string.Empty;
        VistaPrevia = $"{cuotas} cuota(s) de {Formato.Moneda(lista[0].Monto)}. Total a pagar: {Formato.Moneda(total)}{textoInteres}.";
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

            if (!int.TryParse(NumeroCuotasTexto, out var cuotas) || cuotas <= 0 || cuotas > 360)
            {
                throw new InvalidOperationException("El número de cuotas debe estar entre 1 y 360.");
            }

            Formato.IntentarLeerMonto(TasaTexto, out var tasa);

            var deuda = new Deuda
            {
                Nombre = Nombre,
                MontoTotal = monto,
                TasaInteres = tasa,
                InteresIncluido = InteresIncluido,
                NumeroCuotas = cuotas,
                FechaInicio = FechaPrimeraCuota.Date,
                CuentaId = CuentaSeleccionada?.Id,
                Notas = Notas
            };

            await _deudas.CrearAsync(deuda);
            await VolverAsync();
        });
    }
}
