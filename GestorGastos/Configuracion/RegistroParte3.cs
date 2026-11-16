using GestorGastos.Services;
using GestorGastos.ViewModels;
using Plugin.LocalNotification;

namespace GestorGastos.Configuracion;

public static class RegistroParte3
{
    public static MauiAppBuilder AgregarParte3(this MauiAppBuilder builder)
    {
        builder.UseLocalNotification();

        builder.Services.AddSingleton<NotificacionService>();
        builder.Services.AddSingleton<PresupuestoRepository>();
        builder.Services.AddSingleton<AhorroRepository>();
        builder.Services.AddSingleton<DeudaRepository>();
        builder.Services.AddSingleton<IAlertaPresupuestoService, AlertaPresupuestoService>();
        builder.Services.AddSingleton<ITareaInicio, ActualizarCuotasTarea>();

        builder.Services.AddTransient<PresupuestosViewModel>();
        builder.Services.AddTransient<PresupuestoFormViewModel>();
        builder.Services.AddTransient<MetasAhorroViewModel>();
        builder.Services.AddTransient<MetaAhorroFormViewModel>();
        builder.Services.AddTransient<DeudasViewModel>();
        builder.Services.AddTransient<DeudaFormViewModel>();
        builder.Services.AddTransient<DeudaDetalleViewModel>();

        return builder;
    }
}
