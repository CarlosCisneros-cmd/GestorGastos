namespace GestorGastos.Services;

public interface IAlertaPresupuestoService
{
    Task VerificarAsync(int categoriaId, DateTime fecha);
}

public class AlertaPresupuestoNula : IAlertaPresupuestoService
{
    public Task VerificarAsync(int categoriaId, DateTime fecha) => Task.CompletedTask;
}
