using APFood.Data;

namespace APFood.Services.Contract
{
    public interface IDeliveryTaskService
    {
         Task CreateDeliveryTask(Order order, string location);
    }
}
