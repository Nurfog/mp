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
        Task<Preference> CreatePreferenceAsync(string title, decimal price, int quantity, string accessToken);
        Task<Payment> GetPaymentAsync(long paymentId, string accessToken);
    }

    public class MercadoPagoService : IMercadoPagoService
    {
        public MercadoPagoService()
        {
        }

        public async Task<Preference> CreatePreferenceAsync(string title, decimal price, int quantity, string accessToken)
        {
            // Nota: En un entorno de producción con múltiples usuarios concurrentes usando diferentes tokens,
            // MercadoPagoConfig.AccessToken (estático) no es thread-safe. 
            // Preferiblemente usar RequestOptions si la versión del SDK lo soporta en CreateAsync.
            MercadoPagoConfig.AccessToken = accessToken;

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = title,
                        Quantity = quantity,
                        CurrencyId = "CLP", // Ajustado para Chile
                        UnitPrice = price,

                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "https://apimp.norteamericano.cl/success",
                    Failure = "https://apimp.norteamericano.cl/failure",
                    Pending = "https://apimp.norteamericano.cl/pending",
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
            MercadoPagoConfig.AccessToken = accessToken;
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

