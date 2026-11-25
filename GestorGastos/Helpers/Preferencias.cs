namespace GestorGastos.Helpers;

public static class Preferencias
{
    private const string ClaveSesion = "sesion_google_id";
    private const string ClaveHuella = "huella_activa";
    private const string ClaveMoneda = "moneda";
    private const string ClaveTema = "tema";
    private const string ClaveBloqueo = "bloqueo_minutos";
    private const string ClaveAlertas = "alertas_presupuesto";
    private const string ClaveSaldoMinimo = "saldo_minimo_aviso";

    public static string SesionGoogleId
    {
        get => Preferences.Default.Get(ClaveSesion, string.Empty);
        set => Preferences.Default.Set(ClaveSesion, value);
    }

    public static bool HuellaActiva
    {
        get => Preferences.Default.Get(ClaveHuella, false);
        set => Preferences.Default.Set(ClaveHuella, value);
    }

    public static string Moneda
    {
        get => Preferences.Default.Get(ClaveMoneda, "USD");
        set => Preferences.Default.Set(ClaveMoneda, value);
    }

    public static string Tema
    {
        get => Preferences.Default.Get(ClaveTema, "Claro");
        set => Preferences.Default.Set(ClaveTema, value);
    }

    public static int BloqueoMinutos
    {
        get => Preferences.Default.Get(ClaveBloqueo, 1);
        set => Preferences.Default.Set(ClaveBloqueo, value);
    }

    public static bool AlertasPresupuestoActivas
    {
        get => Preferences.Default.Get(ClaveAlertas, true);
        set => Preferences.Default.Set(ClaveAlertas, value);
    }

    /// <summary>Cuando el saldo de una cuenta baja de este valor, la app avisa que queda poco dinero.</summary>
    public static decimal SaldoMinimoAviso
    {
        get => (decimal)Preferences.Default.Get(ClaveSaldoMinimo, 20.0);
        set => Preferences.Default.Set(ClaveSaldoMinimo, (double)value);
    }

    public static void LimpiarSesion()
    {
        Preferences.Default.Remove(ClaveSesion);
        Preferences.Default.Remove(ClaveHuella);
    }
}
