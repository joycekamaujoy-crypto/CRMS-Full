using Microsoft.AspNetCore.Mvc;
using CRMS_UI.Services.Interfaces;

namespace CRMS_UI.Controllers
{
    public class TrackingController : Controller
    {
        private readonly IConfiguration _configuration;

        // Inject IConfiguration
        public TrackingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private IActionResult CheckAuth()
        {
            // Note: We allow both Renter and Owner to view tracking, 
            // but the backend should only send data for *active* rentals/vehicles.
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        // GET: /Tracking/Index
        public IActionResult Index()
        {
            var authCheck = CheckAuth();
            if (authCheck != null) return authCheck;

            ViewData["Title"] = "Real-time Vehicle Tracking";
            ViewData["ApiSettings:SignalRHub"] = _configuration["ApiSettings:SignalRHub"];
            return View();
        }
    }
}
