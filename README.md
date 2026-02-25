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
    "price": 99.99,
    "quantity": 1,
    "currency": "USD",
    "accessToken": "TU_ACCESS_TOKEN",
    "publicKey": "TU_PUBLIC_KEY"
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

Mercado Pago redirige automáticamente al usuario a estos endpoints una vez finalizado el flujo de pago. Todos leen los query params enviados por MP (`payment_id`, `status`, `external_reference`, `merchant_order_id`).

| Método | Ruta | Cuándo se invoca |
|--------|------|-----------------|
| `GET` | `/api/Checkout/success` | Pago aprobado |
| `GET` | `/api/Checkout/failure` | Pago rechazado o cancelado |
| `GET` | `/api/Checkout/pending` | Pago pendiente de confirmación |

**Respuesta de ejemplo (`/success`)**:
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

La URL base de retorno se configura en `appsettings.json`:
```json
{
  "MercadoPago": {
    "BackUrlBase": "https://apimp.norteamericano.cl"
  }
}
```

---

### 3. Webhook (Notificaciones IPN)
Recibe notificaciones automáticas de Mercado Pago cuando cambia el estado de un pago.

- **URL**: `POST /api/Checkout/webhook`
- **Query params**:

| Parámetro | Descripción |
|-----------|-------------|
| `topic` | Tipo de notificación (ej: `payment`) |
| `id` | ID del recurso notificado |
| `access_token` | Token de MP necesario para consultar el detalle del pago |

> **Nota**: El parámetro `access_token` debe configurarse en el panel de Mercado Pago al registrar la URL del webhook, o incluirse manualmente al registrar la preferencia.

---

## Despliegue (FTP a Windows Server)

Usa el script `deploy_ftp.sh` para desplegar en el servidor Windows:

1. **Compilación**: Genera los binarios para `win-x64`.
2. **app_offline.htm**: Detiene el sitio temporalmente para permitir la sobreescritura de archivos.
3. **Subida**: Transfiere vía FTP a `ftp://norteamericano.com/apimp`.
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
├── wwwroot/
│   └── index.html              # UI de prueba (Checkout Pro)
├── appsettings.json            # Configuración (BackUrlBase, logging)
├── Program.cs                  # Middleware, OpenAPI, Scalar
└── deploy_ftp.sh               # Script de despliegue automatizado
```

---

## Historial de Cambios

| Fecha | Cambio |
|-------|--------|
| 2026-02-25 | Agregados endpoints de retorno BackUrl (`/success`, `/failure`, `/pending`). Webhook activado para consultar estado real del pago. BackUrlBase movida a `appsettings.json`. Frontend actualizado para enviar credenciales. |
| Ene 2026 | Implementación inicial: create-preference, webhook, UI de prueba, despliegue FTP, Scalar UI. |
