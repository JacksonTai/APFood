namespace APFood.Data
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public required Order Order { get; set; }
        public decimal Subtotal { get; set; }
        public decimal RunnerPointsUsed { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
