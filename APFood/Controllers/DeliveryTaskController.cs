using APFood.Constants;
using APFood.Models.DeliveryTask;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = $"{UserRole.Customer},{UserRole.Admin}")]
    [Route("[controller]")]
    public class DeliveryTaskController(
        IDeliveryTaskService deliveryTaskService,
        IRunnerPointService runnerPointService,
        ILogger<OrderController> logger
        ) : Controller
    {
        private readonly IDeliveryTaskService _deliveryTaskService = deliveryTaskService;
        private readonly IRunnerPointService _runnerPointService = runnerPointService;
        private readonly ILogger<OrderController> _logger = logger;

        public async Task<IActionResult> Index(DeliveryStatus status = DeliveryStatus.Pending)
        {
            string userId = GetUserId();
            bool isAdmin = User.IsInRole(UserRole.Admin);
            try
            {
                List<DeliveryTaskListViewModel> deliveryTasks;
                Dictionary<DeliveryStatus, int> deliveryTaskCounts;
                if (isAdmin)
                {
                    deliveryTasks = await _deliveryTaskService.GetDeliveryTasksByStatusAdminAsync(status);
                    deliveryTaskCounts = await _deliveryTaskService.GetDeliveryTaskCountsAdminAsync();
                }
                else
                {
                    deliveryTasks = await _deliveryTaskService.GetDeliveryTasksByStatusAsync(status, userId);
                    deliveryTaskCounts = await _deliveryTaskService.GetDeliveryTaskCountsAsync(userId);
                }

                return View(new DeliveryTaskViewModel
                {
                    DeliveryTaskList = deliveryTasks,
                    DeliveryTaskCounts = deliveryTaskCounts,
                    CurrentStatus = status,
                    TotalPointsEarned = isAdmin ? 0 : await _runnerPointService.GetTotalEarned(userId),
                    IsAdmin = isAdmin
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the delivery tasks");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            bool isAdmin = User.IsInRole(UserRole.Admin);
            try
            {
                DeliveryTaskDetailViewModel? deliveryTaskDetail;
                if (isAdmin)
                {
                    deliveryTaskDetail = await _deliveryTaskService.GetDeliveryTaskDetailAdminAsync(id);
                }
                else
                {
                    deliveryTaskDetail = await _deliveryTaskService.GetDeliveryTaskDetailAsync(id, GetUserId());
                }

                if (deliveryTaskDetail != null)
                {
                    deliveryTaskDetail.IsAdmin = isAdmin;
                }
                return deliveryTaskDetail == null ? NotFound() : View(deliveryTaskDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the delivery task detail for task {DeliveryTaskId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }



        [HttpPost("AcceptDeliveryTask")]
        public async Task<IActionResult> AcceptDeliveryTask(int deliveryTaskId)
        {
            try
            {
                await _deliveryTaskService.AcceptDeliveryTaskAsync(GetUserId(), deliveryTaskId);
                return Redirect(Request.Headers.Referer.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while accepting the delivery task {DeliveryTaskId}", deliveryTaskId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost("DeliverDeliveryTask")]
        public async Task<IActionResult> DeliverDeliveryTask(int deliveryTaskId)
        {
            try
            {
                await _deliveryTaskService.DeliverDeliveryTaskAsync(deliveryTaskId);
                return Redirect(Request.Headers.Referer.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while delivering the delivery task {DeliveryTaskId}", deliveryTaskId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost("CancelDeliveryTask")]
        public async Task<IActionResult> CancelDeliveryTask(int deliveryTaskId)
        {
            try
            {
                await _deliveryTaskService.CancelDeliveryTaskAsync(deliveryTaskId);
                return Redirect(Request.Headers.Referer.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cancelling the delivery task {DeliveryTaskId}", deliveryTaskId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User not authenticated.");
        }
    }
}
