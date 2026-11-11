using Plugin.Maui.Biometric;

namespace GestorGastos.Services;

public class BiometriaService
{
    public async Task<bool> AutenticarAsync(string motivo)
    {
        try
        {
            var solicitud = new AuthenticationRequest
            {
                Title = "Verifica tu identidad",
                Description = motivo,
                NegativeText = "Cancelar"
            };

            var resultado = await BiometricAuthenticationService.Default.AuthenticateAsync(solicitud, CancellationToken.None);
            return resultado.Status == BiometricResponseStatus.Success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de biometría: {ex.Message}");
            return false;
        }
    }
}
