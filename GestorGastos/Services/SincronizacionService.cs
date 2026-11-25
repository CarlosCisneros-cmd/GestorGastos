using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GestorGastos.Configuracion;
using GestorGastos.Data;
using GestorGastos.Models;
using SQLite;

namespace GestorGastos.Services;

/// <summary>
/// Web service: sincroniza la base local (SQLite) con la base en la nube (Supabase / PostgreSQL)
/// usando su API REST (HTTP + JSON).
///  - SUBIR:    POST   /rest/v1/{tabla}   (insertar o actualizar)  y  DELETE /rest/v1/{tabla} (borrados)
///  - DESCARGAR: GET   /rest/v1/{tabla}?usuario_id=eq.{id}
/// La app sigue funcionando sin internet: todo se guarda primero en SQLite y se sube cuando hay conexión.
/// </summary>
public class SincronizacionService
{
    // Tablas que se sincronizan (las mismas que crea BaseDatos)
    private static readonly Type[] Tablas =
    {
        typeof(Usuario), typeof(Cuenta), typeof(Categoria), typeof(Transaccion), typeof(Comprobante),
        typeof(PagoRecurrente), typeof(PagoProximo), typeof(Presupuesto), typeof(MetaAhorro),
        typeof(AporteAhorro), typeof(Deuda), typeof(CuotaDeuda)
    };

    private const string ClaveUltima = "sync_ultima";
    private const string FormatoFecha = "yyyy-MM-dd'T'HH:mm:ss";

    private readonly BaseDatos _bd;
    private readonly HttpClient _http;
    private readonly SemaphoreSlim _candado = new(1, 1);
    private CancellationTokenSource? _espera;
    private bool _iniciado;
    private bool _hayCambios;
    private bool _restaurando;

    public event Action? EstadoCambiado;

    public string Estado { get; private set; } = "Sin sincronizar";

    public bool Sincronizando { get; private set; }

