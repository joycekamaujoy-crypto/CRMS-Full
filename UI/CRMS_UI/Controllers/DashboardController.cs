using Microsoft.AspNetCore.Mvc;
using CRMS_UI.Services.Interfaces;
using CRMS_UI.ViewModels.Dashboard;

namespace CRMS_UI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;

        public DashboardController(IApiService apiService)
        {
            _apiService = apiService;
        }

        // Why: Middleware-like check to ensure the user is logged in before proceeding.
        private IActionResult CheckAuthentication()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                // If not authenticated, force them to log in
                return RedirectToAction("Login", "Auth");
            }
            return null; // Return null if authenticated (means check passed)
        }

        // GET: /Dashboard/AdminDashboard
        public async Task<IActionResult> AdminDashboard()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            // Why: Enforce role-based access for the Owner dashboard.
            if (HttpContext.Session.GetString("UserRole") != "Owner")
            {
                return RedirectToAction("UserDashboard");
            }

            ViewData["Title"] = "Admin Dashboard";

            var viewModel = new AdminDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("UserName") ?? "Admin"
            };

            try
            {
                // Fetch active rentals count
                viewModel.ActiveRentals = await _apiService.GetAsync<int>("booking/owner/all", HttpContext);

                // Fetch total vehicles
                viewModel.TotalVehicles = await _apiService.GetAsync<int>("vehicle/all/all", HttpContext);

                // Fetch pending approvals
                viewModel.PendingApprovals = await _apiService.GetAsync<int>("rentals/pending/count", HttpContext);

                // Fetch tracking enabled percent (example)
                viewModel.TrackingEnabledPercent = await _apiService.GetAsync<int>("telemetry/ingest/all", HttpContext);
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"Failed to fetch dashboard data: {ex.Message}";
            }

            return View(viewModel);

            // NOTE: In a real app, we would fetch KPI data here using _apiService.
        }

        // GET: /Dashboard/UserDashboard
        public IActionResult UserDashboard()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            ViewData["Title"] = "User Dashboard";
            // NOTE: In a real app, we would fetch car availability and user's active bookings here.
            return View();
        }
    }
}
