using Microsoft.AspNetCore.Mvc;
using CRMS_UI.Services.Interfaces;
using CRMS_UI.ViewModels.Auth;

namespace CRMS_UI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;

        public AuthController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            ViewData["Title"] = "Login";
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiService.PostAsync<AuthResponse, LoginViewModel>("auth/login", model, HttpContext);

                HttpContext.Session.SetString("JWToken", response.Token);
                HttpContext.Session.SetString("UserRole", response.Role);
                HttpContext.Session.SetString("UserName", response.Name);

                if (response.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("AdminDashboard", "Dashboard");
                }
                else if (response.Role.Equals("Renter", StringComparison.OrdinalIgnoreCase))
                {
                    // Why: Renters view available cars and their own bookings (User view)
                    return RedirectToAction("UserDashboard", "Dashboard");
                }

                // Fallback for an unrecognized role
                ModelState.AddModelError(string.Empty, "Login successful, but role is unrecognized.");
                HttpContext.Session.Clear();
                return View(model);
            }
            catch (HttpRequestException ex)
            {
                // Log the exception details and show a user-friendly error
                Console.WriteLine($"Login failed: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Login failed. Please check your credentials and try again.");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during login.");
                return View(model);
            }
        }

        // GET: /Auth/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Clear the session and redirect to the login page
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
