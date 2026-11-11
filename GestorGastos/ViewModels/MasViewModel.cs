using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Models;
using GestorGastos.Services;

namespace GestorGastos.ViewModels;

public partial class MasViewModel : BaseViewModel
{
    private readonly SesionService _sesion;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string correo = string.Empty;

    [ObservableProperty]
    private string inicial = "U";

    public List<OpcionMenu> Opciones { get; } = new()
    {
        new OpcionMenu { Icono = "🏦", Titulo = "Cuentas", Descripcion = "Bancos, efectivo y tarjetas", Ruta = Rutas.Cuentas },
        new OpcionMenu { Icono = "🏷️", Titulo = "Categorías", Descripcion = "Clasifica tus movimientos", Ruta = Rutas.Categorias },
        new OpcionMenu { Icono = "📅", Titulo = "Calendario", Descripcion = "Movimientos y pagos por día", Ruta = Rutas.Calendario },
        new OpcionMenu { Icono = "🔁", Titulo = "Pagos recurrentes", Descripcion = "Pagos que se repiten", Ruta = Rutas.Recurrentes },
        new OpcionMenu { Icono = "⏰", Titulo = "Próximos pagos", Descripcion = "Pagos pendientes por vencer", Ruta = Rutas.ProximosPagos },
        new OpcionMenu { Icono = "🧾", Titulo = "Comprobantes", Descripcion = "Galería de facturas", Ruta = Rutas.Comprobantes },
        new OpcionMenu { Icono = "🎯", Titulo = "Metas de ahorro", Descripcion = "Ahorra para lo que quieres", Ruta = Rutas.Metas },
        new OpcionMenu { Icono = "💳", Titulo = "Deudas", Descripcion = "Préstamos y cuotas", Ruta = Rutas.Deudas },
        new OpcionMenu { Icono = "⚙️", Titulo = "Ajustes", Descripcion = "Seguridad y preferencias", Ruta = Rutas.Ajustes }
    };

    public MasViewModel(SesionService sesion)
    {
        _sesion = sesion;
    }

    public void Cargar()
    {
        var usuario = _sesion.UsuarioActual;
        if (usuario is null)
        {
            return;
        }

        Nombre = usuario.Nombre;
        Correo = usuario.Correo;
        Inicial = usuario.Inicial;
    }

    [RelayCommand]
    private async Task AbrirAsync(OpcionMenu? opcion)
    {
        if (opcion is null)
        {
            return;
        }

        await IrAAsync(opcion.Ruta);
    }
}
