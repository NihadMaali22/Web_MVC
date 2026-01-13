using lily.Models;
using lily.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace lily.Controllers
{
    public class HomeController : Controller
    {
        private readonly ToysRepository _toysRepository;
        private readonly OrdersRepository _ordersRepository;
        private readonly UsersRepository _usersRepository;

        public HomeController(ToysRepository toysRepository, OrdersRepository ordersRepository, UsersRepository usersRepository)
        {
            _toysRepository = toysRepository;
            _ordersRepository = ordersRepository;
            _usersRepository = usersRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured toys (first 6 active toys)
            var allToys = await _toysRepository.GetAllAsync();
            var featuredToys = allToys
                .Where(t => t.IsActive == true)
                .Take(6)
                .ToList();

            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserID") != null;
            ViewBag.IsAdmin = HttpContext.Session.GetString("Role") == "Admin";

            return View("Index", featuredToys);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            // In a real application, you would send an email or save to database
            TempData["SuccessMessage"] = "Thank you for contacting us! We'll get back to you soon.";
            return RedirectToAction("Contact");
        }

        public async Task<IActionResult> Dashboard()
        {
            // Check if user is admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }

            // Get statistics
            var allToys = await _toysRepository.GetAllAsync();
            var allOrders = await _ordersRepository.GetAllAsync();
            var allUsers = await _usersRepository.GetAllAsync();

            ViewBag.TotalToys = allToys.Count();
            ViewBag.TotalOrders = allOrders.Count();
            ViewBag.TotalUsers = allUsers.Count();
            ViewBag.PendingOrders = allOrders.Count(o => o.Status == "Pending");

            // Get recent orders
            var recentOrders = allOrders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            ViewBag.RecentOrders = recentOrders;

            return View();
        }
    }
}
