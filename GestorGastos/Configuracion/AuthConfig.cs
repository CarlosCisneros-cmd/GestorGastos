namespace GestorGastos.Configuracion;

public static class AuthConfig
{
    public const string GoogleClientId = "510403353694-t468sblviue7u9985irt7lqk25jttmop.apps.googleusercontent.com";

    public const string EsquemaRetorno = "com.googleusercontent.apps.510403353694-t468sblviue7u9985irt7lqk25jttmop";

    public const string FirebaseApiKey = "";

    public static string RedirectUri => $"{EsquemaRetorno}:/oauth2redirect";

    public static bool GoogleConfigurado => !GoogleClientId.StartsWith("TU_") && !EsquemaRetorno.EndsWith("TU_CLIENT_ID");

    public static bool FirebaseConfigurado => !string.IsNullOrWhiteSpace(FirebaseApiKey);
}
