using MercadoPago.Client.Preference;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using MercadoPago.Resource.Payment;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MercadoPagoIntegration.Services
{
    public interface IMercadoPagoService
    {
        Task<Preference> CreatePreferenceAsync(string title, decimal price, int quantity, string accessToken, string currency = "CLP", string? email = null, string? successUrl = null, string? failureUrl = null, string? pendingUrl = null, string? backUrlBase = null, string? defaultReturnUrl = null);
        Task<Payment> GetPaymentAsync(long paymentId, string accessToken);
    }

    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly string _backUrlBase;

        public MercadoPagoService(IConfiguration configuration)
        {
            // Valor por defecto configurado globalmente
            _backUrlBase = ConfiguracionSegura.BackUrlBase;
        }

        public async Task<Preference> CreatePreferenceAsync(string title, decimal price, int quantity, string accessToken, string currency = "CLP", string? email = null, string? successUrl = null, string? failureUrl = null, string? pendingUrl = null, string? backUrlBase = null, string? defaultReturnUrl = null)
        {
            // Si no se recibe token en el request, usamos el harcodeado en ConfiguracionSegura.
            var token = string.IsNullOrEmpty(accessToken) ? ConfiguracionSegura.AccessToken : accessToken;
            MercadoPagoConfig.AccessToken = token; 

            // Determinar la URL base a usar
            var effectiveBackUrlBase = string.IsNullOrEmpty(backUrlBase) ? _backUrlBase : backUrlBase;

            // Helper para construir las BackUrls usando la URL del cliente directamente si existe, si no usar el de la API
            string BuildBackUrl(string route, string? directUrl)
            {
                if (!string.IsNullOrEmpty(directUrl))
                {
                    return directUrl;
                }
                if (!string.IsNullOrEmpty(defaultReturnUrl))
                {
                    return defaultReturnUrl;
                }
                return $"{effectiveBackUrlBase}/api/checkout/{route}";
            }

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = title,
                        Quantity = quantity,
                        CurrencyId = currency,
                        UnitPrice = price,
                    }
                },
                Payer = new PreferencePayerRequest
                {
                    Email = email // El email ahora viene por parámetro
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = BuildBackUrl("success", successUrl),
                    Failure = BuildBackUrl("failure", failureUrl),
                    Pending = BuildBackUrl("pending", pendingUrl),
                },
                AutoReturn = "all", // Cambiado de "approved" a "all" para que retorne siempre
                BinaryMode = true,
                StatementDescriptor = "Instituto Chileno Norteamericano Testingcenter",
            };

            try
            {
                var client = new PreferenceClient();
                var preference = await client.CreateAsync(request);
                return preference;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error al crear preferencia: {ex.Message}");
                throw;
            }
        }

        public async Task<Payment> GetPaymentAsync(long paymentId, string accessToken)
        {
            var token = string.IsNullOrEmpty(accessToken) ? ConfiguracionSegura.AccessToken : accessToken;
            MercadoPagoConfig.AccessToken = token;
            try
            {
                var client = new PaymentClient();
                var payment = await client.GetAsync(paymentId);
                return payment;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error al obtener pago {paymentId}: {ex.Message}");
                throw;
            }
        }
    }
}
