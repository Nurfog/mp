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
                var preference = await _mercadoPagoService.CreatePreferenceAsync(request.Title, request.Price, request.Quantity, request.AccessToken);
                return Ok(new { 
                    id = preference.Id, 
                    init_point = preference.InitPoint,
                    publicKey = request.PublicKey
                });

            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromQuery] string topic, [FromQuery] string id)
        {
            Console.WriteLine($"Webhook recibido: Topic={topic}, ID={id}");

            if (topic == "payment" && long.TryParse(id, out long paymentId))
            {
                try
                {
                    // NOTA: Para obtener el pago necesitamos un Access Token.
                    // En una implementación real, buscarías el token asociado al pago en tu DB
                    // o usarías un token maestro si aplica.
                    // Por ahora, lo dejaremos como un comentario explicativo.
                    // var payment = await _mercadoPagoService.GetPaymentAsync(paymentId, "TU_ACCESS_TOKEN");
                    // Console.WriteLine($"Estado del pago {paymentId}: {payment.Status}");
                    
                    Console.WriteLine($"Procesando notificación de pago para ID: {paymentId}");
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error procesando webhook: {ex.Message}");
                }
            }
            
            return Ok();
        }
    }
}
