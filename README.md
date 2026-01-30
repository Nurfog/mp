# Mercado Pago Checkout Pro Integration (.NET 10)

Este es un backend desarrollado en .NET 10 Core para integrarse con el producto **Checkout Pro** de Mercado Pago. Permite generar preferencias de pago y recibir notificaciones a través de webhooks.

## Requisitos Previos

- **.NET 10 SDK**: Instalado en el sistema.
- **Access Token de Mercado Pago**: Necesitas tus credenciales (Access Token) de tu cuenta de Mercado Pago (puedes usar el modo Sandbox para pruebas).

## Entorno de Producción

La API está desplegada y accesible en:
👉 **[https://apimp.norteamericano.cl/](https://apimp.norteamericano.cl/)**

### Documentación Interactiva (UI)
Puedes probar los endpoints directamente desde el navegador usando Scalar:
👉 **[https://apimp.norteamericano.cl/scalar](https://apimp.norteamericano.cl/scalar)**

---

## Endpoints Principales

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

### 2. Webhook (Notificaciones de Pago)
Recibe notificaciones automáticas de Mercado Pago.
- **URL**: `POST /api/Checkout/webhook?topic=payment&id={id_recurso}`

---

## Despliegue (FTP a Windows Server)

Para desplegar la aplicación en el servidor Windows, usa el script `deploy_ftp.sh`.

### Proceso de Despliegue:
1. **Compilación**: Genera los binarios para `win-x64`.
2. **app_offline.htm**: El script sube automáticamente un archivo de mantenimiento para detener el sitio temporalmente y permitir la sobreescritura de archivos bloqueados.
3. **Subida**: Transfiere los archivos vía FTP a `ftp://norteamericano.com/apimp`.
4. **Reactivación**: Elimina el archivo `app_offline.htm` para que el sitio vuelva a estar en línea.

Ejecución:
```bash
./deploy_ftp.sh
```

---

## Configuración de IIS (Auto-inicio)

Para que la API esté siempre activa y arranque con el servidor:

1. **Application Pool**:
   - **Start Mode**: `AlwaysRunning`
   - **Idle Time-out (minutes)**: `0`
   - **.NET CLR Version**: `No Managed Code`
2. **Sitio Web**:
   - **Preload Enabled**: `True`

---

## Estructura del Proyecto

- `Controllers/CheckoutController.cs`: Endpoints de Mercado Pago.
- `Services/MercadoPagoService.cs`: Lógica del SDK de Mercado Pago.
- `Program.cs`: Configuración de OpenAPI, Scalar y Middleware.
- `deploy_ftp.sh`: Script de automatización de despliegue.
