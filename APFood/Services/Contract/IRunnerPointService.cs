using APFood.Models.Customer;

namespace APFood.Services.Contract
{
    public interface IRunnerPointService
    {
        Task<decimal> GetTotalPoints(string userId);
        Task<decimal> GetTotalSpent(string userId);
        Task<decimal> GetTotalEarned(string userId);
    }
}
