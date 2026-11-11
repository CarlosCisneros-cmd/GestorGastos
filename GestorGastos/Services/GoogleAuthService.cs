using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GestorGastos.Configuracion;
using GestorGastos.Models;

namespace GestorGastos.Services;

public class GoogleAuthService
{
    private const string UrlAutorizacion = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string UrlToken = "https://oauth2.googleapis.com/token";
    private const string UrlFirebase = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithIdp?key=";

    private readonly HttpClient _http = new();

    public bool EstaConfigurado => AuthConfig.GoogleConfigurado;

    public async Task<PerfilGoogle> IniciarSesionAsync()
    {
        if (!EstaConfigurado)
        {
            throw new InvalidOperationException("Falta configurar el Client ID de Google en Configuracion/AuthConfig.cs.");
        }

        var verificador = CrearVerificador();
        var reto = CrearReto(verificador);
        var estado = Guid.NewGuid().ToString("N");

        var url = $"{UrlAutorizacion}?client_id={Uri.EscapeDataString(AuthConfig.GoogleClientId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(AuthConfig.RedirectUri)}" +
                  "&response_type=code" +
                  $"&scope={Uri.EscapeDataString("openid email profile")}" +
                  $"&code_challenge={reto}" +
                  "&code_challenge_method=S256" +
                  $"&state={estado}" +
                  "&prompt=select_account";

        var resultado = await WebAuthenticator.Default.AuthenticateAsync(new WebAuthenticatorOptions
        {
            Url = new Uri(url),
            CallbackUrl = new Uri(AuthConfig.RedirectUri),
            PrefersEphemeralWebBrowserSession = true
        });

        if (!resultado.Properties.TryGetValue("code", out var codigo) || string.IsNullOrEmpty(codigo))
        {
            throw new InvalidOperationException("Google no devolvió el código de acceso.");
        }

        var idToken = await CambiarCodigoPorTokenAsync(codigo, verificador);
        var perfil = LeerPerfil(idToken);

        if (AuthConfig.FirebaseConfigurado)
        {
            perfil.FirebaseUid = await RegistrarEnFirebaseAsync(idToken);
        }

        return perfil;
    }

    private async Task<string> CambiarCodigoPorTokenAsync(string codigo, string verificador)
    {
        var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", AuthConfig.GoogleClientId },
            { "code", codigo },
            { "code_verifier", verificador },
            { "grant_type", "authorization_code" },
            { "redirect_uri", AuthConfig.RedirectUri }
        });

        var respuesta = await _http.PostAsync(UrlToken, datos);
        var contenido = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Google rechazó el inicio de sesión: {contenido}");
        }

        using var json = JsonDocument.Parse(contenido);
        if (!json.RootElement.TryGetProperty("id_token", out var token))
        {
            throw new InvalidOperationException("Google no devolvió el token de identidad.");
        }

        return token.GetString() ?? string.Empty;
    }

    private async Task<string> RegistrarEnFirebaseAsync(string idToken)
    {
        var cuerpo = new
        {
            postBody = $"id_token={idToken}&providerId=google.com",
            requestUri = "http://localhost",
            returnIdpCredential = true,
            returnSecureToken = true
        };

        var respuesta = await _http.PostAsJsonAsync(UrlFirebase + AuthConfig.FirebaseApiKey, cuerpo);
        var contenido = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Firebase rechazó el inicio de sesión: {contenido}");
        }

        using var json = JsonDocument.Parse(contenido);
        return json.RootElement.TryGetProperty("localId", out var uid) ? uid.GetString() ?? string.Empty : string.Empty;
    }

    private static PerfilGoogle LeerPerfil(string idToken)
    {
        var partes = idToken.Split('.');
        if (partes.Length < 2)
        {
            throw new InvalidOperationException("El token de Google no es válido.");
        }

        var carga = partes[1].Replace('-', '+').Replace('_', '/');
        carga = carga.PadRight(carga.Length + (4 - carga.Length % 4) % 4, '=');
        var texto = Encoding.UTF8.GetString(Convert.FromBase64String(carga));

        using var json = JsonDocument.Parse(texto);
        var raiz = json.RootElement;

        return new PerfilGoogle
        {
            Id = Leer(raiz, "sub"),
            Correo = Leer(raiz, "email"),
            Nombre = string.IsNullOrEmpty(Leer(raiz, "name")) ? Leer(raiz, "email") : Leer(raiz, "name"),
            FotoUrl = Leer(raiz, "picture")
        };
    }

    private static string Leer(JsonElement elemento, string propiedad)
    {
        return elemento.TryGetProperty(propiedad, out var valor) ? valor.GetString() ?? string.Empty : string.Empty;
    }

    private static string CrearVerificador()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64Url(bytes);
    }

    private static string CrearReto(string verificador)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verificador));
        return Base64Url(hash);
    }

    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
