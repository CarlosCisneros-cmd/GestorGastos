using CommunityToolkit.Mvvm.ComponentModel;
using GestorGastos.Helpers;

namespace GestorGastos.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoEstaOcupado))]
    private bool estaOcupado;

    [ObservableProperty]
    private string titulo = string.Empty;

    public bool NoEstaOcupado => !EstaOcupado;

    protected async Task EjecutarAsync(Func<Task> accion)
    {
        if (EstaOcupado)
        {
            return;
        }

        try
        {
            EstaOcupado = true;
            await accion();
        }
        catch (InvalidOperationException ex)
        {
            await Dialogos.AlertaAsync("Atención", ex.Message);
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("Ocurrió un error", ex.Message);
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    protected static Task IrAAsync(string ruta)
    {
        return Shell.Current.GoToAsync(ruta);
    }

    protected static Task IrAAsync(string ruta, IDictionary<string, object> parametros)
    {
        return Shell.Current.GoToAsync(ruta, parametros);
    }

    protected static Task VolverAsync()
    {
        return Shell.Current.GoToAsync("..");
    }
}
