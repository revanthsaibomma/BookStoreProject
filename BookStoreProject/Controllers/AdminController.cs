using AutoMapper;
using BookStoreProject.Repository;
using BookStoreProject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminOrderRepo _adminRepo;
        private readonly IMapper _mapper;

        public AdminController(
            IAdminOrderRepo adminRepo,
            IMapper mapper)
        {
            _adminRepo = adminRepo;
            _mapper = mapper;
        }

        // ==========================
        // ADMIN DASHBOARD
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Validate repository
                if (_adminRepo == null)
                {
                    TempData["Error"] =
                        "Dashboard service is unavailable.";

                    return View(new AdminDashboardDto());
                }

                var dashboard =
                    await _adminRepo.GetDashboardDataAsync();

                // Validate dashboard data
                if (dashboard == null)
                {
                    TempData["Error"] =
                        "Unable to load dashboard data.";

                    return View(new AdminDashboardDto());
                }

                var dashboardDto =
                    _mapper.Map<AdminDashboardDto>(dashboard);

                // Validate mapped DTO
                if (dashboardDto == null)
                {
                    TempData["Error"] =
                        "Unable to prepare dashboard data.";

                    return View(new AdminDashboardDto());
                }

                return View(dashboardDto);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading the admin dashboard.";

                return View(new AdminDashboardDto());
            }
        }
    }
}