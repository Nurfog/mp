using MercadoPagoIntegration.Models;
using MercadoPagoIntegration.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MercadoPagoIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IMercadoPagoService _mercadoPagoService;

        public CheckoutController(IMercadoPagoService mercadoPagoService)
        {
            _mercadoPagoService = mercadoPagoService;
        }

        [HttpPost("create-preference")]
        public async Task<IActionResult> CreatePreference([FromBody] CheckoutRequest request)
        {
            try
            {
                // Determinar BackUrlBase dinámica
                var backUrlBase = request.BackUrlBase;
                if (string.IsNullOrEmpty(backUrlBase))
                {
                    backUrlBase = $"{Request.Scheme}://{Request.Host}";
                }

                var preference = await _mercadoPagoService.CreatePreferenceAsync(
                    request.Title, 
                    request.Price, 
                    request.Quantity, 
                    request.AccessToken, 
                    request.Currency,
                    request.Email,
                    request.SuccessUrl,
                    request.FailureUrl,
                    request.PendingUrl,
                    backUrlBase,
                    request.DefaultReturnUrl
                );
                return Ok(new {
                    id = preference.Id,
                    init_point = preference.InitPoint,
                    publicKey = string.IsNullOrEmpty(request.PublicKey) ? ConfiguracionSegura.PublicKey : request.PublicKey
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ─── BackUrl: retorno tras pago aprobado ───────────────────────────
        // ─── BackUrl: retorno tras pago aprobado ───────────────────────────
        [HttpGet("success")]
        public IActionResult PaymentSuccess(
            [FromQuery(Name = "payment_id")] string? paymentId,
            [FromQuery(Name = "status")] string? status,
            [FromQuery(Name = "external_reference")] string? externalReference,
            [FromQuery(Name = "merchant_order_id")] string? merchantOrderId,
            [FromQuery] string? redirectUrl)
        {
            Console.WriteLine($"[BackUrl] SUCCESS - payment_id={paymentId}, status={status}, ref={externalReference}");

            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var finalUrl = BuildRedirectUrl(redirectUrl, paymentId, status, externalReference, merchantOrderId);
                return Redirect(finalUrl);
            }

            return Ok(new
            {
                result = "success",
                payment_id = paymentId,
                status = status,
                external_reference = externalReference,
                merchant_order_id = merchantOrderId,
                message = "Pago aprobado correctamente."
            });
        }

        // ─── BackUrl: retorno tras pago rechazado ──────────────────────────
        [HttpGet("failure")]
        public IActionResult PaymentFailure(
            [FromQuery(Name = "payment_id")] string? paymentId,
            [FromQuery(Name = "status")] string? status,
            [FromQuery(Name = "external_reference")] string? externalReference,
            [FromQuery] string? redirectUrl)
        {
            Console.WriteLine($"[BackUrl] FAILURE - payment_id={paymentId}, status={status}, ref={externalReference}");

            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var finalUrl = BuildRedirectUrl(redirectUrl, paymentId, status, externalReference);
                return Redirect(finalUrl);
            }

            return Ok(new
            {
                result = "failure",
                payment_id = paymentId,
                status = status,
                external_reference = externalReference,
                message = "El pago fue rechazado o cancelado."
            });
        }

        // ─── BackUrl: retorno tras pago pendiente ──────────────────────────
        [HttpGet("pending")]
        public IActionResult PaymentPending(
            [FromQuery(Name = "payment_id")] string? paymentId,
            [FromQuery(Name = "status")] string? status,
            [FromQuery(Name = "external_reference")] string? externalReference,
            [FromQuery] string? redirectUrl)
        {
            Console.WriteLine($"[BackUrl] PENDING - payment_id={paymentId}, status={status}, ref={externalReference}");

            if (!string.IsNullOrEmpty(redirectUrl))
            {
                var finalUrl = BuildRedirectUrl(redirectUrl, paymentId, status, externalReference);
                return Redirect(finalUrl);
            }

            return Ok(new
            {
                result = "pending",
                payment_id = paymentId,
                status = status,
                external_reference = externalReference,
                message = "El pago está pendiente de confirmación."
            });
        }

        private string BuildRedirectUrl(string baseUrl, string? paymentId, string? status, string? externalReference, string? merchantOrderId = null)
        {
            var uriBuilder = new System.UriBuilder(baseUrl);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

            if (!string.IsNullOrEmpty(paymentId)) query["payment_id"] = paymentId;
            if (!string.IsNullOrEmpty(status)) query["status"] = status;
            if (!string.IsNullOrEmpty(externalReference)) query["external_reference"] = externalReference;
            if (!string.IsNullOrEmpty(merchantOrderId)) query["merchant_order_id"] = merchantOrderId;

            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        // ─── Webhook de notificaciones IPN ────────────────────────────────
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook(
            [FromQuery] string? topic,
            [FromQuery] string? id,
            [FromQuery] string? access_token)
        {
            Console.WriteLine($"[Webhook] recibido: Topic={topic}, ID={id}");

            if (topic == "payment" && long.TryParse(id, out long paymentId))
            {
                try
                {
                    // Usamos el token recibido o el de configuración si no viene ninguno
                    var token = string.IsNullOrEmpty(access_token) ? ConfiguracionSegura.AccessToken : access_token;
                    
                    if (string.IsNullOrEmpty(token))
                    {
                        Console.WriteLine("[Webhook] ADVERTENCIA: No hay access_token disponible para consultar el pago.");
                        return Ok();
                    }

                    var payment = await _mercadoPagoService.GetPaymentAsync(paymentId, token);
                    Console.WriteLine($"[Webhook] Pago {paymentId}: status={payment.Status}, amount={payment.TransactionAmount}");
                    // TODO: Aquí puedes actualizar tu DB, activar el servicio del usuario, etc.
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"[Webhook] Error procesando pago {paymentId}: {ex.Message}");
                }
            }

            return Ok();
        }
    }
}
