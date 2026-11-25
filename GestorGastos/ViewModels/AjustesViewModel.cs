using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class AjustesViewModel : BaseViewModel
{
    private readonly SesionService _sesion;
    private readonly BiometriaService _biometria;
    private readonly SincronizacionService _sync;
    private bool _cargando;

    [ObservableProperty]
    private string estadoNube = string.Empty;

    [ObservableProperty]
    private bool nubeOcupada;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string correo = string.Empty;

    [ObservableProperty]
    private string inicial = "U";

    [ObservableProperty]
    private bool huellaActiva;

    [ObservableProperty]
    private string bloqueoSeleccionado = "1 minuto";

    [ObservableProperty]
    private string monedaSeleccionada = "USD";

    [ObservableProperty]
    private string temaSeleccionado = "Claro";

    [ObservableProperty]
    private bool alertasActivas = true;

    public List<string> OpcionesBloqueo { get; } = new() { "Desactivado", "1 minuto", "5 minutos", "15 minutos" };

    public List<string> Monedas { get; } = Formato.MonedasDisponibles;

    public List<string> Temas { get; } = new() { "Claro", "Oscuro", "Sistema" };

    public AjustesViewModel(SesionService sesion, BiometriaService biometria, SincronizacionService sync)
    {
        _sesion = sesion;
        _biometria = biometria;
        _sync = sync;
        Titulo = "Ajustes";
        _sync.EstadoCambiado += () => MainThread.BeginInvokeOnMainThread(ActualizarNube);
    }

    private void ActualizarNube()
    {
        EstadoNube = _sync.Estado;
        NubeOcupada = _sync.Sincronizando;
    }

    [RelayCommand]
    private async Task SincronizarAsync()
    {
        _sync.Iniciar();
        var ok = await _sync.SubirAsync();
        ActualizarNube();
        await Dialogos.AlertaAsync(ok ? "Sincronizado" : "No se pudo sincronizar", _sync.Estado);
    }

    [RelayCommand]
    private async Task RestaurarAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Restaurar desde la nube",
            "Se reemplazarán los datos de este teléfono por los guardados en la nube. ¿Continuar?", "Restaurar");
        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var total = await _sync.DescargarAsync();
            ActualizarNube();
            await Dialogos.AlertaAsync("Restauración", total > 0 ? $"Se descargaron {total} registros de la nube." : _sync.Estado);
        });
    }

    public void Cargar()
    {
        _cargando = true;
        var usuario = _sesion.UsuarioActual;
        if (usuario is not null)
        {
            Nombre = usuario.Nombre;
            Correo = usuario.Correo;
            Inicial = usuario.Inicial;
        }

        HuellaActiva = Preferencias.HuellaActiva;
        BloqueoSeleccionado = Preferencias.BloqueoMinutos switch
        {
            0 => "Desactivado",
            5 => "5 minutos",
            15 => "15 minutos",
            _ => "1 minuto"
        };
        MonedaSeleccionada = Preferencias.Moneda;
        TemaSeleccionado = Preferencias.Tema;
        AlertasActivas = Preferencias.AlertasPresupuestoActivas;
        ActualizarNube();
        _cargando = false;
    }

    partial void OnHuellaActivaChanged(bool value)
    {
        if (_cargando)
        {
            return;
        }

        _ = CambiarHuellaAsync(value);
    }

    partial void OnBloqueoSeleccionadoChanged(string value)
    {
        if (_cargando)
        {
            return;
        }

        Preferencias.BloqueoMinutos = value switch
        {
            "Desactivado" => 0,
            "5 minutos" => 5,
            "15 minutos" => 15,
            _ => 1
        };
    }

    partial void OnMonedaSeleccionadaChanged(string value)
    {
        if (_cargando || string.IsNullOrEmpty(value))
        {
            return;
        }

        Preferencias.Moneda = value;
    }

    partial void OnTemaSeleccionadoChanged(string value)
    {
        if (_cargando || string.IsNullOrEmpty(value))
        {
            return;
        }

        Preferencias.Tema = value;
        App.AplicarTema(value);
    }

    partial void OnAlertasActivasChanged(bool value)
    {
        if (_cargando)
        {
            return;
        }

        Preferencias.AlertasPresupuestoActivas = value;
    }

    private async Task CambiarHuellaAsync(bool activar)
    {
        if (!activar)
        {
            Preferencias.HuellaActiva = false;
            return;
        }

        var aprobado = await _biometria.AutenticarAsync("Confirma tu huella para activar el acceso rápido");
        if (aprobado)
        {
            Preferencias.HuellaActiva = true;
            await Dialogos.AlertaAsync("Huella activada", "La próxima vez podrás ingresar con tu huella sin abrir Google.");
            return;
        }

        _cargando = true;
        HuellaActiva = false;
        _cargando = false;
        await Dialogos.AlertaAsync("No se activó", "No se pudo verificar tu huella. Revisa que tengas una huella registrada en el teléfono.");
    }

    [RelayCommand]
    private Task VerCategoriasAsync() => IrAAsync(Rutas.Categorias);

    [RelayCommand]
    private Task VerCuentasAsync() => IrAAsync(Rutas.Cuentas);

    [RelayCommand]
    private async Task CerrarSesionAsync()
    {
        var confirmar = await Dialogos.ConfirmarAsync("Cerrar sesión", "¿Quieres cerrar tu sesión? También se desactivará el acceso con huella.", "Cerrar sesión");
        if (!confirmar)
        {
            return;
        }

        await _sesion.CerrarAsync();
        await Shell.Current.GoToAsync($"//{Rutas.Login}");
    }
}
