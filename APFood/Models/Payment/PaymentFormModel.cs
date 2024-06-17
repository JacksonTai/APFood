using APFood.Constants.Payment;
using System.ComponentModel.DataAnnotations;

namespace APFood.Models.Payment
{
    public class PaymentFormModel
    {
        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; } 

        [Required(ErrorMessage = "Card Holder is required")]
        public string? CardHolder { get; set; }

        [Required(ErrorMessage = "Card number is required")]
        [RegularExpression(@"\d{16}", ErrorMessage = "Card number must be 16 digits")]
        public string? CardNumber { get; set; }

        [Required(ErrorMessage = "Expiration month is required")]
        public string? ExpirationMonth { get; set; }

        [Required(ErrorMessage = "Expiration year is required")]
        public string? ExpirationYear { get; set; }

        [Required(ErrorMessage = "CVV is required")]
        [RegularExpression(@"\d{3}", ErrorMessage = "CVV must be 3 digits")]
        public string? CVV { get; set; }
    }
}
