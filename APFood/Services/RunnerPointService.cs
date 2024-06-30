using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Customer;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace APFood.Services
{
    public class RunnerPointService(APFoodContext context) : IRunnerPointService
    {

        private readonly APFoodContext _context = context;

        public async Task<decimal> GetTotalPoints(string userId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            return await _context.Customers
                .Where(c => c.Id == userId)
                .Select(c => c.Points)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalSpent(string userId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Order.CustomerId == userId)
                .Select(p => p.RunnerPointsUsed)
                .SumAsync();
        }

        public async Task<decimal> GetTotalEarned(string userId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            return await _context.DeliveryTasks
                .Where(dt => dt.RunnerDeliveryTasks.Any(dt => dt.RunnerId == userId && dt.Status == DeliveryStatus.Delivered))
                .Where(dt => dt.Status == DeliveryStatus.Delivered)
                .Where(dt => dt.Order.Status == OrderStatus.Completed)
                .CountAsync() * OrderConstants.RunnerPointsPerDelivery;
        }
    }
}