    public DateTime? UltimaSincronizacion
    {
        get
        {
            var valor = Preferences.Default.Get(ClaveUltima, string.Empty);
            return DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha) ? fecha : null;
        }
        private set => Preferences.Default.Set(ClaveUltima, value?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty);
    }

    public SincronizacionService(BaseDatos bd)
    {
        _bd = bd;
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        if (SupabaseConfig.EstaConfigurado)
        {
            _http.BaseAddress = new Uri(SupabaseConfig.Url.TrimEnd('/') + "/rest/v1/");
            _http.DefaultRequestHeaders.Add("apikey", SupabaseConfig.ClavePublica);
            // Las claves antiguas "anon" son JWT (empiezan con eyJ) y también van como Bearer
            if (SupabaseConfig.ClavePublica.StartsWith("eyJ", StringComparison.Ordinal))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseConfig.ClavePublica);
            }
        }
        else
        {
            Estado = "Nube no configurada";
        }
    }

    public static bool HayInternet => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    /// <summary>Empieza a escuchar cambios en SQLite y en la conexión a internet.</summary>
    public void Iniciar()
    {
        if (_iniciado || !SupabaseConfig.EstaConfigurado)
        {
            return;
        }

        _iniciado = true;
        _bd.DatosCambiados += () =>
        {
            if (_restaurando)
            {
                return;
            }
            _hayCambios = true;
            ProgramarSubida();
        };
        Connectivity.Current.ConnectivityChanged += (_, e) =>
        {
            if (e.NetworkAccess == NetworkAccess.Internet && _hayCambios)
            {
                ProgramarSubida();
            }
        };
    }

    // Espera 3 segundos después del último cambio para no llamar al servidor en cada clic
    private void ProgramarSubida()
    {
        _espera?.Cancel();
        _espera = new CancellationTokenSource();
        var token = _espera.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), token);
                await SubirAsync();
            }
            catch (TaskCanceledException)
            {
            }
        });
    }

    /// <summary>Envía todos los datos locales del usuario a la nube.</summary>
    public async Task<bool> SubirAsync()
    {
        if (!SupabaseConfig.EstaConfigurado)
        {
            CambiarEstado("Nube no configurada");
            return false;
        }

        if (!HayInternet)
        {
            _hayCambios = true;
            CambiarEstado("Sin internet: los cambios se subirán al reconectar");
            return false;
        }

        await _candado.WaitAsync();
        try
        {
            Sincronizando = true;
            _hayCambios = false;
            CambiarEstado("Sincronizando...");

            var conexion = await _bd.ConexionAsync();
            var usuario = _bd.UsuarioId;

            foreach (var tipo in Tablas)
            {
                var mapa = await conexion.GetMappingAsync(tipo);
                var filas = await conexion.QueryAsync(mapa, $"SELECT * FROM \"{mapa.TableName}\"");

                var lista = new List<Dictionary<string, object?>>();
                var ids = new List<int>();
                foreach (var fila in filas)
                {
                    var registro = new Dictionary<string, object?> { ["usuario_id"] = usuario };
                    foreach (var columna in mapa.Columns)
                    {
                        registro[columna.Name] = ValorParaJson(columna.GetValue(fila));
                    }
                    registro["sincronizado"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                    lista.Add(registro);
                    ids.Add(Convert.ToInt32(mapa.PK!.GetValue(fila)));
                }

                if (lista.Count > 0)
                {
                    // POST con "merge-duplicates" = insertar o actualizar (upsert)
                    var json = JsonSerializer.Serialize(lista);
                    using var solicitud = new HttpRequestMessage(HttpMethod.Post, $"{mapa.TableName}?on_conflict=usuario_id,Id")
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                    solicitud.Headers.Add("Prefer", "resolution=merge-duplicates,return=minimal");
                    await EnviarAsync(solicitud, mapa.TableName);
                }

                // DELETE: quita de la nube lo que se eliminó en el teléfono
                var filtro = $"{mapa.TableName}?usuario_id=eq.{Uri.EscapeDataString(usuario)}";
                if (ids.Count > 0)
                {
                    filtro += $"&Id=not.in.({string.Join(',', ids)})";
                }
                using var borrar = new HttpRequestMessage(HttpMethod.Delete, filtro);
                await EnviarAsync(borrar, mapa.TableName);
            }

            UltimaSincronizacion = DateTime.Now;
            CambiarEstado($"Sincronizado {UltimaSincronizacion:dd/MM/yyyy HH:mm}");
            return true;
        }
        catch (Exception ex)
        {
            _hayCambios = true;
            CambiarEstado($"Error al sincronizar: {ex.Message}");
            return false;
        }
        finally
        {
            Sincronizando = false;
            _candado.Release();
            EstadoCambiado?.Invoke();
        }
    }

    /// <summary>Descarga los datos del usuario desde la nube y reemplaza los del teléfono.</summary>
    public async Task<int> DescargarAsync()
    {
        if (!SupabaseConfig.EstaConfigurado)
        {
            throw new InvalidOperationException("La nube no está configurada.");
        }

        if (!HayInternet)
        {
            throw new InvalidOperationException("No hay conexión a internet.");
        }

        await _candado.WaitAsync();
        try
        {
            Sincronizando = true;
            CambiarEstado("Descargando de la nube...");

            var conexion = await _bd.ConexionAsync();
            var usuario = _bd.UsuarioId;
            var datos = new Dictionary<Type, (TableMapping Mapa, List<object> Filas)>();
            var total = 0;

            foreach (var tipo in Tablas)
            {
                var mapa = await conexion.GetMappingAsync(tipo);
                using var respuesta = await _http.GetAsync($"{mapa.TableName}?usuario_id=eq.{Uri.EscapeDataString(usuario)}&select=*");
                var cuerpo = await respuesta.Content.ReadAsStringAsync();
                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"{mapa.TableName}: {(int)respuesta.StatusCode} {cuerpo}");
                }

                using var documento = JsonDocument.Parse(cuerpo);
                var filas = new List<object>();
                foreach (var elemento in documento.RootElement.EnumerateArray())
                {
                    var objeto = Activator.CreateInstance(tipo)!;
                    foreach (var columna in mapa.Columns)
                    {
                        if (elemento.TryGetProperty(columna.Name, out var valor))
                        {
                            columna.SetValue(objeto, ValorDesdeJson(valor, columna.ColumnType));
                        }
                    }
                    filas.Add(objeto);
                }

                datos[tipo] = (mapa, filas);
                total += filas.Count;
            }

            if (total == 0)
            {
                CambiarEstado("No hay datos en la nube para este usuario");
                return 0;
            }

            _restaurando = true;
            await conexion.RunInTransactionAsync(db =>
            {
                foreach (var (mapa, filas) in datos.Values)
                {
                    db.Execute($"DELETE FROM \"{mapa.TableName}\"");
                    foreach (var fila in filas)
                    {
                        db.Insert(fila, "OR REPLACE", fila.GetType());
                    }
                }
            });

            UltimaSincronizacion = DateTime.Now;
            CambiarEstado($"Restaurado desde la nube {UltimaSincronizacion:dd/MM/yyyy HH:mm}");
            return total;
        }
        finally
        {
            _restaurando = false;
            Sincronizando = false;
            _candado.Release();
            EstadoCambiado?.Invoke();
        }
    }

    /// <summary>True si el teléfono todavía no tiene cuentas ni movimientos.</summary>
    public async Task<bool> BaseLocalVaciaAsync()
    {
        var conexion = await _bd.ConexionAsync();
        var cuentas = await conexion.Table<Cuenta>().CountAsync();
        var movimientos = await conexion.Table<Transaccion>().CountAsync();
        return cuentas == 0 && movimientos == 0;
    }

    private async Task EnviarAsync(HttpRequestMessage solicitud, string tabla)
    {
        using var respuesta = await _http.SendAsync(solicitud);
        if (!respuesta.IsSuccessStatusCode)
        {
            var detalle = await respuesta.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"{tabla}: {(int)respuesta.StatusCode} {detalle}");
        }
    }

    private void CambiarEstado(string estado)
    {
        Estado = estado;
        MainThread.BeginInvokeOnMainThread(() => EstadoCambiado?.Invoke());
    }

    private static object? ValorParaJson(object? valor) => valor switch
    {
        DateTime fecha => fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture),
        _ => valor
    };

    private static object? ValorDesdeJson(JsonElement valor, Type tipo)
    {
        if (valor.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var real = Nullable.GetUnderlyingType(tipo) ?? tipo;
        if (real == typeof(string)) return valor.GetString() ?? string.Empty;
        if (real == typeof(int)) return valor.GetInt32();
        if (real == typeof(long)) return valor.GetInt64();
        if (real == typeof(decimal)) return valor.GetDecimal();
        if (real == typeof(double)) return valor.GetDouble();
        if (real == typeof(bool)) return valor.GetBoolean();
        if (real == typeof(DateTime))
        {
            return DateTime.Parse(valor.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }
        return null;
    }
}
