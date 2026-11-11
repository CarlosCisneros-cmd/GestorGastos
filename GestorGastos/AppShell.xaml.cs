using GestorGastos.Configuracion;
using GestorGastos.Views;

namespace GestorGastos;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(Rutas.Cuentas, typeof(vCuentas));
        Routing.RegisterRoute(Rutas.CuentaForm, typeof(vCuentaForm));
        Routing.RegisterRoute(Rutas.Categorias, typeof(vCategorias));
        Routing.RegisterRoute(Rutas.CategoriaForm, typeof(vCategoriaForm));
        Routing.RegisterRoute(Rutas.Ajustes, typeof(vAjustes));

        Routing.RegisterRoute(Rutas.EditarMovimiento, typeof(vRegistrarMovimiento));
        Routing.RegisterRoute(Rutas.Calendario, typeof(vCalendario));
        Routing.RegisterRoute(Rutas.Recurrentes, typeof(vRecurrentes));
        Routing.RegisterRoute(Rutas.RecurrenteForm, typeof(vRecurrenteForm));
        Routing.RegisterRoute(Rutas.ProximosPagos, typeof(vProximosPagos));
        Routing.RegisterRoute(Rutas.ProximoPagoForm, typeof(vProximoPagoForm));
        Routing.RegisterRoute(Rutas.Comprobantes, typeof(vGaleriaComprobantes));
        Routing.RegisterRoute(Rutas.ComprobanteDetalle, typeof(vComprobanteDetalle));

        Routing.RegisterRoute(Rutas.PresupuestoForm, typeof(vPresupuestoForm));
        Routing.RegisterRoute(Rutas.Metas, typeof(vMetasAhorro));
        Routing.RegisterRoute(Rutas.MetaForm, typeof(vMetaAhorroForm));
        Routing.RegisterRoute(Rutas.Deudas, typeof(vDeudas));
        Routing.RegisterRoute(Rutas.DeudaForm, typeof(vDeudaForm));
        Routing.RegisterRoute(Rutas.DeudaDetalle, typeof(vDeudaDetalle));
    }
}
