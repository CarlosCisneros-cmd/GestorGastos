using GestorGastos.Helpers;

namespace GestorGastos.Models;

public class PresupuestoProgreso
{
    public Presupuesto Presupuesto { get; set; } = new();

    public string CategoriaNombre { get; set; } = string.Empty;

    public string CategoriaIcono { get; set; } = "📦";

    public decimal Gastado { get; set; }

    public int Id => Presupuesto.Id;

    public string Nombre => string.IsNullOrWhiteSpace(Presupuesto.Nombre) ? CategoriaNombre : Presupuesto.Nombre;

    public string Titulo => $"{CategoriaIcono} {Nombre}";

    public decimal Limite => Presupuesto.Monto;

    public double Porcentaje => Limite <= 0 ? 0 : (double)(Gastado / Limite);

    public double Progreso => Math.Min(Porcentaje, 1);

    public bool EstaExcedido => Porcentaje >= 1 && Gastado > Limite;

    public bool EnAdvertencia => Porcentaje >= 0.75 && !EstaExcedido;

    public string PorcentajeTexto => $"{Math.Round(Porcentaje * 100):0}%";

    public string DetalleTexto => $"{Formato.Moneda(Gastado)} de {Formato.Moneda(Limite)} · {Presupuesto.Periodo}";

    public string RestanteTexto => EstaExcedido
        ? $"Excedido {Formato.Moneda(Gastado - Limite)}"
        : $"Quedan {Formato.Moneda(Limite - Gastado)}";

    public string ColorEstado => EstaExcedido ? "#E0526B" : (EnAdvertencia ? "#E59A2F" : "#1F9D6B");

    public string ColorFondoEstado => EstaExcedido ? "#FDE7EB" : (EnAdvertencia ? "#FDF0DC" : "#E2F6EC");
}
