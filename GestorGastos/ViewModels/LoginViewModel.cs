using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly SesionService _sesion;
    private readonly GoogleAuthService _google;
    private readonly BiometriaService _biometria;
    private readonly IEnumerable<ITareaInicio> _tareas;

    [ObservableProperty]
    private bool puedeUsarHuella;

    [ObservableProperty]
    private bool esModoDesarrollo;

    public LoginViewModel(SesionService sesion, GoogleAuthService google, BiometriaService biometria, IEnumerable<ITareaInicio> tareas)
    {
        _sesion = sesion;
        _google = google;
        _biometria = biometria;
        _tareas = tareas;
#if DEBUG
        EsModoDesarrollo = true;
#endif
    }

    public void Actualizar()
    {
        PuedeUsarHuella = _sesion.PuedeUsarHuella;
    }

    [RelayCommand]
    private async Task ContinuarConGoogleAsync()
    {
        if (!_google.EstaConfigurado)
        {
            var mensaje = "Todavía no se configuró el Client ID de Google (archivo Configuracion/AuthConfig.cs).";
            if (EsModoDesarrollo)
            {
                mensaje += " Mientras tanto puedes usar el botón \"Entrar en modo prueba\".";
            }
            await Dialogos.AlertaAsync("Inicio de sesión con Google", mensaje);
            return;
        }

        await EjecutarAsync(async () =>
        {
            PerfilGoogle perfil;
            try
            {
                perfil = await _google.IniciarSesionAsync();
            }
            catch (TaskCanceledException)
            {
                return;
            }

            await _sesion.IniciarAsync(perfil);
            await EntrarAsync();
        });
    }

    [RelayCommand]
    private async Task IngresarConHuellaAsync()
    {
        if (!_sesion.PuedeUsarHuella)
        {
            await Dialogos.AlertaAsync("Huella digital", "Primero ingresa con Google y activa la huella en Ajustes.");
            return;
        }

        var aprobado = await _biometria.AutenticarAsync("Usa tu huella para ingresar a la app");
        if (!aprobado)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var restaurada = await _sesion.RestaurarAsync();
            if (!restaurada)
            {
                await Dialogos.AlertaAsync("Huella digital", "No encontramos tu sesión. Ingresa nuevamente con Google.");
                return;
            }
            await EntrarAsync();
        });
    }

    [RelayCommand]
    private async Task EntrarModoPruebaAsync()
    {
        await EjecutarAsync(async () =>
        {
            var perfil = new PerfilGoogle
            {
                Id = "modoprueba",
                Nombre = "Usuario de prueba",
                Correo = "prueba@ejemplo.com"
            };
            await _sesion.IniciarAsync(perfil);
            await EntrarAsync();
        });
    }

    private async Task EntrarAsync()
    {
        foreach (var tarea in _tareas)
        {
            try
            {
                await tarea.EjecutarAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Tarea de inicio con error: {ex.Message}");
            }
        }

        await Shell.Current.GoToAsync($"//{Rutas.Inicio}");
    }
}
