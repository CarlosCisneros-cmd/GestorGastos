namespace GestorGastos.Configuracion;

/// <summary>
/// Datos del proyecto de Supabase (la base de datos en la nube).
/// Se obtienen en Supabase > Project Settings > API (o "Connect").
/// </summary>
public static class SupabaseConfig
{
    // Ejemplo: https://abcdefghijkl.supabase.co
    public const string Url = "https://eplrggigvguzbwelavnw.supabase.co";

    // La clave pública "anon" o "publishable" (NO la "service_role" / "secret")
    public const string ClavePublica = "sb_publishable_93N6E3XvAhsJxU6G5i12pA_1Bn3hoec";

    public static bool EstaConfigurado =>
        Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && !ClavePublica.StartsWith("TU_");
}
