namespace GestorGastos.Services;

public class ActualizarCuotasTarea : ITareaInicio
{
    private readonly DeudaRepository _deudas;

    public ActualizarCuotasTarea(DeudaRepository deudas)
    {
        _deudas = deudas;
    }

    public async Task EjecutarAsync()
    {
        await _deudas.ActualizarCuotasVencidasAsync();
    }
}
