namespace GestorGastos.Configuracion;

public static class AuthConfig
{
    public const string GoogleClientId = "TU_CLIENT_ID.apps.googleusercontent.com";

    public const string EsquemaRetorno = "com.googleusercontent.apps.TU_CLIENT_ID";

    public const string FirebaseApiKey = "";

    public static string RedirectUri => $"{EsquemaRetorno}:/oauth2redirect";

    public static bool GoogleConfigurado => !GoogleClientId.StartsWith("TU_") && !EsquemaRetorno.EndsWith("TU_CLIENT_ID");

    public static bool FirebaseConfigurado => !string.IsNullOrWhiteSpace(FirebaseApiKey);
}
