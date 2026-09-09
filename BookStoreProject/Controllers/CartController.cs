using AutoMapper;
using BookStoreProject.DTOs;
using BookStoreProject.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStoreProject.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartRepo _cartRepo;
        private readonly IMapper _mapper;

        public CartController(
            ICartRepo cartRepo,
            IMapper mapper)
        {
            _cartRepo = cartRepo;
            _mapper = mapper;
        }


        // ==========================
        // DISPLAY SHOPPING CART
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                string? claimId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Validate logged-in user
                if (!int.TryParse(claimId, out int userId) || userId <= 0)
                {
                    return LocalRedirect("/Identity/Account/Login");
                }

                var cart =
                    await _cartRepo.GetUserCart(userId);

                if (cart == null)
                {
                    return View(new List<CartItemDto>());
                }

                // CartDetail → CartItemDto
                var cartDto =
                    _mapper.Map<List<CartItemDto>>(cart);

                return View(cartDto);
            }
            catch (Exception)
            {
                TempData["Error"] = "Unable to load your cart.";
                return View(new List<CartItemDto>());
            }
        }


        // ==========================
        // ADD BOOK TO CART
        // ==========================
        [HttpGet]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            try
            {
                // Validate Book ID
                if (bookId <= 0)
                {
                    TempData["Error"] = "Invalid book selected.";
                    return RedirectToAction("Index", "Home");
                }

                string? claimId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Validate user
                if (!int.TryParse(claimId, out int userId) || userId <= 0)
                {
                    return LocalRedirect("/Identity/Account/Login");
                }

                await _cartRepo.AddItem(bookId, userId);

                TempData["Success"] = "Book added to cart.";

                return RedirectToAction("Index", "Cart");
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to add the book to your cart.";

                return RedirectToAction("Index", "Home");
            }
        }


        // ==========================
        // HOME PAGE (+)
        // ==========================
        [HttpGet]
        public async Task<IActionResult> IncreaseFromHome(int bookId)
        {
            try
            {
                // Validate Book ID
                if (bookId <= 0)
                {
                    TempData["Error"] = "Invalid book selected.";
                    return RedirectToAction("Index", "Home");
                }

                string? claimId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Validate user
                if (!int.TryParse(claimId, out int userId) || userId <= 0)
                {
                    return LocalRedirect("/Identity/Account/Login");
                }

                await _cartRepo.IncreaseBookQuantity(
                    userId,
                    bookId);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to increase the book quantity.";

                return RedirectToAction("Index", "Home");
            }
        }


        // ==========================
        // HOME PAGE (-)
        // ==========================
        [HttpGet]
        public async Task<IActionResult> DecreaseFromHome(int bookId)
        {
            try
            {
                // Validate Book ID
                if (bookId <= 0)
                {
                    TempData["Error"] = "Invalid book selected.";
                    return RedirectToAction("Index", "Home");
                }

                string? claimId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Validate user
                if (!int.TryParse(claimId, out int userId) || userId <= 0)
                {
                    return LocalRedirect("/Identity/Account/Login");
                }

                await _cartRepo.DecreaseBookQuantity(
                    userId,
                    bookId);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to decrease the book quantity.";

                return RedirectToAction("Index", "Home");
            }
        }


        // ==========================
        // CART PAGE (+)
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Increase(int id)
        {
            try
            {
                // Validate CartDetail ID
                if (id <= 0)
                {
                    TempData["Error"] = "Invalid cart item.";
                    return RedirectToAction("Index");
                }

                // Validate logged-in user
                string? claimId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(claimId, out int userId) || userId <= 0)
                {
                    return LocalRedirect("/Identity/Account/Login");
                }

                await _cartRepo.IncreaseQuantity(id);

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to increase the book quantity.";

                return RedirectToAction("Index");
            }
        }


        // ==========================
        // CART PAGE (-)
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Decrease(int id)
        {
            try
            {
                // Validate CartDetail ID
                if (id <= 0)
                {
                    TempData["Error"] = "Invalid cart item.";
                    return RedirectToAction("Index");
                }

                // Validate logged-in user
                string? claimId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(claimId, out int userId) || userId <= 0)
                {
                    return LocalRedirect("/Identity/Account/Login");
                }

                await _cartRepo.DecreaseQuantity(id);

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to decrease the book quantity.";

                return RedirectToAction("Index");
            }
        }
    }
}