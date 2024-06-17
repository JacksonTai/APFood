namespace APFood.Data
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int FoodId { get; set; }
        public required Food Food { get; set; }
        public int Quantity { get; set; }
    }
}
