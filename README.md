# Mercado Pago Checkout Pro Integration (.NET 10)

Este es un backend desarrollado en .NET 10 Core para integrarse con el producto **Checkout Pro** de Mercado Pago. Permite generar preferencias de pago y recibir notificaciones a través de webhooks.

## Requisitos Previos

- **.NET 10 SDK**: Instalado en el sistema.
- **Access Token de Mercado Pago**: Necesitas tus credenciales (Access Token) de tu cuenta de Mercado Pago (puedes usar el modo Sandbox para pruebas).

## Instalación y Configuración

### 1. Requisitos
- **.NET 10 SDK**: [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/10.0)

### 2. Configuración por Sistema Operativo

#### **En Linux (Arch/Ubuntu/Debian/etc.)**
Si el SDK no está en tu PATH global, configura las variables de entorno en tu terminal:
```bash
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
```
Para ejecutar el proyecto:
```bash
cd MercadoPagoIntegration
dotnet run
```

#### **En Windows (CMD/PowerShell)**
Asegúrate de que el instalador de .NET haya añadido `dotnet` a las variables de entorno del sistema.
Para ejecutar el proyecto:
```powershell
cd MercadoPagoIntegration
dotnet run
```

### 3. Configuración de Credenciales
Ya no es necesario configurar las credenciales en `appsettings.json`. Ahora se deben pasar directamente en cada petición al endpoint de creación de preferencia. Esto permite manejar múltiples cuentas o tokens dinámicamente.


---

## Endpoints

### 1. Crear Preferencia de Pago
Genera un `init_point` que permite redirigir al cliente al entorno de pago de Mercado Pago.

- **URL**: `POST /api/Checkout/create-preference`
- **Cuerpo (JSON)**:
  ```json
  {
    "title": "Nombre del Producto",
    "price": 15000,
    "quantity": 1,
    "accessToken": "TU_ACCESS_TOKEN",
    "publicKey": "TU_PUBLIC_KEY"
  }
  ```
- **Respuesta Exitosa (200 OK)**:
  ```json
  {
    "id": "PREF_ID",
    "init_point": "https://www.mercadopago.cl/checkout/v1/redirect?pref_id=...",
    "publicKey": "TU_PUBLIC_KEY"
  }
  ```
- **Descripción**: El `init_point` es la URL a la que debes redirigir al usuario en el frontend para que complete el pago.

### 2. Webhook (Notificaciones de Pago)
Recibe notificaciones automáticas de Mercado Pago cuando cambia el estado de un pago.

- **URL**: `POST /api/Checkout/webhook`
- **Query Parameters**:
  - `topic`: Indica el tipo de evento (ej: `payment`).
  - `id`: El ID del recurso asociado al evento.
- **Descripción**: Este endpoint procesa las notificaciones. Si el `topic` es `payment`, intenta parsear el `id` y registrar la acción en la consola.

> [!IMPORTANT]
> **Gestión de Tokens en Webhooks**:
> Dado que ahora los tokens se pasan por endpoint, al recibir un webhook deberías identificar a qué cuenta pertenece ese pago usando metadatos o el campo `external_reference` al crear la preferencia.

---

## Despliegue (FTP a Windows Server)

Para desplegar la aplicación en tu servidor Windows, se incluye el script `deploy_ftp.sh`. Este script automatiza la compilación para Windows y la subida de archivos.

### Instrucciones:
1. Asegúrate de tener configurado el sitio en IIS apuntando a la carpeta de destino.
2. Ejecuta el script de despliegue:
   ```bash
   ./deploy_ftp.sh
   ```
3. El script realizará un `dotnet publish` con el runtime `win-x64` y subirá los archivos vía FTP.

### Configuración de Auto-inicio (Reinicio de Servidor)
Para asegurar que la API se inicie automáticamente cuando el servidor Windows se reinicie (sin esperar a que alguien entre a la URL), realiza lo siguiente en el **IIS Manager**:

1. **Application Pool**:
   - Ve a **Application Pools** y selecciona `MercadoPagoIntegration`.
   - Haz clic en **Advanced Settings**.
   - Cambia **Start Mode** de `OnDemand` a `AlwaysRunning`.
   - Cambia **Idle Time-out (minutes)** de `20` a `0`.

2. **Sitio Web**:
   - Selecciona tu sitio en el árbol de la izquierda.
   - Haz clic en **Advanced Settings**.
   - Cambia **Preload Enabled** a `True`.

---

## Estructura del Proyecto

- `Controllers/CheckoutController.cs`: Expone los endpoints de la API.
- `Services/MercadoPagoService.cs`: Encapsula la lógica del SDK de Mercado Pago.
- `Models/CheckoutRequest.cs`: Modelo de datos para las peticiones.
- `deploy_ftp.sh`: Script de automatización de despliegue por FTP.

---

> [!TIP]
> Para probar los webhooks localmente, puedes usar herramientas como **ngrok** para exponer tu puerto local al internet.
