using GestorGastos.Helpers;

namespace GestorGastos.Models;

public class MetaAhorroItem
{
    public MetaAhorroItem(MetaAhorro meta)
    {
        Meta = meta;
    }

    public MetaAhorro Meta { get; }

    public int Id => Meta.Id;

    public string Titulo => $"{Meta.Icono} {Meta.Nombre}";

    public double Progreso => Meta.MontoObjetivo <= 0 ? 0 : Math.Min((double)(Meta.MontoActual / Meta.MontoObjetivo), 1);

    public string PorcentajeTexto => $"{Math.Round(Progreso * 100):0}%";

    public string AvanceTexto => $"{Formato.Moneda(Meta.MontoActual)} de {Formato.Moneda(Meta.MontoObjetivo)}";

    public bool Completada => Meta.MontoActual >= Meta.MontoObjetivo;

    public string FechaTexto
    {
        get
        {
            if (Completada)
            {
                return "¡Meta cumplida! 🎉";
            }

            if (!Meta.FechaLimite.HasValue)
            {
                return "Sin fecha límite";
            }

            var faltante = Meta.MontoObjetivo - Meta.MontoActual;
            var meses = Math.Max(1, ((Meta.FechaLimite.Value.Year - DateTime.Today.Year) * 12) + Meta.FechaLimite.Value.Month - DateTime.Today.Month);
            return $"Fecha límite: {Formato.FechaLarga(Meta.FechaLimite.Value)} · ahorra {Formato.Moneda(faltante / meses)}/mes";
        }
    }

    public string ColorAnillo => Completada ? "#1F9D6B" : "#7C5CDB";
}
