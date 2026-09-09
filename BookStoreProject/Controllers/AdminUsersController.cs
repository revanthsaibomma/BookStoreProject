using AutoMapper;
using BookStoreProject.Data;
using BookStoreProject.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AdminUsersController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        // ==========================
        // DISPLAY USERS
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Get all users
                var users =
                    await _userManager.Users.ToListAsync();

                // Validate user collection
                if (users == null)
                {
                    TempData["Error"] =
                        "Unable to load users.";

                    return View(new List<ApplicationUserDto>());
                }

                // ApplicationUser → ApplicationUserDto
                var userDtos =
                    _mapper.Map<List<ApplicationUserDto>>(users);

                // Validate mapping
                if (userDtos == null)
                {
                    TempData["Error"] =
                        "Unable to prepare user data.";

                    return View(new List<ApplicationUserDto>());
                }

                return View(userDtos);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading users.";

                return View(new List<ApplicationUserDto>());
            }
        }
    }
}