using lily.Models;
using lily.Repository;
using Microsoft.AspNetCore.Mvc;

namespace lily.Controllers
{
    public class ToysController : Controller
    {
        private readonly ToysRepository _toysRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        //dependency injection
        public ToysController(ToysRepository toysRepository, IWebHostEnvironment webHostEnvironment)
        {
            _toysRepository = toysRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // Check if user is Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // Check if user is logged in
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserID") != null;
        }

        // GET: Toys
        public async Task<IActionResult> Index(string category)
        {
            IEnumerable<Toy> toys;
            
            if (!string.IsNullOrEmpty(category))
            {
                toys = await _toysRepository.GetByCategoryAsync(category);
                ViewBag.SelectedCategory = category;
            }
            else
            {
                toys = await _toysRepository.GetAllAsync();
                toys = toys.Where(t => t.IsActive == true);
            }
            
            // Get all distinct categories for filter
            var allToys = await _toysRepository.GetAllAsync();
            ViewBag.Categories = allToys
                .Where(t => t.IsActive == true && !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.IsAdmin = IsAdmin();
            return View(toys);
        }

        // GET: Toys/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toy = await _toysRepository.GetByIdAsync(id.Value);

            if (toy == null)
            {
                return NotFound();
            }

            ViewBag.IsLoggedIn = IsLoggedIn();
            ViewBag.IsAdmin = IsAdmin();
            return View(toy);
        }

        // GET: Toys/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }
            return View();
        }

        // POST: Toys/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Toy toy, IFormFile imageFile)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }

            // Remove Orders from ModelState validation since it's not in the form
            ModelState.Remove("Orders");
            
            if (ModelState.IsValid)
            {
                // Handle image upload to add imgage to toy
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "toys");
                    Directory.CreateDirectory(uploadsFolder); // Create if not exists

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    toy.ImageUrl = "/images/toys/" + uniqueFileName;
                }

                await _toysRepository.AddAsync(toy);
                TempData["SuccessMessage"] = "Toy created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(toy);
        }

        // GET: Toys/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var toy = await _toysRepository.GetByIdAsync(id.Value);
            if (toy == null)
            {
                return NotFound();
            }
            return View(toy);
        }

        // POST: Toys/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Toy toy, IFormFile imageFile)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }

            if (id != toy.ToyID)
            {
                return NotFound();
            }

            // Remove Orders from ModelState validation
            ModelState.Remove("Orders");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "toys");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        toy.ImageUrl = "/images/toys/" + uniqueFileName;
                    }

                    await _toysRepository.UpdateAsync(toy);
                    TempData["SuccessMessage"] = "Toy updated successfully!";
                }
                catch (Exception)
                {
                    var exists = await _toysRepository.GetByIdAsync(toy.ToyID);
                    if (exists == null)
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(toy);
        }

        // GET: Toys/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var toy = await _toysRepository.GetByIdAsync(id.Value);

            if (toy == null)
            {
                return NotFound();
            }

            return View(toy);
        }

        // POST: Toys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }

            var toy = await _toysRepository.GetByIdAsync(id);
            if (toy != null)
            {
                toy.IsActive = false; // Soft delete
                await _toysRepository.UpdateAsync(toy);
                TempData["SuccessMessage"] = "Toy deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
