namespace GestorGastos.Services;

/// <summary>
/// Se ejecuta al iniciar sesión: si el teléfono está vacío y hay datos en la nube, los descarga;
/// si no, sube los datos locales. Después queda escuchando los cambios para subirlos solo.
/// </summary>
public class SincronizacionInicialTarea : ITareaInicio
{
    private readonly SincronizacionService _sync;

    public SincronizacionInicialTarea(SincronizacionService sync)
    {
        _sync = sync;
    }

    public async Task EjecutarAsync()
    {
        _sync.Iniciar();
        if (!SincronizacionService.HayInternet)
        {
            return;
        }

        try
        {
            if (await _sync.BaseLocalVaciaAsync())
            {
                var descargados = await _sync.DescargarAsync();
                if (descargados > 0)
                {
                    return;
                }
            }

            _ = _sync.SubirAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sincronización inicial: {ex.Message}");
        }
    }
}
