using Android.App;
using Android.Content;
using Android.Content.PM;
using GestorGastos.Configuracion;
using Microsoft.Maui.Authentication;

namespace GestorGastos;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = AuthConfig.EsquemaRetorno)]
public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
{
}
