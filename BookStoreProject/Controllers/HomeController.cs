using AutoMapper;
using BookStoreProject.DTOs;
using BookStoreProject.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStoreProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBookRepo _bookRepo;
        private readonly ICartRepo _cartRepo;
        private readonly IMapper _mapper;

        public HomeController(
            IBookRepo bookRepo,
            ICartRepo cartRepo,
            IMapper mapper)
        {
            _bookRepo = bookRepo;
            _cartRepo = cartRepo;
            _mapper = mapper;
        }


        // ==========================================
        // HOME PAGE
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Admin users should go to Admin dashboard
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }


                // ==================================
                // GET USER ID
                // ==================================

                int userId = 0;

                string? claimId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(claimId))
                {
                    if (!int.TryParse(claimId, out userId))
                    {
                        return BadRequest(
                            "Invalid user information.");
                    }

                    if (userId < 0)
                    {
                        return BadRequest(
                            "Invalid user ID.");
                    }
                }


                // ==================================
                // GET BOOKS
                // ==================================

                var books = await _bookRepo.GetBooks();

                if (books == null)
                {
                    return View(new List<BookDisplayDto>());
                }


                // ==================================
                // GET GENRES
                // ==================================

                ViewBag.Genres = books
                    .Select(b => b.Genre)
                    .Where(g => g != null)
                    .DistinctBy(g => g.Id)
                    .ToList();


                // ==================================
                // MAP BOOKS
                // ==================================

                var model = new List<BookDisplayDto>();

                foreach (var book in books)
                {
                    if (book == null)
                        continue;

                    var bookDto =
                        _mapper.Map<BookDisplayDto>(book);

                    // Get quantity only for logged-in users
                    if (userId > 0)
                    {
                        bookDto.Quantity =
                            await _cartRepo.GetBookQuantity(
                                userId,
                                book.BookId);
                    }
                    else
                    {
                        bookDto.Quantity = 0;
                    }

                    model.Add(bookDto);
                }


                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("====================================");
                Console.WriteLine("HOME INDEX ERROR");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("====================================");

                return StatusCode(500,
                    "Something went wrong while loading books.");
            }
        }


        // ==========================================
        // BOOK DETAILS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                // ==================================
                // VALIDATE BOOK ID
                // ==================================

                if (id <= 0)
                {
                    return BadRequest(
                        "Invalid book ID.");
                }


                // ==================================
                // GET BOOK
                // ==================================

                var book = await _bookRepo.SearchById(id);


                // ==================================
                // BOOK NOT FOUND
                // ==================================

                if (book == null)
                {
                    return NotFound(
                        $"Book with ID {id} was not found.");
                }


                // ==================================
                // VALIDATE BOOK DATA
                // ==================================

                if (book.BookId <= 0)
                {
                    return BadRequest(
                        "Invalid book information.");
                }


                // ==================================
                // MAP TO DTO
                // ==================================

                var bookDto =
                    _mapper.Map<BookDisplayDto>(book);


                if (bookDto == null)
                {
                    return StatusCode(
                        500,
                        "Unable to load book details.");
                }


                return View(bookDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine("====================================");
                Console.WriteLine("BOOK DETAIL ERROR");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("====================================");

                return StatusCode(
                    500,
                    "Something went wrong while loading the book.");
            }
        }
    }
}