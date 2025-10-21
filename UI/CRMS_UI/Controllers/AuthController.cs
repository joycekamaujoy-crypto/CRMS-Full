using Microsoft.AspNetCore.Mvc;
using CRMS_UI.Services.Interfaces;
using CRMS_UI.ViewModels.Auth;
using System.Net;

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
        public IActionResult Register()
        {
            ViewData["Title"] = "Register";
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _apiService.PostAsync<AuthResponse, RegisterViewModel>("auth/register", model, HttpContext);

                TempData["SuccessMessage"] = "Registration successful! Please sign in to continue.";
                return RedirectToAction("Login");
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
                return View(model);
            }
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
                    return RedirectToAction("UserDashboard", "Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Login successful, but role is unrecognized.");
                HttpContext.Session.Clear();
                return View(model);
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("Email has not been confirmed"))
                {
                    ModelState.AddModelError(string.Empty, "You must confirm your email before you can log in. Please check your inbox.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Login failed. Please check your credentials and try again.");
                }
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during login.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
