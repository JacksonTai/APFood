using APFood.Constants;
using APFood.Models.DeliveryTask;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    [Route("[controller]")]
    public class DeliveryTaskController(
        IDeliveryTaskService deliveryTaskService,
        ILogger<OrderController> logger
        ) : Controller
    {
        private readonly IDeliveryTaskService _deliveryTaskService = deliveryTaskService;
        private readonly ILogger<OrderController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Index(DeliveryStatus status = DeliveryStatus.Pending)
        {
            string userId = GetUserId();
            try
            {
                return View(new DeliveryTaskViewModel
                {
                    DeliveryTaskList = await _deliveryTaskService.GetDeliveryTasksByStatusAsync(status, userId),
                    DeliveryTaskCounts = await _deliveryTaskService.GetDeliveryTaskCountsAsync(userId),
                    CurrentStatus = status
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
            try
            {
                DeliveryTaskDetailViewModel? deliveryTaskDetail =
                    await _deliveryTaskService.GetDeliveryTaskDetailAsync(id, GetUserId());
                return deliveryTaskDetail == null ? NotFound() : View(deliveryTaskDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the detail for delivery task {DeliveryTaskId}", id);
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
