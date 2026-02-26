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
        Task<Preference> CreatePreferenceAsync(string title, decimal price, int quantity, string accessToken, string currency = "CLP");
        Task<Payment> GetPaymentAsync(long paymentId, string accessToken);
    }

    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly string _backUrlBase;

        public MercadoPagoService(IConfiguration configuration)
        {
            // Ahora usamos los valores hardcodeados que se compilan dentro del DLL para máxima seguridad en el servidor.
            _backUrlBase = ConfiguracionSegura.BackUrlBase;
        }

        public async Task<Preference> CreatePreferenceAsync(string title, decimal price, int quantity, string accessToken, string currency = "CLP")
        {
            // Si no se recibe token en el request, usamos el harcodeado en ConfiguracionSegura.
            var token = string.IsNullOrEmpty(accessToken) ? ConfiguracionSegura.AccessToken : accessToken;
            MercadoPagoConfig.AccessToken = token; 

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
                    Email = "test_user_123@testuser.com" // Email de prueba genérico para evitar errores de validación en Sandbox
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{_backUrlBase}/api/checkout/success",
                    Failure = $"{_backUrlBase}/api/checkout/failure",
                    Pending = $"{_backUrlBase}/api/checkout/pending",
                },
                AutoReturn = "approved",
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
