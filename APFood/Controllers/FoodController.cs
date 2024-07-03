using APFood.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using APFood.Areas.Identity.Data;
using APFood.Constants.Food;
using Microsoft.EntityFrameworkCore;
using APFood.Constants.Order;
using APFood.Constants;
using Microsoft.AspNetCore.Authorization;
using APFood.Services;

namespace APFood.Controllers
{
    [Authorize(Roles = $"{UserRole.FoodVendor}")]
    public class FoodController : Controller
    {
        private readonly APFoodContext _context;
        private readonly UserManager<APFoodUser> _userManager;
        private readonly ILogger<FoodController> _logger;
        private readonly S3Service _s3Service;

        public FoodController(APFoodContext context, UserManager<APFoodUser> userManager, ILogger<FoodController> logger, S3Service s3Service)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _s3Service = s3Service;
        }

        // GET: Food
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var foodVendor = await _context.FoodVendors.FirstOrDefaultAsync(v => v.Id == user.Id);
            if (foodVendor == null)
            {
                return NotFound();
            }

            var foods = await _context.Foods
                .Where(f => f.FoodVendorId == foodVendor.Id && f.Status != FoodStatus.Deleted)
                .ToListAsync();

            foreach (var food in foods)
            {
                if (!string.IsNullOrEmpty(food.ImageUrl))
                {
                    var fileName = Path.GetFileName(food.ImageUrl);
                    food.ImageUrl = _s3Service.GeneratePreSignedURL(fileName, TimeSpan.FromMinutes(30));
                }
            }

            return View(foods);
        }

        // GET: Food/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Food/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Food food)
        {
            _logger.LogInformation("Create method called");

            var user = await _userManager.GetUserAsync(User);
            var foodVendor = _context.FoodVendors.FirstOrDefault(v => v.Id == user.Id);

            if (foodVendor == null)
            {
                _logger.LogError("Food vendor not found for user {UserId}", user.Id);
                return NotFound();
            }

            if (food.ImageFile != null)
            {
                try
                {
                    var fileName = Path.GetFileName(food.ImageFile.FileName);
                    using (var stream = food.ImageFile.OpenReadStream())
                    {
                        food.ImageUrl = await _s3Service.UploadFileAsync(stream, fileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file");
                    ModelState.AddModelError(string.Empty, "Error uploading file");
                    return View(food);
                }
            }

            food.FoodVendorId = foodVendor.Id;
            food.FoodVendor = foodVendor;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(food);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Food created successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving food to database");
                    ModelState.AddModelError(string.Empty, "Error saving to database");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid");
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                    }
                }
            }

            return View(food);
        }

        // GET: Food/Edit/id
        public async Task<IActionResult> Edit(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(food.ImageUrl))
            {
                var fileName = Path.GetFileName(food.ImageUrl);
                food.ImageUrl = _s3Service.GeneratePreSignedURL(fileName, TimeSpan.FromMinutes(30));
            }

            return View(food);
        }

        // POST: Food/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Food food)
        {
            if (id != food.Id)
            {
                return NotFound();
            }

            var existingFood = await _context.Foods.FindAsync(id);
            if (existingFood == null)
            {
                return NotFound();
            }

            if (food.ImageFile != null)
            {
                try
                {
                    var fileName = Path.GetFileName(food.ImageFile.FileName);
                    using (var stream = food.ImageFile.OpenReadStream())
                    {
                        existingFood.ImageUrl = await _s3Service.UploadFileAsync(stream, fileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file");
                    ModelState.AddModelError(string.Empty, "Error uploading file");
                    return View(food);
                }
            }

            if (ModelState.IsValid)
            {
                existingFood.Name = food.Name;
                existingFood.Description = food.Description;
                existingFood.Price = food.Price;
                existingFood.Category = food.Category;
                existingFood.Status = food.Status;

                try
                {
                    _context.Update(existingFood);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Food updated successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving food to database");
                    ModelState.AddModelError(string.Empty, "Error saving to database");
                }
            }

            return View(food);
        }

        // GET: Food/Delete/id
        public async Task<IActionResult> Delete(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(food.ImageUrl))
            {
                var fileName = Path.GetFileName(food.ImageUrl);
                food.ImageUrl = _s3Service.GeneratePreSignedURL(fileName, TimeSpan.FromMinutes(30));
            }

            return View(food);
        }

        // POST: Food/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food != null)
            {
                try
                {
                    var hasActiveOrders = await _context.OrderItems
                        .Include(oi => oi.Order)
                        .AnyAsync(oi => oi.FoodId == id && (oi.Order.Status == OrderStatus.Processing || oi.Order.Status == OrderStatus.Ready));

                    if (hasActiveOrders)
                    {
                        ModelState.AddModelError(string.Empty, "Cannot delete food item as it is part of active orders.");
                        _logger.LogInformation("Cannot delete food item as it is part of active orders.");
                        return View(food);
                    }

                    // Remove cart items for this food
                    var cartItems = await _context.CartItems.Where(ci => ci.FoodId == id).ToListAsync();
                    if (cartItems.Any())
                    {
                        _context.CartItems.RemoveRange(cartItems);
                        _logger.LogInformation($"Removed {cartItems.Count} cart items containing food ID {id}");
                    }

                    food.Status = FoodStatus.Deleted; // Mark food as deleted
                    _context.Update(food);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Food marked as deleted successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error marking food as deleted");
                    ModelState.AddModelError(string.Empty, "Error updating status in database");
                }
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
