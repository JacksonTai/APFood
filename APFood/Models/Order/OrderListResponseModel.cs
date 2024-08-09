using System.Text.Json.Serialization;

namespace APFood.Models.Order
{
    public class OrderListResponseModel
    {
        [JsonPropertyName("$values")]
        public List<OrderListViewModel> orderListViewModels { get; set; }
    }
}
