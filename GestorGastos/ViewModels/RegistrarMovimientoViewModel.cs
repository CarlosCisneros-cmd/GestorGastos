using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestorGastos.Configuracion;
using GestorGastos.Helpers;
using GestorGastos.Models;
using GestorGastos.Services;
using System.Collections.ObjectModel;
using static Android.Provider.ContactsContract.CommonDataKinds;

namespace GestorGastos.ViewModels;

public partial class RegistrarMovimientoViewModel : BaseViewModel, IQueryAttributable
{
    private readonly TransaccionRepository _transacciones;
    private readonly CuentaRepository _cuentas;
    private readonly CategoriaRepository _categorias;
    private readonly ComprobanteRepository _comprobantes;

    private int _id;
    private bool _edicionCargada;
    private DatosFactura? _datosFactura;
    private string? _rutaOriginal;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsGasto))]
    [NotifyPropertyChangedFor(nameof(EsIngreso))]
    [NotifyPropertyChangedFor(nameof(EsTransferencia))]
    [NotifyPropertyChangedFor(nameof(NoEsTransferencia))]
    [NotifyPropertyChangedFor(nameof(TextoBoton))]
    [NotifyPropertyChangedFor(nameof(ColorMonto))]
    private string tipo = TiposMovimiento.Gasto;

    [ObservableProperty]
    private string montoTexto = string.Empty;

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string nota = string.Empty;

    [ObservableProperty]
    private DateTime fecha = DateTime.Today;

    [ObservableProperty]
    private Cuenta? cuentaSeleccionada;

    [ObservableProperty]
    private Cuenta? cuentaDestino;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CategoriaTexto))]
    private Categoria? categoriaSeleccionada;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneComprobante))]
    private string? rutaComprobante;

    [ObservableProperty]
    private bool datosDetectados;

    [ObservableProperty]
    private string mensajeDeteccion = string.Empty;

    [ObservableProperty]
    private bool leyendoFactura;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBoton))]
    [NotifyPropertyChangedFor(nameof(PuedeEditar))]
    private bool esEdicion;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeEditar))]
    private bool esTransferenciaGuardada;

    public ObservableCollection<Cuenta> Cuentas { get; } = new();

    public ObservableCollection<Categoria> Categorias { get; } = new();

    public bool EsGasto => Tipo == TiposMovimiento.Gasto;

    public bool EsIngreso => Tipo == TiposMovimiento.Ingreso;

    public bool EsTransferencia => Tipo == TiposMovimiento.Transferencia;

    public bool NoEsTransferencia => !EsTransferencia;

    public string CategoriaTexto => CategoriaSeleccionada?.NombreConIcono ?? "Ninguna (toca una categoría)";

    public bool TieneComprobante => !string.IsNullOrEmpty(RutaComprobante);

    public bool PuedeEditar => !EsTransferenciaGuardada;

    public string ColorMonto => Tipo switch
    {
        TiposMovimiento.Ingreso => "#1F9D6B",
        TiposMovimiento.Transferencia => "#7C5CDB",
        _ => "#E0526B"
    };

    public string TextoBoton => EsEdicion ? "Guardar cambios" : Tipo switch
    {
        TiposMovimiento.Ingreso => "Guardar ingreso",
        TiposMovimiento.Transferencia => "Guardar transferencia",
        _ => "Guardar gasto"
    };

    public RegistrarMovimientoViewModel(TransaccionRepository transacciones, CuentaRepository cuentas, CategoriaRepository categorias, ComprobanteRepository comprobantes)
    {
        _transacciones = transacciones;
        _cuentas = cuentas;
        _categorias = categorias;
        _comprobantes = comprobantes;
        Titulo = "Nuevo movimiento";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var valor) && valor is int id)
        {
            _id = id;
            EsEdicion = true;
            Titulo = "Editar movimiento";
        }
    }

    public async Task CargarAsync()
    {
        try
        {
            var idCuentaActual = CuentaSeleccionada?.Id;
            var idDestinoActual = CuentaDestino?.Id;
            Cuentas.ReemplazarCon(await _cuentas.ObtenerActivasAsync());
            CuentaSeleccionada = Cuentas.FirstOrDefault(c => c.Id == idCuentaActual) ?? Cuentas.FirstOrDefault();
            CuentaDestino = Cuentas.FirstOrDefault(c => c.Id == idDestinoActual);

            if (EsEdicion && !_edicionCargada)
            {
                _edicionCargada = true;
                await CargarEdicionAsync();
                return;
            }

            await CargarCategoriasAsync(CategoriaSeleccionada?.Id);
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("No se pudo cargar", ex.Message);
        }
    }

    private async Task CargarEdicionAsync()
    {
        var transaccion = await _transacciones.ObtenerPorIdAsync(_id);
        if (transaccion is null)
        {
            await Dialogos.AlertaAsync("Movimiento", "El movimiento ya no existe.");
            await VolverAsync();
            return;
        }

        EsTransferenciaGuardada = transaccion.EsTransferencia;
        Tipo = transaccion.EsTransferencia ? TiposMovimiento.Transferencia : transaccion.Tipo;
        MontoTexto = Formato.MontoParaEditar(transaccion.Monto);
        Nombre = transaccion.Nombre;
        Nota = transaccion.Nota;
        Fecha = transaccion.Fecha;
        CuentaSeleccionada = Cuentas.FirstOrDefault(c => c.Id == transaccion.CuentaId);
        await CargarCategoriasAsync(transaccion.CategoriaId);

        var comprobante = await _comprobantes.ObtenerPorTransaccionAsync(_id);
        RutaComprobante = comprobante?.RutaImagen;
        _rutaOriginal = RutaComprobante;
    }

    private async Task CargarCategoriasAsync(int? seleccionarId)
    {
        if (EsTransferencia)
        {
            Categorias.Clear();
            CategoriaSeleccionada = null;
            return;
        }

        var tipoCategoria = EsIngreso ? TiposCategoria.Ingreso : TiposCategoria.Gasto;
        var lista = await _categorias.ObtenerPorTipoAsync(tipoCategoria);
        Categorias.ReemplazarCon(lista);
        CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == seleccionarId);
    }

    [RelayCommand]
    private async Task CambiarTipoAsync(string nuevoTipo)
    {
        if (EsTransferenciaGuardada || Tipo == nuevoTipo)
        {
            return;
        }

        if (EsEdicion && nuevoTipo == TiposMovimiento.Transferencia)
        {
            await Dialogos.AlertaAsync("Transferencia", "Un gasto o ingreso guardado no se puede convertir en transferencia. Crea una transferencia nueva.");
            return;
        }

        Tipo = nuevoTipo;
        await CargarCategoriasAsync(null);
    }

    [RelayCommand]
    private async Task EscanearFacturaAsync()
    {
        string? ruta;
        try
        {
            ruta = await _comprobantes.TomarFotoAsync();
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("Cámara", ex.Message);
            return;
        }

        if (ruta is null)
        {
            return;
        }

        await ProcesarImagenAsync(ruta, true);
    }

    [RelayCommand]
    private async Task AdjuntarDeGaleriaAsync()
    {
        string? ruta;
        try
        {
            ruta = await _comprobantes.ElegirDeGaleriaAsync();
        }
        catch (Exception ex)
        {
            await Dialogos.AlertaAsync("Galería", ex.Message);
            return;
        }

        if (ruta is null)
        {
            return;
        }

        var leer = await Dialogos.ConfirmarAsync("Comprobante", "¿Quieres que la app lea los datos de esta factura?", "Leer datos", "Solo adjuntar");
        await ProcesarImagenAsync(ruta, leer);
    }

    private async Task ProcesarImagenAsync(string ruta, bool leerDatos)
    {
        if (RutaComprobante != _rutaOriginal)
        {
            _comprobantes.DescartarArchivo(RutaComprobante);
        }

        RutaComprobante = ruta;
        DatosDetectados = false;
        MensajeDeteccion = string.Empty;
        _datosFactura = null;

        if (!leerDatos)
        {
            return;
        }

        try
        {
            LeyendoFactura = true;
            var datos = await _comprobantes.LeerFacturaAsync(ruta);
            _datosFactura = datos;

            if (EsTransferencia)
            {
                Tipo = TiposMovimiento.Gasto;
                await CargarCategoriasAsync(null);
            }

            if (datos.Total.HasValue)
            {
                MontoTexto = Formato.MontoParaEditar(datos.Total.Value);
            }

            if (datos.Fecha.HasValue)
            {
                Fecha = datos.Fecha.Value;
            }

            if (!string.IsNullOrWhiteSpace(datos.Comercio))
            {
                Nombre = datos.Comercio;
            }

            DatosDetectados = datos.Exito;
            MensajeDeteccion = datos.Exito
                ? "Datos detectados en la factura. Revisa la información y elige la categoría antes de guardar."
                : "No pudimos leer el total de la factura. La foto quedó adjunta; ingresa el monto manualmente.";
        }
        finally
        {
            LeyendoFactura = false;
        }
    }

    [RelayCommand]
    private void QuitarComprobante()
    {
        if (RutaComprobante != _rutaOriginal)
        {
            _comprobantes.DescartarArchivo(RutaComprobante);
        }

        RutaComprobante = null;
        DatosDetectados = false;
        MensajeDeteccion = string.Empty;
        _datosFactura = null;
    }

    [RelayCommand]
    private void SeleccionarCategoria(Categoria? categoria)
    {
        CategoriaSeleccionada = categoria;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (!Formato.IntentarLeerMonto(MontoTexto, out var monto) || monto <= 0)
            {
                throw new InvalidOperationException("Ingresa un monto válido mayor a cero.");
            }

            if (CuentaSeleccionada is null)
            {
                throw new InvalidOperationException("Primero crea o selecciona una cuenta.");
            }

            if (EsTransferencia)
            {
                if (CuentaDestino is null)
                {
                    throw new InvalidOperationException("Selecciona la cuenta de destino.");
                }

                await _transacciones.RegistrarTransferenciaAsync(CuentaSeleccionada.Id, CuentaDestino.Id, monto, Fecha, Nota);
                await TerminarAsync("Transferencia registrada");
                return;
            }

            if (CategoriaSeleccionada is null)
            {
                throw new InvalidOperationException("Elige una categoría.");
            }

            var transaccion = EsEdicion
                ? await _transacciones.ObtenerPorIdAsync(_id) ?? new Transaccion()
                : new Transaccion();

            transaccion.Tipo = Tipo;
            transaccion.Monto = monto;
            transaccion.Nombre = Nombre;
            transaccion.Nota = Nota;
            transaccion.Fecha = Fecha.Date;
            transaccion.CuentaId = CuentaSeleccionada.Id;
            transaccion.CategoriaId = CategoriaSeleccionada.Id;

            var idGuardado = await _transacciones.GuardarAsync(transaccion);

            if (!string.IsNullOrEmpty(RutaComprobante))
            {
                await _comprobantes.GuardarAsync(idGuardado, RutaComprobante, _datosFactura);
            }
            else if (!string.IsNullOrEmpty(_rutaOriginal))
            {
                await _comprobantes.QuitarDeTransaccionAsync(idGuardado);
            }

            await TerminarAsync(EsIngreso ? "Ingreso registrado" : "Gasto registrado");
        });
    }

    [RelayCommand]
    private async Task EliminarAsync()
    {
        var mensaje = EsTransferenciaGuardada
            ? "Se eliminarán los dos movimientos de la transferencia y se devolverán los saldos."
            : "El saldo de la cuenta se ajustará automáticamente.";
        var confirmar = await Dialogos.ConfirmarAsync("Eliminar movimiento", mensaje, "Eliminar");
        if (!confirmar)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _transacciones.EliminarAsync(_id);
            await VolverAsync();
        });
    }

    private async Task TerminarAsync(string mensaje)
    {
        if (EsEdicion)
        {
            await VolverAsync();
            return;
        }

        Limpiar();
        await Dialogos.AlertaAsync("Listo", mensaje);
        await Shell.Current.GoToAsync($"//{Rutas.Inicio}");
    }

    private void Limpiar()
    {
        MontoTexto = string.Empty;
        Nombre = string.Empty;
        Nota = string.Empty;
        Fecha = DateTime.Today;
        CategoriaSeleccionada = null;
        CuentaDestino = null;
        RutaComprobante = null;
        _rutaOriginal = null;
        _datosFactura = null;
        DatosDetectados = false;
        MensajeDeteccion = string.Empty;
    }
}
