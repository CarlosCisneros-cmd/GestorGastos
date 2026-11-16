using GestorGastos.Helpers;

namespace GestorGastos.Models;

public class DeudaResumen
{
    public Deuda Deuda { get; set; } = new();

    public int CuotasPagadas { get; set; }

    public CuotaDeuda? ProximaCuota { get; set; }

    public decimal TotalAPagar { get; set; }

    public int Id => Deuda.Id;

    public string Nombre => Deuda.Nombre;

    public bool EstaPagada => Deuda.MontoPendiente <= 0;

    public double Progreso => TotalAPagar <= 0 ? 0 : Math.Min((double)((TotalAPagar - Deuda.MontoPendiente) / TotalAPagar), 1);

    public string PendienteTexto => Formato.Moneda(Deuda.MontoPendiente);

    public string CuotasTexto => $"{CuotasPagadas} de {Deuda.NumeroCuotas} cuotas pagadas";

    public string ProximaCuotaTexto => EstaPagada || ProximaCuota is null
        ? "Deuda pagada ✅"
        : $"Próxima cuota: {Formato.Moneda(ProximaCuota.Monto)} · {Formato.FechaCorta(ProximaCuota.FechaVencimiento)}";

    public string ColorEstado => EstaPagada ? "#1F9D6B" : (ProximaCuota is not null && ProximaCuota.FechaVencimiento.Date < DateTime.Today ? "#E0526B" : "#8A8294");
}
