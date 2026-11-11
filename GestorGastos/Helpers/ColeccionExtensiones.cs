using System.Collections.ObjectModel;

namespace GestorGastos.Helpers;

public static class ColeccionExtensiones
{
    public static void ReemplazarCon<T>(this ObservableCollection<T> coleccion, IEnumerable<T> elementos)
    {
        coleccion.Clear();
        foreach (var elemento in elementos)
        {
            coleccion.Add(elemento);
        }
    }
}
