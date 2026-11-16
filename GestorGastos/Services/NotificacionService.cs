using GestorGastos.Helpers;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace GestorGastos.Services;

public class NotificacionService
{
    public async Task MostrarAsync(string titulo, string mensaje)
    {
        var enviada = false;
        try
        {
            var permitido = await LocalNotificationCenter.Current.RequestNotificationPermission();
            if (permitido)
            {
                var solicitud = new NotificationRequest
                {
                    NotificationId = Random.Shared.Next(1000, int.MaxValue),
                    Title = titulo,
                    Description = mensaje
                };
                enviada = await LocalNotificationCenter.Current.Show(solicitud);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo mostrar la notificación: {ex.Message}");
        }

        if (!enviada)
        {
            await MainThread.InvokeOnMainThreadAsync(() => Dialogos.AlertaAsync(titulo, mensaje));
        }
    }
}
