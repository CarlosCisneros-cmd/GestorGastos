using GestorGastos.Models;
using SQLite;

namespace GestorGastos.Data;

public class BaseDatos
{
    private const SQLiteOpenFlags Opciones = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    private readonly SemaphoreSlim _candado = new(1, 1);
    private SQLiteAsyncConnection? _conexion;
    private string _archivoAbierto = string.Empty;

    public string ArchivoActual { get; private set; } = "gastos_invitado.db3";

    /// <summary>Identificador del usuario (se usa como usuario_id en la nube).</summary>
    public string UsuarioId { get; private set; } = "invitado";

    /// <summary>Se dispara cada vez que se inserta, actualiza o elimina un registro.</summary>
    public event Action? DatosCambiados;

    public void SeleccionarUsuario(string googleId)
    {
        var limpio = new string(googleId.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrEmpty(limpio))
        {
            limpio = "invitado";
        }
        ArchivoActual = $"gastos_{limpio}.db3";
        UsuarioId = limpio;
    }

    public async Task<SQLiteAsyncConnection> ConexionAsync()
    {
        if (_conexion is not null && _archivoAbierto == ArchivoActual)
        {
            return _conexion;
        }

        await _candado.WaitAsync();
        try
        {
            if (_conexion is not null && _archivoAbierto == ArchivoActual)
            {
                return _conexion;
            }

            if (_conexion is not null)
            {
                await _conexion.CloseAsync();
                _conexion = null;
            }

            var ruta = FileAccessHelper.GetLocalFilePath(ArchivoActual);
            var conexion = new SQLiteAsyncConnection(ruta, Opciones);

            await conexion.CreateTablesAsync(CreateFlags.None,
                typeof(Usuario),
                typeof(Cuenta),
                typeof(Categoria),
                typeof(Transaccion),
                typeof(Comprobante),
                typeof(PagoRecurrente),
                typeof(PagoProximo),
                typeof(Presupuesto),
                typeof(MetaAhorro),
                typeof(AporteAhorro),
                typeof(Deuda),
                typeof(CuotaDeuda));

            await SembrarCategoriasAsync(conexion);

            conexion.GetConnection().TableChanged += (_, _) => DatosCambiados?.Invoke();

            _conexion = conexion;
            _archivoAbierto = ArchivoActual;
            return conexion;
        }
        finally
        {
            _candado.Release();
        }
    }

    public async Task CerrarAsync()
    {
        await _candado.WaitAsync();
        try
        {
            if (_conexion is not null)
            {
                await _conexion.CloseAsync();
                _conexion = null;
                _archivoAbierto = string.Empty;
            }
        }
        finally
        {
            _candado.Release();
        }
    }

    private static async Task SembrarCategoriasAsync(SQLiteAsyncConnection conexion)
    {
        var cantidad = await conexion.Table<Categoria>().CountAsync();
        if (cantidad > 0)
        {
            return;
        }

        var categorias = new List<Categoria>
        {
            Nueva("Comida", TiposCategoria.Gasto, "🍔", "#E0526B"),
            Nueva("Transporte", TiposCategoria.Gasto, "🚌", "#E59A2F"),
            Nueva("Hogar", TiposCategoria.Gasto, "🏠", "#8B5CF6"),
            Nueva("Servicios", TiposCategoria.Gasto, "💡", "#3B82F6"),
            Nueva("Salud", TiposCategoria.Gasto, "💊", "#14B8A6"),
            Nueva("Ocio", TiposCategoria.Gasto, "🎮", "#EC4899"),
            Nueva("Compras", TiposCategoria.Gasto, "🛍️", "#F97316"),
            Nueva("Estudios", TiposCategoria.Gasto, "📚", "#6366F1"),
            Nueva("Suscripciones", TiposCategoria.Gasto, "🔁", "#0EA5E9"),
            Nueva("Regalos", TiposCategoria.Gasto, "🎁", "#D946EF"),
            Nueva(CategoriasEspeciales.PagoDeudas, TiposCategoria.Gasto, "💳", "#B45309"),
            Nueva(CategoriasEspeciales.Ahorro, TiposCategoria.Gasto, "🐷", "#7C5CDB"),
            Nueva("Otros gastos", TiposCategoria.Gasto, "📦", "#8A8294"),
            Nueva("Sueldo", TiposCategoria.Ingreso, "💼", "#1F9D6B"),
            Nueva("Trabajo independiente", TiposCategoria.Ingreso, "💻", "#059669"),
            Nueva("Propinas y regalos", TiposCategoria.Ingreso, "🎉", "#10B981"),
            Nueva("Inversiones", TiposCategoria.Ingreso, "📈", "#16A34A"),
            Nueva("Otros ingresos", TiposCategoria.Ingreso, "💰", "#22C55E"),
            Nueva(CategoriasEspeciales.Transferencia, TiposCategoria.Ambos, "🔄", "#7C5CDB")
        };

        await conexion.InsertAllAsync(categorias);
    }

    private static Categoria Nueva(string nombre, string tipo, string icono, string color) => new()
    {
        Nombre = nombre,
        Tipo = tipo,
        Icono = icono,
        Color = color,
        EsPredeterminada = true,
        Activa = true
    };
}
