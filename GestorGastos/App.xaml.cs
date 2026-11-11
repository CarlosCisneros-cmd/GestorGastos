using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Services;

namespace GestorGastos;

public partial class App : Application
{
    private readonly SesionService _sesion;

    public App(SesionService sesion)
    {
        InitializeComponent();
        _sesion = sesion;
        AplicarTema(Preferencias.Tema);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var ventana = new Window(new AppShell());
        ventana.Stopped += (sender, e) => _sesion.MarcarPausa();
        ventana.Resumed += async (sender, e) =>
        {
            if (_sesion.DebeBloquear() && Shell.Current is not null)
            {
                await Shell.Current.GoToAsync($"//{Rutas.Login}");
            }
        };
        return ventana;
    }

    public static void AplicarTema(string tema)
    {
        if (Current is null)
        {
            return;
        }

        Current.UserAppTheme = tema switch
        {
            "Oscuro" => AppTheme.Dark,
            "Sistema" => AppTheme.Unspecified,
            _ => AppTheme.Light
        };
    }
}
