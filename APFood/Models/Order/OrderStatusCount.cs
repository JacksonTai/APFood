using System.Text.Json.Serialization;

namespace APFood.Models.Order
{
    public class OrderStatusCount
    {
        [JsonPropertyName("Pending")]
        public int Pending { get; set; }

        [JsonPropertyName("Processing")]
        public int Processing { get; set; }

        [JsonPropertyName("Ready")]
        public int Ready { get; set; }

        [JsonPropertyName("Completed")]
        public int Completed { get; set; }

        [JsonPropertyName("Cancelled")]
        public int Cancelled { get; set; }
    }
}
