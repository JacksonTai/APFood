using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.DeliveryTask;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;

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

        public async Task<DeliveryTask?> GetDeliveryTaskByIdAsync(int deliveryTaskId)
        {
            return await _context.DeliveryTasks
                .Include(o => o.Order)
                .FirstOrDefaultAsync(o => o.Id == deliveryTaskId);
        }

        public async Task<List<DeliveryTaskListViewModel>> GetDeliveryTasksByStatusAsync(DeliveryStatus status, string currentUserId)
        {
            IQueryable<DeliveryTask> query = _context.DeliveryTasks
                .Include(dt => dt.Order.Customer)
                .Include(dt => dt.Order.Items)
                .Include(dt => dt.RunnerDeliveryTasks)
                .Where(dt => dt.Order.CustomerId != currentUserId);

            if (status == DeliveryStatus.Pending)
            {
                query = query.Where(dt => (dt.Order.Status == OrderStatus.Processing || dt.Order.Status == OrderStatus.Ready) &&
                                           dt.Status == DeliveryStatus.Pending ||
                                          (dt.Status == DeliveryStatus.Cancelled &&
                                          !dt.RunnerDeliveryTasks.Any(rdt => rdt.RunnerId == currentUserId &&
                                                                             rdt.Status == DeliveryStatus.Cancelled)));
            }
            else
            {
                query = query.Where(dt => dt.RunnerDeliveryTasks.Any(rdt => rdt.RunnerId == currentUserId && rdt.Status == status));
            }
            return await query.Select(dt => new DeliveryTaskListViewModel
            {
                DeliveryTaskId = dt.Id,
                DeliveryLocation = dt.Location,
                DeliveryStatus = dt.Status,
                CustomerName = dt.Order.Customer.FullName,
                QueueNumber = dt.Order.QueueNumber,
                OrderId = dt.Order.Id,
                OrderStatus = dt.Order.Status,
                OrderTime = dt.Order.CreatedAt,
                IsAcceptableDeliveryTask = IsAcceptableDeliveryTask(dt, currentUserId),
                IsDeliverableDeliveryTask = IsDeliverableDeliveryTask(dt, currentUserId),
                IsCancellableDeliveryTask = IsCancellableDeliveryTask(dt, currentUserId)
            }).ToListAsync();
        }

        public async Task<Dictionary<DeliveryStatus, int>> GetDeliveryTaskCountsAsync(string currentUserId)
        {
            Dictionary<DeliveryStatus, int> deliveryTaskCounts = [];

            IQueryable<DeliveryTask> query = _context.DeliveryTasks
                .Where(dt => dt.Order.CustomerId != currentUserId);

            deliveryTaskCounts[DeliveryStatus.Pending] = await query
                .Where(dt =>(dt.Order.Status == OrderStatus.Processing || dt.Order.Status == OrderStatus.Ready) &&
                             dt.Status == DeliveryStatus.Pending ||
                            (dt.Status == DeliveryStatus.Cancelled &&
                            !dt.RunnerDeliveryTasks.Any(rdt => rdt.RunnerId == currentUserId && rdt.Status == DeliveryStatus.Cancelled)))
                .CountAsync();
 
            foreach (DeliveryStatus status in Enum.GetValues(typeof(DeliveryStatus)).Cast<DeliveryStatus>())
            {
                if (status != DeliveryStatus.Pending)
                {
                    deliveryTaskCounts[status] = await query
                        .Where(dt => dt.RunnerDeliveryTasks.Any(rdt => rdt.RunnerId == currentUserId && rdt.Status == status))
                        .CountAsync();
                }
                
            }
            return deliveryTaskCounts;
        }

        public async Task<DeliveryTaskDetailViewModel?> GetDeliveryTaskDetailAsync(int deliveryTaskId, string currentUserId)
        {
            DeliveryTaskDetailViewModel? deliveryTaskDetailView = null;
            IQueryable<DeliveryTask> query = _context.DeliveryTasks
                .Include(dt => dt.Order)
                .Include(dt => dt.Order.Items)
                .ThenInclude(oi => oi.Food)
                .Include(dt => dt.Order.Customer)
                .Include(dt => dt.RunnerDeliveryTasks)
                .Where(dt => dt.Id == deliveryTaskId)
                .Where(dt => dt.Order.CustomerId != currentUserId)
                .Where(dt => (dt.Order.Status == OrderStatus.Processing || dt.Order.Status == OrderStatus.Ready) && 
                              dt.Status == DeliveryStatus.Pending ||
                             (dt.Status == DeliveryStatus.Cancelled && 
                             !dt.RunnerDeliveryTasks.Any(rdt => rdt.RunnerId == currentUserId && rdt.Status == DeliveryStatus.Cancelled)) ||
                              dt.RunnerDeliveryTasks.Any(rdt => rdt.RunnerId == currentUserId));

            DeliveryTask? deliveryTask = await query.FirstOrDefaultAsync();
            if (deliveryTask != null)
            {
                deliveryTaskDetailView = new DeliveryTaskDetailViewModel
                {
                    DeliveryTaskId = deliveryTask.Id,
                    DeliveryLocation = deliveryTask.Location,
                    DeliveryStatus = deliveryTask.Status,
                    QueueNumber = deliveryTask.Order.QueueNumber,
                    CustomerName = deliveryTask.Order.Customer.FullName,
                    OrderId = deliveryTask.Order.Id,
                    OrderStatus = deliveryTask.Order.Status,
                    OrderTime = deliveryTask.Order.CreatedAt,
                    Items = deliveryTask.Order.Items,
                    IsAcceptableDeliveryTask = IsAcceptableDeliveryTask(deliveryTask, currentUserId),
                    IsDeliverableDeliveryTask = IsDeliverableDeliveryTask(deliveryTask, currentUserId),
                    IsCancellableDeliveryTask = IsCancellableDeliveryTask(deliveryTask, currentUserId)
                };
            }
            return deliveryTaskDetailView;
        }

        public async Task UpdateDeliveryStatusAsync(int deliveryTaskId, DeliveryStatus newStatus)
        {
            DeliveryTask deliveryTask = await GetDeliveryTaskByIdAsync(deliveryTaskId)
                ?? throw new Exception("Delivery task not found");
            deliveryTask.Status = newStatus;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRunnerDeliveryStatusAsync(int deliveryTaskId, DeliveryStatus newStatus)
        {
            DeliveryTask deliveryTask = await GetDeliveryTaskByIdAsync(deliveryTaskId)
                ?? throw new Exception("Delivery task not found");

            RunnerDeliveryTask runnerDeliveryTask = await _context.RunnerDeliveryTasks
                .Where(rdt => rdt.Status == DeliveryStatus.Accepted)
                .FirstOrDefaultAsync(rdt => rdt.DeliveryTaskId == deliveryTask.Id) ??
                    throw new Exception("Runner delivery task not found");

            runnerDeliveryTask.Status = newStatus;
            await _context.SaveChangesAsync();
        }

        public async Task<string> AcceptDeliveryTaskAsync(string userId, int deliveryTaskId)
        {
            bool hasAcceptedTask = await _context.RunnerDeliveryTasks
                .AnyAsync(rdt => rdt.RunnerId == userId && rdt.Status == DeliveryStatus.Accepted);

            if (hasAcceptedTask)
            {
                return "You already have an ongoing accepted delivery task. Please complete it before accepting a new one.";
            }
            else
            {
                _context.RunnerDeliveryTasks.Add(new RunnerDeliveryTask
                {
                    DeliveryTaskId = deliveryTaskId,
                    RunnerId = userId,
                    Status = DeliveryStatus.Accepted,
                });

                DeliveryTask deliveryTask = await _context.DeliveryTasks.FindAsync(deliveryTaskId)
                    ?? throw new Exception("Delivery Task not found");
                deliveryTask.Status = DeliveryStatus.Accepted;
                _context.DeliveryTasks.Update(deliveryTask);
                await _context.SaveChangesAsync();

                return "Delivery task accepted successfully.";
            }
        }

        public async Task DeliverDeliveryTaskAsync(int deliveryTaskId)
        {
            await UpdateRunnerDeliveryStatusAsync(deliveryTaskId, DeliveryStatus.Delivered);
            await UpdateDeliveryStatusAsync(deliveryTaskId, DeliveryStatus.Delivered);
        }

        public async Task CancelDeliveryTaskAsync(int deliveryTaskId)
        {
            await UpdateRunnerDeliveryStatusAsync(deliveryTaskId, DeliveryStatus.Cancelled);
            await UpdateDeliveryStatusAsync(deliveryTaskId, DeliveryStatus.Cancelled);
        }

        private static bool IsAcceptableDeliveryTask(DeliveryTask deliveryTask, string userId)
        {
            bool isValidOrderStatus = deliveryTask.Order.Status == OrderStatus.Processing ||
                                      deliveryTask.Order.Status == OrderStatus.Ready;
            bool isOrderedByCurrentUser = deliveryTask.Order.CustomerId == userId;
            bool isCancelledByCurrentUser = deliveryTask.RunnerDeliveryTasks.Any(rdt => rdt.RunnerId == userId &&
                                                                                        rdt.Status == DeliveryStatus.Cancelled);

            return (deliveryTask.Status == DeliveryStatus.Pending && isValidOrderStatus && !isOrderedByCurrentUser) ||
                   (deliveryTask.Status == DeliveryStatus.Cancelled && !isCancelledByCurrentUser);
        }

        private static bool IsDeliverableDeliveryTask(DeliveryTask deliveryTask, string userId)
        {
            return IsAcceptedByCurrentUser(deliveryTask, userId) && deliveryTask.Order.Status == OrderStatus.Ready;
        }

        private static bool IsCancellableDeliveryTask(DeliveryTask deliveryTask, string userId)
        {
            return IsAcceptedByCurrentUser(deliveryTask, userId);
        }

        private static bool IsAcceptedByCurrentUser(DeliveryTask deliveryTask, string userId)
        {
            return deliveryTask.Status == DeliveryStatus.Accepted && 
                   deliveryTask.RunnerDeliveryTasks.Any(rdt => rdt.RunnerId == userId && rdt.Status == DeliveryStatus.Accepted);
        }

    }
}
