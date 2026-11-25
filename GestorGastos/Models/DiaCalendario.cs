using CommunityToolkit.Mvvm.ComponentModel;

namespace GestorGastos.Models;

public partial class DiaCalendario : ObservableObject
{
    [ObservableProperty]
    private bool seleccionado;

    public DateTime Fecha { get; set; }

    public bool EsDelMes { get; set; }

    public bool EsHoy { get; set; }

    public bool TieneIngresos { get; set; }

    public bool TieneGastos { get; set; }

    public bool TienePagos { get; set; }

    public int Fila { get; set; }

    public int Columna { get; set; }

    public string Numero => EsDelMes ? Fecha.Day.ToString() : string.Empty;

    public double Opacidad => EsDelMes ? 1 : 0;
}
