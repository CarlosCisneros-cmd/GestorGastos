namespace GestorGastos.Helpers;

public static class Periodos
{
    public static DateTime InicioMes(DateTime fecha) => new(fecha.Year, fecha.Month, 1);

    public static DateTime InicioMesSiguiente(DateTime fecha) => InicioMes(fecha).AddMonths(1);

    public static DateTime FinMes(DateTime fecha) => InicioMesSiguiente(fecha).AddDays(-1);

    public static DateTime InicioSemana(DateTime fecha)
    {
        var dia = (int)fecha.DayOfWeek;
        var diferencia = dia == 0 ? 6 : dia - 1;
        return fecha.Date.AddDays(-diferencia);
    }

    public static DateTime SumarMeses(DateTime fecha, int meses, int diaOriginal)
    {
        var destino = new DateTime(fecha.Year, fecha.Month, 1).AddMonths(meses);
        var dia = Math.Min(diaOriginal, DateTime.DaysInMonth(destino.Year, destino.Month));
        return new DateTime(destino.Year, destino.Month, dia);
    }
}
