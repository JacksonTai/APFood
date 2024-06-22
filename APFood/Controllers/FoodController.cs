using APFood.Data;
using APFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using APFood.Areas.Identity.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace APFood.Controllers
{
    public class FoodController : Controller
    {
        private readonly APFoodContext _context;
        private readonly UserManager<APFoodUser> _userManager;
        private readonly ILogger<FoodController> _logger;

        public FoodController(APFoodContext context, UserManager<APFoodUser> userManager, ILogger<FoodController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Food
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var foodVendor = _context.FoodVendors.FirstOrDefault(v => v.Id == user.Id);
            if (foodVendor == null)
            {
                return NotFound();
            }
            var foods = _context.Foods.Where(f => f.FoodVendorId == foodVendor.Id).ToList();
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
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/foodImg", fileName);

                    _logger.LogInformation("Saving file to {FilePath}", filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await food.ImageFile.CopyToAsync(stream);
                    }

                    food.ImageUrl = "/foodImg/" + fileName;
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
            _logger.LogInformation(food.Category + "food.Category");
            _logger.LogInformation(food.Description + "food.Description");
            _logger.LogInformation(food.FoodVendorId + "foodModel.FoodVendorId");
            _logger.LogInformation(food.ImageUrl + "food.ImageUrl");
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
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/foodImg", fileName);

                    _logger.LogInformation("Saving file to {FilePath}", filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await food.ImageFile.CopyToAsync(stream);
                    }

                    existingFood.ImageUrl = "/foodImg/" + fileName;
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
                    _context.Foods.Remove(food);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Food deleted successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting food from database");
                    ModelState.AddModelError(string.Empty, "Error deleting from database");
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
