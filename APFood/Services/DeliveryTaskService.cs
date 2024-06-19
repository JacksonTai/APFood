using APFood.Constants;
using APFood.Data;
using APFood.Services.Contract;

namespace APFood.Services
{
    public class DeliveryTaskService(APFoodContext context) : IDeliveryTaskService
    {
        private readonly APFoodContext _context = context;

        public async Task CreateDeliveryTask(Order order, string location)
        {
            _context.DeliveryTasks.Add(new DeliveryTask
            {
                Status = DeliveryStatus.Pending,
                OrderId = order.Id,
                Order = order,
                Location = location
            });
            await _context.SaveChangesAsync(); 
        }


    }
}
