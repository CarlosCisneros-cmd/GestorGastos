using GestorGastos.Helpers;

namespace GestorGastos.Models;

public class CuotaItem
{
    public CuotaItem(CuotaDeuda cuota, int totalCuotas)
    {
        Cuota = cuota;
        TotalCuotas = totalCuotas;
    }

    public CuotaDeuda Cuota { get; }

    public int TotalCuotas { get; }

    public int Id => Cuota.Id;

    public string Titulo => $"Cuota {Cuota.Numero} de {TotalCuotas}";

    public string MontoTexto => Formato.Moneda(Cuota.Monto);

    public bool EstaPagada => Cuota.Estado == EstadosCuota.Pagada;

    public bool PuedePagar => !EstaPagada;

    public bool EstaVencida => !EstaPagada && Cuota.FechaVencimiento.Date < DateTime.Today;

    public string EstadoTexto
    {
        get
        {
            if (EstaPagada)
            {
                return $"Pagada el {Formato.FechaCorta(Cuota.FechaPago ?? Cuota.FechaVencimiento)}";
            }

            return EstaVencida
                ? $"Vencida · {Formato.FechaCorta(Cuota.FechaVencimiento)}"
                : $"Vence {Formato.FechaCorta(Cuota.FechaVencimiento)}";
        }
    }

    public string ColorEstado => EstaPagada ? "#1F9D6B" : (EstaVencida ? "#E0526B" : "#8A8294");

    public string Icono => EstaPagada ? "✅" : (EstaVencida ? "⚠️" : "📅");
}
