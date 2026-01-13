using lily.Models;
using lily.Repository;
using Microsoft.AspNetCore.Mvc;

namespace lily.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrdersRepository _ordersRepository;
        private readonly ToysRepository _toysRepository;

        public OrdersController(OrdersRepository ordersRepository, ToysRepository toysRepository)
        {
            _ordersRepository = ordersRepository;
            _toysRepository = toysRepository;
        }

        // Check if user is logged in
        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserID");
        }

        // Check if user is Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: Orders - User sees their orders, Admin sees all
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view orders!";
                return RedirectToAction("Login", "Account");
            }

            IEnumerable<Order> orders;

            // If not admin, show only user's orders
            if (!IsAdmin())
            {
                orders = await _ordersRepository.GetByUserIdAsync(userId.Value);
            }
            else
            {
                orders = await _ordersRepository.GetAllAsync();
            }

            ViewBag.IsAdmin = IsAdmin();
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view order details!";
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var order = await _ordersRepository.GetByIdAsync(id.Value);

            if (order == null)
            {
                return NotFound();
            }

            // Users can only see their own orders
            if (!IsAdmin() && order.UserID != userId)
            {
                TempData["ErrorMessage"] = "Access denied!";
                return RedirectToAction("Index");
            }

            ViewBag.IsAdmin = IsAdmin();
            return View(order);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to place an order!";
                return RedirectToAction("Login", "Account");
            }

            // Get available toys
            var toys = await _toysRepository.GetAllAsync();
            var availableToys = toys.Where(t => t.IsActive == true && t.Stock > 0).ToList();

            ViewBag.Toys = availableToys;
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int toyId, int quantity)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to place an order!";
                return RedirectToAction("Login", "Account");
            }

            var toy = await _toysRepository.GetByIdAsync(toyId);
            if (toy == null)
            {
                TempData["ErrorMessage"] = "Toy not found!";
                return RedirectToAction("Index", "Toys");
            }

            if (toy.IsActive != true)
            {
                TempData["ErrorMessage"] = "This toy is not available!";
                return RedirectToAction("Index", "Toys");
            }

            if (toy.Stock < quantity)
            {
                TempData["ErrorMessage"] = "Insufficient stock!";
                return RedirectToAction("Create");
            }

            var order = new Order
            {
                UserID = userId.Value,
                ToyID = toyId,
                Quantity = quantity,
                TotalPrice = toy.Price * quantity,
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            await _ordersRepository.AddAsync(order);

            // Update stock
            toy.Stock -= quantity;
            await _toysRepository.UpdateAsync(toy);

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Index");
        }

        // POST: Orders/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(int toyId, int quantity)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to place an order!";
                return RedirectToAction("Login", "Account");
            }

            var toy = await _toysRepository.GetByIdAsync(toyId);
            if (toy == null)
            {
                TempData["ErrorMessage"] = "Toy not found!";
                return RedirectToAction("Index", "Toys");
            }

            if (toy.IsActive != true)
            {
                TempData["ErrorMessage"] = "This toy is not available!";
                return RedirectToAction("Index", "Toys");
            }

            if (toy.Stock < quantity)
            {
                TempData["ErrorMessage"] = "Insufficient stock!";
                return RedirectToAction("Details", "Toys", new { id = toyId });
            }

            var order = new Order
            {
                UserID = userId.Value,
                ToyID = toyId,
                Quantity = quantity,
                TotalPrice = toy.Price * quantity,
                OrderDate = DateTime.Now,
                Status = "Pending"
            };

            await _ordersRepository.AddAsync(order);

            // Update stock
            toy.Stock -= quantity;
            await _toysRepository.UpdateAsync(toy);

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Index");
        }

        // POST: Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }

            var order = await _ordersRepository.GetByIdAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _ordersRepository.UpdateAsync(order);
                TempData["SuccessMessage"] = $"Order status updated to {status}!";
            }

            return RedirectToAction("Index");
        }

        // GET: Orders/Delete/5
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

            var order = await _ordersRepository.GetByIdAsync(id.Value);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index");
            }

            var order = await _ordersRepository.GetByIdAsync(id);
            if (order != null)
            {
                await _ordersRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Order deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/History - Show order history for current user
        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Please login to view order history!";
                return RedirectToAction("Login", "Account");
            }

            var orders = await _ordersRepository.GetByUserIdAsync(userId.Value);

            return View(orders);
        }
    }
}
