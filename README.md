# Gestor de Gastos Personales

Aplicación móvil para registrar ingresos y gastos, organizarlos por cuentas y categorías, controlar presupuestos, metas de ahorro y deudas, y saber en todo momento cuánto dinero hay disponible.

Proyecto final de la asignatura **Desarrollo de Aplicaciones Móviles** — Universidad Tecnológica Israel, carrera de Sistemas de Información.

## Integrantes

| Integrante | Parte desarrollada |
|---|---|
| Carlos Alejandro Cisneros Lugo | Parte 1: base del proyecto, inicio de sesión, inicio, cuentas, categorías y ajustes. Integración final, servicio web (Supabase) e inicio de sesión con Google |
| Daniel Quiñonez | Parte 2: registrar movimientos, historial, calendario, pagos recurrentes, próximos pagos y comprobantes |
| Efrain Salazar | Parte 3: presupuestos con alertas, metas de ahorro y deudas |

## Tecnologías

- **.NET MAUI** (.NET 10, C# y XAML) — aplicación para Android e iOS.
- **SQLite** (`sqlite-net-pcl`) — base de datos dentro del teléfono; la app funciona sin internet.
- **Supabase** (PostgreSQL + API REST) — base de datos en la nube y servicio web.
- **Google OAuth 2.0** — inicio de sesión con Google; huella digital con `Plugin.Maui.Biometric`.
- **Plugin.Maui.OCR** — lectura de facturas con la cámara.
- **Plugin.LocalNotification** — alertas de presupuesto y de saldo bajo.

## Estructura del proyecto

```
GestorGastos.sln
GestorGastos/
├── Configuracion/   Configuración de Google (AuthConfig) y Supabase (SupabaseConfig)
├── Data/            BaseDatos.cs: conexión SQLite y creación de tablas
├── Models/          Tablas y modelos
├── Repositories/    Leer y guardar en SQLite (CuentaRepository, TransaccionRepository, ...)
├── Services/        Servicio web (SincronizacionService), Google, huella, notificaciones, OCR
├── Utils/           FileAccessHelper (ruta de la base de datos)
├── ViewModels/      Lógica de cada pantalla (MVVM)
├── Views/           Pantallas (vInicio, vCuentas, vRegistrarMovimiento, ...)
└── Platforms/       Archivos de Android e iOS
BaseDeDatos/
└── supabase_tablas.sql   Script para crear las tablas en Supabase
```

## Servicio web (conexión con la base de datos en la nube)

El servicio web está implementado en `GestorGastos/Services/SincronizacionService.cs` y usa la **API REST de Supabase** (HTTP + JSON):

| Acción | Método | Endpoint |
|---|---|---|
| Subir / actualizar registros | `POST` | `/rest/v1/{Tabla}?on_conflict=usuario_id,Id` (upsert) |
| Eliminar registros borrados en el teléfono | `DELETE` | `/rest/v1/{Tabla}?usuario_id=eq.{usuario}&Id=not.in.(...)` |
| Descargar los datos del usuario | `GET` | `/rest/v1/{Tabla}?usuario_id=eq.{usuario}&select=*` |

Tablas: `Usuarios`, `Cuentas`, `Categorias`, `Transacciones`, `Comprobantes`, `PagosRecurrentes`, `PagosProximos`, `Presupuestos`, `MetasAhorro`, `AportesAhorro`, `Deudas`, `CuotasDeuda`.

Funcionamiento:
1. Todo se guarda primero en SQLite (funciona sin internet).
2. Unos segundos después de cada cambio, la app envía los datos a Supabase.
3. Si no hay internet, los cambios se envían al volver la conexión.
4. Al iniciar sesión en un teléfono vacío, la app descarga los datos del usuario.
5. En **Más → Ajustes → Nube (web service)** están los botones **Sincronizar ahora** y **Restaurar**.

## Cómo ejecutar el proyecto

1. Instalar **Visual Studio 2026** con la carga de trabajo **Desarrollo de interfaz de usuario de aplicaciones multiplataforma de .NET (.NET MAUI)** (incluye el SDK de Android y OpenJDK).
2. Clonar el repositorio:
   ```
   git clone https://github.com/CarlosCisneros-cmd/GestorGastos.git
   ```
3. Abrir `GestorGastos.sln` en Visual Studio. Los paquetes NuGet (incluido SQLite) se descargan solos al compilar; no hay que instalar ninguna base de datos.
4. Conectar un celular Android con **Depuración USB** activada (o crear un emulador en *Herramientas → Android → Android Device Manager*).
5. Elegir el dispositivo junto al botón ▶ y presionar ▶.
6. En la pantalla de inicio se puede entrar con **Google** o con **Entrar en modo prueba**.

## Configuración (si se usa otra cuenta de Supabase o de Google)

**Supabase (base de datos en la nube):**
1. Crear un proyecto en https://supabase.com.
2. En *SQL Editor* ejecutar el script `BaseDeDatos/supabase_tablas.sql`.
3. Copiar la *Project URL* y la *Publishable key* en `GestorGastos/Configuracion/SupabaseConfig.cs`.

**Google (inicio de sesión):**
1. En https://console.cloud.google.com crear un proyecto y configurar *Google Auth Platform* (usuarios externos, agregar usuarios de prueba).
2. Crear un *ID de cliente de OAuth* de tipo **iOS** con el ID de paquete `com.uisrael.gestorgastos`.
3. Copiar el Client ID en `GestorGastos/Configuracion/AuthConfig.cs` (`GoogleClientId` y `EsquemaRetorno`).

> Las claves incluidas en el código son **públicas** (la *publishable key* de Supabase y el *Client ID* de Google); no se incluye ninguna clave secreta.
