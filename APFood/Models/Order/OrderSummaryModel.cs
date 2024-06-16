namespace APFood.Models.Order
{
    public class OrderSummaryModel
    {
        public decimal Subtotal { get; set; } = 0;
        public decimal DeliveryFee { get; set; } = 0;
        public decimal RunnerPointsRedeemed { get; set; } = 0;
        public decimal Total { get; set; } = 0;
    }
}
