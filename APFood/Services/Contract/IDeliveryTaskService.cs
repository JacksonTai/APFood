using APFood.Constants;
using APFood.Data;
using APFood.Models.DeliveryTask;

namespace APFood.Services.Contract
{
    public interface IDeliveryTaskService
    {
        Task CreateDeliveryTask(Order order, string location);
        Task<DeliveryTask?> GetDeliveryTaskByIdAsync(int deliveryTaskId);
        Task<List<DeliveryTaskListViewModel>> GetDeliveryTasksByStatusAsync(DeliveryStatus status, string currentUserId);
        Task<Dictionary<DeliveryStatus, int>> GetDeliveryTaskCountsAsync(string currentUserId);
        Task<DeliveryTaskDetailViewModel?> GetDeliveryTaskDetailAsync(int deliveryTaskId, string currentUserId);
        Task UpdateDeliveryStatusAsync(int deliveryTaskId, DeliveryStatus newStatus);
        Task UpdateRunnerDeliveryStatusAsync(int deliveryTaskId, DeliveryStatus newStatus);
        Task<string> AcceptDeliveryTaskAsync(string userId, int deliveryTaskId);
        Task DeliverDeliveryTaskAsync(int deliveryTaskId);
        Task CancelDeliveryTaskAsync(int deliveryTaskId);
    }
}
