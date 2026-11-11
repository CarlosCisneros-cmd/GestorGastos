using GestorGastos.Data;
using GestorGastos.Helpers;
using GestorGastos.Models;

namespace GestorGastos.Services;

public class SesionService
{
    private readonly BaseDatos _bd;
    private DateTime? _momentoPausa;

    public SesionService(BaseDatos bd)
    {
        _bd = bd;
    }

    public Usuario? UsuarioActual { get; private set; }

    public bool SesionIniciada => UsuarioActual is not null;

    public bool HaySesionGuardada => !string.IsNullOrEmpty(Preferencias.SesionGoogleId);

    public bool PuedeUsarHuella => HaySesionGuardada && Preferencias.HuellaActiva;

    public async Task IniciarAsync(PerfilGoogle perfil)
    {
        _bd.SeleccionarUsuario(perfil.Id);
        var conexion = await _bd.ConexionAsync();

        var usuario = await conexion.Table<Usuario>()
            .Where(u => u.GoogleId == perfil.Id)
            .FirstOrDefaultAsync();

        if (usuario is null)
        {
            usuario = new Usuario
            {
                GoogleId = perfil.Id,
                Nombre = perfil.Nombre,
                Correo = perfil.Correo,
                FotoUrl = perfil.FotoUrl,
                FechaRegistro = DateTime.Now,
                UltimoIngreso = DateTime.Now
            };
            await conexion.InsertAsync(usuario);
        }
        else
        {
            usuario.Nombre = perfil.Nombre;
            usuario.Correo = perfil.Correo;
            usuario.FotoUrl = perfil.FotoUrl;
            usuario.UltimoIngreso = DateTime.Now;
            await conexion.UpdateAsync(usuario);
        }

        if (Preferencias.SesionGoogleId != perfil.Id)
        {
            Preferencias.HuellaActiva = false;
        }

        Preferencias.SesionGoogleId = perfil.Id;
        UsuarioActual = usuario;
        _momentoPausa = null;
    }

    public async Task<bool> RestaurarAsync()
    {
        var googleId = Preferencias.SesionGoogleId;
        if (string.IsNullOrEmpty(googleId))
        {
            return false;
        }

        _bd.SeleccionarUsuario(googleId);
        var conexion = await _bd.ConexionAsync();
        var usuario = await conexion.Table<Usuario>()
            .Where(u => u.GoogleId == googleId)
            .FirstOrDefaultAsync();

        if (usuario is null)
        {
            return false;
        }

        usuario.UltimoIngreso = DateTime.Now;
        await conexion.UpdateAsync(usuario);
        UsuarioActual = usuario;
        _momentoPausa = null;
        return true;
    }

    public async Task CerrarAsync()
    {
        UsuarioActual = null;
        _momentoPausa = null;
        Preferencias.LimpiarSesion();
        SecureStorage.Default.RemoveAll();
        await _bd.CerrarAsync();
    }

    public void MarcarPausa()
    {
        if (SesionIniciada)
        {
            _momentoPausa = DateTime.Now;
        }
    }

    public bool DebeBloquear()
    {
        if (!SesionIniciada || _momentoPausa is null)
        {
            return false;
        }

        var minutos = Preferencias.BloqueoMinutos;
        var transcurrido = DateTime.Now - _momentoPausa.Value;
        _momentoPausa = null;

        if (minutos <= 0 || transcurrido.TotalMinutes < minutos)
        {
            return false;
        }

        UsuarioActual = null;
        return true;
    }
}
