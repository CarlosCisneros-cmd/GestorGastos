using System.Globalization;
using GestorGastos.Configuracion;
using GestorGastos.Data;
using GestorGastos.Helpers;
using GestorGastos.Services;
using GestorGastos.ViewModels;
using GestorGastos.Views;
using Microsoft.Extensions.Logging;

namespace GestorGastos;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        CultureInfo.DefaultThreadCurrentCulture = Formato.Cultura;
        CultureInfo.DefaultThreadCurrentUICulture = Formato.Cultura;

        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddSingleton<BaseDatos>();
        builder.Services.AddSingleton<SesionService>();
        builder.Services.AddSingleton<GoogleAuthService>();
        builder.Services.AddSingleton<BiometriaService>();
        builder.Services.AddSingleton<CuentaRepository>();
        builder.Services.AddSingleton<CategoriaRepository>();
        builder.Services.AddSingleton<TransaccionRepository>();
        builder.Services.AddSingleton<ResumenRepository>();
        builder.Services.AddSingleton<IAlertaPresupuestoService, AlertaPresupuestoNula>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<InicioViewModel>();
        builder.Services.AddTransient<MasViewModel>();
        builder.Services.AddTransient<AjustesViewModel>();
        builder.Services.AddTransient<CuentasViewModel>();
        builder.Services.AddTransient<CuentaFormViewModel>();
        builder.Services.AddTransient<CategoriasViewModel>();
        builder.Services.AddTransient<CategoriaFormViewModel>();

        builder.Services.AddTransient<vLogin>();
        builder.Services.AddTransient<vInicio>();
        builder.Services.AddTransient<vMas>();
        builder.Services.AddTransient<vAjustes>();
        builder.Services.AddTransient<vCuentas>();
        builder.Services.AddTransient<vCuentaForm>();
        builder.Services.AddTransient<vCategorias>();
        builder.Services.AddTransient<vCategoriaForm>();

        builder.Services.AddTransient<vMovimientos>();
        builder.Services.AddTransient<vRegistrarMovimiento>();
        builder.Services.AddTransient<vCalendario>();
        builder.Services.AddTransient<vRecurrentes>();
        builder.Services.AddTransient<vRecurrenteForm>();
        builder.Services.AddTransient<vProximosPagos>();
        builder.Services.AddTransient<vProximoPagoForm>();
        builder.Services.AddTransient<vGaleriaComprobantes>();
        builder.Services.AddTransient<vComprobanteDetalle>();

        builder.Services.AddTransient<vPresupuestos>();
        builder.Services.AddTransient<vPresupuestoForm>();
        builder.Services.AddTransient<vMetasAhorro>();
        builder.Services.AddTransient<vMetaAhorroForm>();
        builder.Services.AddTransient<vDeudas>();
        builder.Services.AddTransient<vDeudaForm>();
        builder.Services.AddTransient<vDeudaDetalle>();

        builder.AgregarParte2();
        builder.AgregarParte3();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
