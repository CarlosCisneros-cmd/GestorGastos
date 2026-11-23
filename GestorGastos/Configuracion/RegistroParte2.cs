using GestorGastos.Services;
using GestorGastos.ViewModels;
using Plugin.Maui.OCR;

namespace GestorGastos.Configuracion;

public static class RegistroParte2
{
    public static MauiAppBuilder AgregarParte2(this MauiAppBuilder builder)
    {
        builder.UseOcr();

        builder.Services.AddSingleton<RecurrenteRepository>();
        builder.Services.AddSingleton<ProximoPagoRepository>();
        builder.Services.AddSingleton<ComprobanteRepository>();
        builder.Services.AddSingleton<ITareaInicio, ProcesarRecurrentesTarea>();

        builder.Services.AddTransient<MovimientosViewModel>();
        builder.Services.AddTransient<RegistrarMovimientoViewModel>();
        builder.Services.AddTransient<CalendarioViewModel>();
        builder.Services.AddTransient<RecurrentesViewModel>();
        builder.Services.AddTransient<RecurrenteFormViewModel>();
        builder.Services.AddTransient<ProximosPagosViewModel>();
        builder.Services.AddTransient<ProximoPagoFormViewModel>();
        builder.Services.AddTransient<GaleriaComprobantesViewModel>();
        builder.Services.AddTransient<ComprobanteDetalleViewModel>();

        return builder;
    }
}
