using GestorGastos.Data;
using GestorGastos.Models;
using Plugin.Maui.OCR;

namespace GestorGastos.Repositories;

public class ComprobanteRepository
{
    private readonly BaseDatos _bd;
    private bool _ocrIniciado;

    public ComprobanteRepository(BaseDatos bd)
    {
        _bd = bd;
    }

    private static string Carpeta => Path.Combine(FileSystem.AppDataDirectory, "comprobantes");

    public async Task<string?> TomarFotoAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            throw new InvalidOperationException("Este dispositivo no tiene cámara disponible.");
        }

        var permiso = await Permissions.RequestAsync<Permissions.Camera>();
        if (permiso != PermissionStatus.Granted)
        {
            throw new InvalidOperationException("Necesitamos permiso para usar la cámara. Actívalo en los ajustes del teléfono.");
        }

        var foto = await MediaPicker.Default.CapturePhotoAsync();
        return foto is null ? null : await GuardarCopiaAsync(foto);
    }

    public async Task<string?> ElegirDeGaleriaAsync()
    {
        var foto = await MediaPicker.Default.PickPhotoAsync();
        return foto is null ? null : await GuardarCopiaAsync(foto);
    }

    public async Task<DatosFactura> LeerFacturaAsync(string ruta)
    {
        try
        {
            if (!_ocrIniciado)
            {
                await OcrPlugin.Default.InitAsync();
                _ocrIniciado = true;
            }

            var bytes = await File.ReadAllBytesAsync(ruta);
            var resultado = await OcrPlugin.Default.RecognizeTextAsync(bytes, true);
            if (!resultado.Success)
            {
                return new DatosFactura();
            }

            return AnalizadorFactura.Analizar(resultado.AllText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de OCR: {ex.Message}");
            return new DatosFactura();
        }
    }

    public async Task GuardarAsync(int transaccionId, string rutaImagen, DatosFactura? datos)
    {
        var conexion = await _bd.ConexionAsync();
        var anteriores = await conexion.Table<Comprobante>()
            .Where(c => c.TransaccionId == transaccionId)
            .ToListAsync();

        foreach (var anterior in anteriores.Where(a => a.RutaImagen != rutaImagen))
        {
            await conexion.DeleteAsync<Comprobante>(anterior.Id);
            BorrarArchivo(anterior.RutaImagen);
        }

        var existente = anteriores.FirstOrDefault(a => a.RutaImagen == rutaImagen);
        if (existente is not null)
        {
            return;
        }

        var comprobante = new Comprobante
        {
            TransaccionId = transaccionId,
            RutaImagen = rutaImagen,
            TextoDetectado = datos?.TextoCompleto ?? string.Empty,
            Comercio = datos?.Comercio ?? string.Empty,
            MontoDetectado = datos?.Total,
            FechaCaptura = DateTime.Now
        };
        await conexion.InsertAsync(comprobante);
    }

    public async Task<Comprobante?> ObtenerPorTransaccionAsync(int transaccionId)
    {
        var conexion = await _bd.ConexionAsync();
        return await conexion.Table<Comprobante>()
            .Where(c => c.TransaccionId == transaccionId)
            .FirstOrDefaultAsync();
    }

    public async Task QuitarDeTransaccionAsync(int transaccionId)
    {
        var conexion = await _bd.ConexionAsync();
        var lista = await conexion.Table<Comprobante>()
            .Where(c => c.TransaccionId == transaccionId)
            .ToListAsync();

        foreach (var comprobante in lista)
        {
            await conexion.DeleteAsync<Comprobante>(comprobante.Id);
            BorrarArchivo(comprobante.RutaImagen);
        }
    }

    public async Task<List<ComprobanteDetalle>> ObtenerGaleriaAsync(DateTime desde, DateTime hasta, int? categoriaId)
    {
        var conexion = await _bd.ConexionAsync();
        var sql = @"
SELECT p.Id, p.TransaccionId, p.RutaImagen, p.Comercio, p.TextoDetectado, p.FechaCaptura,
       t.Nombre AS NombreMovimiento, t.Monto AS Monto, t.Fecha AS Fecha, t.CategoriaId AS CategoriaId,
       IFNULL(c.Nombre, '') AS CategoriaNombre, IFNULL(c.Icono, '') AS CategoriaIcono
FROM Comprobantes p
INNER JOIN Transacciones t ON t.Id = p.TransaccionId
LEFT JOIN Categorias c ON c.Id = t.CategoriaId
WHERE t.Fecha >= ? AND t.Fecha < ?";

        var parametros = new List<object> { desde.Date, hasta.Date };
        if (categoriaId.HasValue)
        {
            sql += " AND t.CategoriaId = ?";
            parametros.Add(categoriaId.Value);
        }

        sql += " ORDER BY t.Fecha DESC, p.Id DESC";
        return await conexion.QueryAsync<ComprobanteDetalle>(sql, parametros.ToArray());
    }

    public async Task<ComprobanteDetalle?> ObtenerDetalleAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        const string sql = @"
SELECT p.Id, p.TransaccionId, p.RutaImagen, p.Comercio, p.TextoDetectado, p.FechaCaptura,
       t.Nombre AS NombreMovimiento, t.Monto AS Monto, t.Fecha AS Fecha, t.CategoriaId AS CategoriaId,
       IFNULL(c.Nombre, '') AS CategoriaNombre, IFNULL(c.Icono, '') AS CategoriaIcono
FROM Comprobantes p
INNER JOIN Transacciones t ON t.Id = p.TransaccionId
LEFT JOIN Categorias c ON c.Id = t.CategoriaId
WHERE p.Id = ?";
        var lista = await conexion.QueryAsync<ComprobanteDetalle>(sql, id);
        return lista.FirstOrDefault();
    }

    public async Task EliminarAsync(int id)
    {
        var conexion = await _bd.ConexionAsync();
        var comprobante = await conexion.FindAsync<Comprobante>(id);
        if (comprobante is null)
        {
            return;
        }

        await conexion.DeleteAsync<Comprobante>(id);
        BorrarArchivo(comprobante.RutaImagen);
    }

    public void DescartarArchivo(string? ruta)
    {
        if (!string.IsNullOrEmpty(ruta))
        {
            BorrarArchivo(ruta);
        }
    }

    private static async Task<string> GuardarCopiaAsync(FileResult foto)
    {
        Directory.CreateDirectory(Carpeta);
        var extension = Path.GetExtension(foto.FileName);
        if (string.IsNullOrEmpty(extension))
        {
            extension = ".jpg";
        }

        var destino = Path.Combine(Carpeta, $"comprobante_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..6]}{extension}");
        await using var origen = await foto.OpenReadAsync();
        await using var archivo = File.Create(destino);
        await origen.CopyToAsync(archivo);
        return destino;
    }

    private static void BorrarArchivo(string ruta)
    {
        try
        {
            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudo borrar la imagen: {ex.Message}");
        }
    }
}
