using GYM_System.Data;
using GYM_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GYM_System.Controllers
{
    public class FoodItemsController : Controller
    {
        private readonly GymDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // To access wwwroot path

        public FoodItemsController(GymDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: FoodItems
        public async Task<IActionResult> Index()
        {
            return View(await _context.FoodItems.ToListAsync());
        }

        // GET: FoodItems/Details/5 (Optional, could just use Index for overview and Edit for details)
        // For now, we'll keep it simple and rely on Edit for details.

        // GET: FoodItems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FoodItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Unit,CaloriesPer100Units,ProteinPer100Units,CarbsPer100Units,FatPer100Units")] FoodItem foodItem, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Ensure the 'images/fooditems' directory exists
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "fooditems");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name to prevent conflicts
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    foodItem.ImagePath = "/images/fooditems/" + uniqueFileName; // Store relative path
                }

                _context.Add(foodItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Food item '{foodItem.Name}' added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(foodItem);
        }

        // GET: FoodItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }
            return View(foodItem);
        }

        // POST: FoodItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Unit,CaloriesPer100Units,ProteinPer100Units,CarbsPer100Units,FatPer100Units,ImagePath")] FoodItem foodItem, IFormFile? imageFile)
        {
            if (id != foodItem.Id)
            {
                return NotFound();
            }

            // Remove ImagePath from ModelState as it's handled manually
            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch existing food item to handle image path updates
                    var existingFoodItem = await _context.FoodItems.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
                    if (existingFoodItem == null)
                    {
                        return NotFound();
                    }

                    // Handle image file upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingFoodItem.ImagePath))
                        {
                            string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, existingFoodItem.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "fooditems");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        foodItem.ImagePath = "/images/fooditems/" + uniqueFileName;
                    }
                    else
                    {
                        // If no new image, retain the existing one
                        foodItem.ImagePath = existingFoodItem.ImagePath;
                    }

                    _context.Update(foodItem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Food item '{foodItem.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodItemExists(foodItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(foodItem);
        }

        // GET: FoodItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // POST: FoodItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem != null)
            {
                // Delete the associated image file if it exists
                if (!string.IsNullOrEmpty(foodItem.ImagePath))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, foodItem.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.FoodItems.Remove(foodItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Food item '{foodItem.Name}' deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FoodItemExists(int id)
        {
            return _context.FoodItems.Any(e => e.Id == id);
        }
    }
}