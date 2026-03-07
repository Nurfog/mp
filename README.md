# Mercado Pago Checkout Pro Integration (.NET 10)

Backend desarrollado en .NET 10 para integrarse con **Checkout Pro** de Mercado Pago. Permite generar preferencias de pago, recibir notificaciones vía webhooks, y manejar las URLs de retorno tras el pago.

## Requisitos Previos

- **.NET 10 SDK**: Instalado en el sistema.
- **Credenciales de Mercado Pago**: `Access Token` y `Public Key` de tu cuenta (usa modo Sandbox para pruebas).

## Entorno de Producción

La API está desplegada y accesible en:
👉 **[https://apimp.norteamericano.cl/](https://apimp.norteamericano.cl/)**

### Documentación Interactiva (Scalar UI)
👉 **[https://apimp.norteamericano.cl/scalar](https://apimp.norteamericano.cl/scalar)**

---

## Endpoints

### 1. Crear Preferencia de Pago
Genera un `init_point` para redirigir al cliente al entorno de pago de Mercado Pago.

- **URL**: `POST /api/Checkout/create-preference`
- **Cuerpo (JSON)**:
  ```json
  {
    "title": "Nombre del Producto",
    "price": 15000,
    "quantity": 1,
    "currency": "CLP",
    "email": "comprador@correo.com",  // Opcional: auto-completa el checkout y permite pre-asignar usuario
    "successUrl": "https://tu-sitio.com/exito", // Opcional: Redirige aquí tras pago aprobado
    "failureUrl": "https://tu-sitio.com/error", // Opcional: Redirige aquí tras pago fallido
    "pendingUrl": "https://tu-sitio.com/espera", // Opcional: Redirige aquí tras pago pendiente
    "defaultReturnUrl": "https://tu-sitio.com", // Opcional: URL por defecto para el botón "Volver" o fallbacks
    "backUrlBase": "https://miproxy.com",     // Opcional: Fuerza la URL base para Mercado Pago
    "accessToken": "TU_ACCESS_TOKEN", // Opcional (fallback al DLL)
    "publicKey": "TU_PUBLIC_KEY"      // Opcional (fallback al DLL)
  }
  ```

  > Monedas soportadas: `USD`, `ARS`, `BRL`, `CLP`, `COP`, `MXN`, `PEN`, `UYU`  
  > Default: `USD`
- **Respuesta Exitosa (200 OK)**:
  ```json
  {
    "id": "PREF_ID",
    "init_point": "https://www.mercadopago.cl/checkout/v1/redirect?pref_id=...",
    "publicKey": "TU_PUBLIC_KEY"
  }
  ```

---

### 2. URLs de Retorno (BackUrls)

Las URLs proporcionadas en la creación de preferencia (`successUrl`, `failureUrl`, `pendingUrl`) se configuran directamente en Mercado Pago. Esto significa que **Mercado Pago redirigirá al usuario directamente a la URL de tu aplicación** sin pasar por esta API.

**Comportamiento de Redirección Directa:**
Si enviaste las URLs de retorno al crear la preferencia, el flujo será:
1. El usuario finaliza el pago.
2. Mercado Pago redirige al usuario **directamente** a tu URL indicada (o a `defaultReturnUrl` si alguna falto o el usuario presionó "Volver").
3. Al hacer la redirección, Mercado Pago adjunta automáticamente en tu URL los parámetros del pago (`payment_id`, `status`, `external_reference`, `merchant_order_id`, etc.). Tu aplicación (ej. frontend) debe procesar estos query params de forma nativa.

**Ejemplo de flujo con `successUrl`:**
1. Solicitas preferencia con `"successUrl": "https://miapp.com/pago-ok"`.
2. Mercado Pago redirige al usuario a: `https://miapp.com/pago-ok?payment_id=123&status=approved&external_reference=null&merchant_order_id=987`...

> **Nota:** Con esta modalidad, la API ya no actúa como intermediario para la redirección.

---

**Comportamiento por defecto (Fallback a la API):**
Si **no** envías URLs opcionales, Mercado Pago redirigirá de manera predeterminada a los endpoints incluidos en esta API (`/api/Checkout/success`, `/failure`, `/pending`). Estos endpoints leerán los query params de MP y responderán con un JSON estándar:

```json
{
  "result": "success",
  "payment_id": "123456789",
  "status": "approved",
  "external_reference": null,
  "merchant_order_id": "987654321",
  "message": "Pago aprobado correctamente."
}
```

**Detección Dinámica de URL Base:**
La API detecta automáticamente el esquema (`http`/`https`) y el host de la solicitud actual para construir las URLs de retorno. Esto significa que **no necesitas configurar `BackUrlBase`** si tu frontend está en el mismo dominio o si usas el dominio estándar.

Si necesitas forzar una URL base distinta (ej: detrás de un túnel o proxy), puedes enviarla en el parámetro `backUrlBase` al crear la preferencia.

---

### 3. Webhook (Notificaciones IPN)
Recibe notificaciones automáticas de Mercado Pago cuando cambia el estado de un pago.

- **URL**: `POST /api/Checkout/webhook`
- **Query params**:

| Parámetro | Descripción |
|-----------|-------------|
| `topic` | Tipo de notificación (ej: `payment`) |
| `id` | ID del recurso notificado |
| `access_token` | (Opcional) Token de MP para consultar el detalle (fallback al DLL) |

> **Nota**: El parámetro `access_token` debe configurarse en el panel de Mercado Pago al registrar la URL del webhook, o incluirse manualmente al registrar la preferencia.

---

## Despliegue (FTP a Windows Server)

Usa el script `deploy_ftp.sh` para desplegar en el servidor Windows:

1. **Compilación**: Genera los binarios para `win-x64`.
2. **app_offline.htm**: Detiene el sitio temporalmente para permitir la sobreescritura de archivos.
3. **Subida**: Transfiere vía FTP a `ftp://example.com/apimp`.
4. **Reactivación**: Elimina `app_offline.htm` para volver a poner el sitio en línea.

```bash
./deploy_ftp.sh
```

---

## Configuración de IIS (Auto-inicio)

Para que la API esté siempre activa:

1. **Application Pool**:
   - **Start Mode**: `AlwaysRunning`
   - **Idle Time-out (minutes)**: `0`
   - **.NET CLR Version**: `No Managed Code`
2. **Sitio Web**:
   - **Preload Enabled**: `True`

---

## Estructura del Proyecto

```
MercadoPagoIntegration/
├── Controllers/
│   └── CheckoutController.cs   # Endpoints: create-preference, success, failure, pending, webhook
├── Services/
│   └── MercadoPagoService.cs   # Lógica del SDK de Mercado Pago
├── Models/
│   └── CheckoutRequest.cs      # Modelo de la solicitud de pago
├── appsettings.json            # Configuración (BackUrlBase, logging)
├── Program.cs                  # Middleware, OpenAPI, Scalar
└── deploy_ftp.sh               # Script de despliegue automatizado
```

---

## Seguridad en Producción (Configuración Segura)

Para proteger las credenciales y URLs, hemos implementado un sistema de **Configuración Segura vía Compilación**.

### 1. Archivo de Configuración Local (`ConfiguracionSegura.cs`)
Tus claves y URLs sensibles ahora se almacenan en el archivo:
`MercadoPagoIntegration/Services/ConfiguracionSegura.cs`

- **Este archivo está en `.gitignore`**: Nunca se subirá a GitHub.
- **Seguridad**: Los valores se compilan directamente dentro del binario `MercadoPagoIntegration.dll`.
- **Invisibilidad**: Al ser parte del DLL, no hay archivos de texto plano (`.json` o `.env`) en el servidor que un usuario malintencionado pueda leer fácilmente.

### 2. Cómo actualizar claves
Si necesitas cambiar un Token o una URL:
1. Edita el archivo `Services/ConfiguracionSegura.cs` en tu entorno local.
2. Ejecuta `./deploy_ftp.sh` para recompilar y subir el nuevo DLL al servidor.

---

## Guía de Pruebas (Modo Sandbox)

Para probar correctamente la integración sin que Mercado Pago fuerce el Login con tu cuenta real:
1. Usa siempre **Ventana de Incógnito** (sin sesiones previas activas).
2. Usa un **correo inventado** en el campo de email (si envías un correo registrado, pedirá contraseña).
3. Asegúrate de que las credenciales enviadas comiencen por **`TEST-`**.
4. Usa una **[Tarjeta de Prueba Oficial](https://www.mercadopago.cl/developers/es/docs/checkout-pro/additional-content/test-cards)** de Mercado Pago (ej: Visa aprobada `4010 ... 0001`).

---

## Historial de Cambios
| Fecha | Descripción |
|-------|--------|
| Marzo 2026 | **Redirección Directa y DefaultReturnUrl**: Mercado Pago ahora redirige directamente al cliente con sus parámetros query. Agregado `defaultReturnUrl` para controlar el botón "Volver a la tienda". La interfaz de prueba (index.html) fue removida. |
| Feb 2026 | **Guest Checkout**: Soporte para pago sin cuenta vía `init_point` nativo. Parámetro `email` opcional para el pagador. BinaryMode activado. |
| Feb 2026 | Seguridad: Migración a configuración compilada. Eliminación de dependencia `dotenv.net`. |
| 2026-02-25 | Agregados endpoints de retorno BackUrl (`/success`, `/failure`, `/pending`). Webhook activado para consultar estado real del pago. BackUrlBase movida a `appsettings.json`. Frontend actualizado para enviar credenciales. |
| Ene 2026 | Implementación inicial: create-preference, webhook, UI de prueba, despliegue FTP, Scalar UI. |

