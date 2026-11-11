namespace GestorGastos.Helpers;

public static class Dialogos
{
    private static Page? PaginaActual => Shell.Current?.CurrentPage ?? Application.Current?.Windows.FirstOrDefault()?.Page;

    public static Task AlertaAsync(string titulo, string mensaje)
    {
        var pagina = PaginaActual;
        return pagina is null ? Task.CompletedTask : pagina.DisplayAlertAsync(titulo, mensaje, "Aceptar");
    }

    public static async Task<bool> ConfirmarAsync(string titulo, string mensaje, string aceptar = "Sí", string cancelar = "Cancelar")
    {
        var pagina = PaginaActual;
        if (pagina is null) return false;
        return await pagina.DisplayAlertAsync(titulo, mensaje, aceptar, cancelar);
    }

    public static async Task<string?> ElegirAsync(string titulo, params string[] opciones)
    {
        var pagina = PaginaActual;
        if (pagina is null) return null;
        var resultado = await pagina.DisplayActionSheetAsync(titulo, "Cancelar", null, opciones);
        return resultado == "Cancelar" ? null : resultado;
    }

    public static async Task<string?> PedirTextoAsync(string titulo, string mensaje, string valorInicial = "", Keyboard? teclado = null)
    {
        var pagina = PaginaActual;
        if (pagina is null) return null;
        return await pagina.DisplayPromptAsync(titulo, mensaje, "Aceptar", "Cancelar", initialValue: valorInicial, keyboard: teclado ?? Keyboard.Default);
    }
}
