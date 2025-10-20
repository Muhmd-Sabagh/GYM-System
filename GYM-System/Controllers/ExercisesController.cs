using GYM_System.Data;
using GYM_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GYM_System.Controllers
{
    public class ExercisesController : Controller
    {
        private readonly GymDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment; // To access wwwroot path

        public ExercisesController(GymDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Exercises
        public async Task<IActionResult> Index()
        {
            return View(await _context.Exercises.ToListAsync());
        }

        // GET: Exercises/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Exercises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,YouTubeLink")] Exercise exercise, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Ensure the 'images/exercises' directory exists
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "exercises");
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
                    exercise.ImagePath = "/images/exercises/" + uniqueFileName; // Store relative path
                }

                _context.Add(exercise);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Exercise '{exercise.Name}' added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(exercise);
        }

        // GET: Exercises/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }
            return View(exercise);
        }

        // POST: Exercises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,YouTubeLink,ImagePath")] Exercise exercise, IFormFile? imageFile)
        {
            if (id != exercise.Id)
            {
                return NotFound();
            }

            // Remove ImagePath from ModelState as it's handled manually
            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch existing exercise to handle image path updates
                    var existingExercise = await _context.Exercises.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                    if (existingExercise == null)
                    {
                        return NotFound();
                    }

                    // Handle image file upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existingExercise.ImagePath))
                        {
                            string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, existingExercise.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "exercises");
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
                        exercise.ImagePath = "/images/exercises/" + uniqueFileName;
                    }
                    else
                    {
                        // If no new image, retain the existing one
                        exercise.ImagePath = existingExercise.ImagePath;
                    }

                    _context.Update(exercise);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Exercise '{exercise.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExerciseExists(exercise.Id))
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
            return View(exercise);
        }

        // GET: Exercises/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exercise = await _context.Exercises
                .FirstOrDefaultAsync(m => m.Id == id);
            if (exercise == null)
            {
                return NotFound();
            }

            return View(exercise);
        }

        // POST: Exercises/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise != null)
            {
                // Delete the associated image file if it exists
                if (!string.IsNullOrEmpty(exercise.ImagePath))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, exercise.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.Exercises.Remove(exercise);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Exercise '{exercise.Name}' deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ExerciseExists(int id)
        {
            return _context.Exercises.Any(e => e.Id == id);
        }
    }
}