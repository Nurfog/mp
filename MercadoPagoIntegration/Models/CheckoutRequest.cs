using System.Collections.Generic;

namespace MercadoPagoIntegration.Models
{
    public class CheckoutRequest
    {
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public string AccessToken { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string Currency { get; set; } = "CLP";
        public string? Email { get; set; }
        public string? SuccessUrl { get; set; }
        public string? FailureUrl { get; set; }
        public string? PendingUrl { get; set; }
        public string? BackUrlBase { get; set; }
        public string? DefaultReturnUrl { get; set; }
    }
}
