using lily.Models;
using lily.Repository;
using Microsoft.AspNetCore.Mvc;

namespace lily.Controllers
{
    public class AccountController : Controller
    {
        private readonly UsersRepository _usersRepository;

        public AccountController(UsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            // If already logged in, redirect to home
            if (HttpContext.Session.GetInt32("UserID") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _usersRepository.GetByUsernameAsync(username);

            if (user != null && user.Password == password)
            {
                // Set session variables
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role ?? "User");

                TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorMessage"] = "Invalid username or password!";
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("UserID") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if username exists
                if (await _usersRepository.UsernameExistsAsync(user.Username))
                {
                    TempData["ErrorMessage"] = "Username already exists!";
                    return View(user);
                }

                user.Role = "User";
                user.CreatedDate = DateTime.Now;

                await _usersRepository.AddAsync(user);

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            // Clear session
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully!";
            return RedirectToAction("Login");
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _usersRepository.GetByIdAsync(userId.Value);
            return View(user);
        }

        // GET: Account/ManageUsers
        public async Task<IActionResult> ManageUsers()
        {
            // Check if user is admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Admin access required!";
                return RedirectToAction("Index", "Home");
            }

            var users = await _usersRepository.GetAllAsync();
            return View(users);
        }
    }
}
