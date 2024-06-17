using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Controllers;
using APFood.Data;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace APFood.Services
{
    public class DeliveryTaskService(APFoodContext context) : IDeliveryTaskService
    {
        private readonly APFoodContext _context = context;

        public async Task CreateDeliveryTask(Order order, string location)
        {
            _context.DeliveryTasks.Add(new Data.DeliveryTask
            {
                Status = DeliveryStatus.Pending,
                OrderId = order.Id,
                Order = order,
                Location = location,
            });
            await _context.SaveChangesAsync(); 
        }


    }
}
