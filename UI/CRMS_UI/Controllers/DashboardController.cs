using Microsoft.AspNetCore.Mvc;
using CRMS_UI.Services.Interfaces;
using CRMS_UI.ViewModels.Dashboard;
using System.Threading.Tasks; // Make sure this is included for Task

namespace CRMS_UI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;

        public DashboardController(IApiService apiService)
        {
            _apiService = apiService;
        }

        private IActionResult CheckAuthentication()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        // GET: /Dashboard/AdminDashboard
        public async Task<IActionResult> AdminDashboard()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

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
                // Start all API calls in parallel
                // NOTE: Update these paths to your real API endpoints
                var activeRentalsTask = _apiService.GetAsync<int>("booking/count/owner/active", HttpContext);
                var totalVehiclesTask = _apiService.GetAsync<int>("vehicle/count/all", HttpContext);
                var pendingApprovalsTask = _apiService.GetAsync<int>("booking/count/pending", HttpContext);
                var trackingEnabledTask = _apiService.GetAsync<int>("telemetry/count/active", HttpContext);

                // Wait for all of them to complete
                await Task.WhenAll(activeRentalsTask, totalVehiclesTask, pendingApprovalsTask, trackingEnabledTask);

                // Assign the results from the completed tasks
                viewModel.ActiveRentals = activeRentalsTask.Result;
                viewModel.TotalVehicles = totalVehiclesTask.Result;
                viewModel.PendingApprovals = pendingApprovalsTask.Result;
                viewModel.TrackingEnabledPercent = trackingEnabledTask.Result;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to fetch dashboard data: {ex.Message}";
            }

            return View(viewModel);
        }

        // GET: /Dashboard/UserDashboard
        public async Task<IActionResult> UserDashboard()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            ViewData["Title"] = "User Dashboard";

            var viewModel = new UserDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("UserName") ?? "Renter"
            };

            try
            {
                // Start all API calls in parallel
                // NOTE: Update these paths to your real API endpoints
                var availableCarsTask = _apiService.GetAsync<int>("vehicle/count/available", HttpContext);
                var activeBookingsTask = _apiService.GetAsync<int>("booking/count/my-active", HttpContext);
                var pendingApprovalsTask = _apiService.GetAsync<int>("booking/count/my-pending", HttpContext);

                // Wait for all of them to complete
                await Task.WhenAll(availableCarsTask, activeBookingsTask, pendingApprovalsTask);

                // Assign the results from the completed tasks
                viewModel.AvailableCarsCount = availableCarsTask.Result;
                viewModel.ActiveBookingsCount = activeBookingsTask.Result;
                viewModel.PendingApprovalsCount = pendingApprovalsTask.Result;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to fetch your dashboard data: {ex.Message}";
            }

            return View(viewModel);
        }
    }
}