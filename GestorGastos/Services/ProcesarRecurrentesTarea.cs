namespace GestorGastos.Services;

public class ProcesarRecurrentesTarea : ITareaInicio
{
    private readonly RecurrenteRepository _recurrentes;

    public ProcesarRecurrentesTarea(RecurrenteRepository recurrentes)
    {
        _recurrentes = recurrentes;
    }

    public async Task EjecutarAsync()
    {
        await _recurrentes.ProcesarVencidosAsync();
    }
}
