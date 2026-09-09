using BookStoreProject.DAL;
using BookStoreProject.Models;
using BookStoreProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStoreProject.Controllers
{
    public class ChatController : Controller
    {
        private readonly GeminiService _gemini;
        private readonly ApplicationDbContext _context;

        public ChatController(
            GeminiService gemini,
            ApplicationDbContext context)
        {
            _gemini = gemini;
            _context = context;
        }


        // ==========================================================
        // AI CHAT PAGE
        // ==========================================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ==========================================================
        // ASK BOOKNEST AI
        // ==========================================================

        [HttpPost]
        public async Task<IActionResult> Ask(
            [FromBody] ChatRequest request)
        {
            try
            {
                // ==================================================
                // 1. REQUEST VALIDATION
                // ==================================================

                if (request == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid request."
                    });
                }


                // ==================================================
                // 2. INPUT VALIDATION
                // ==================================================

                if (string.IsNullOrWhiteSpace(request.Mood))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Please enter what you are looking for."
                    });
                }


                string userMessage =
                    request.Mood.Trim();


                // Minimum length validation
                if (userMessage.Length < 2)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Please enter at least 2 characters."
                    });
                }


                // Maximum length validation
                if (userMessage.Length > 150)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Your request cannot exceed 150 characters."
                    });
                }


                // ==================================================
                // 3. GET SEARCH INTENT FROM GEMINI
                // ==================================================

                var intent =
                    await _gemini.GetSearchIntent(userMessage);


                // ==================================================
                // 4. VALIDATE GEMINI RESPONSE
                // ==================================================

                if (intent == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "I couldn't understand your request."
                    });
                }


                if (string.IsNullOrWhiteSpace(intent.Type))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "I couldn't understand what type of books you want."
                    });
                }


                // ==================================================
                // 5. NORMALIZE GEMINI RESPONSE
                // ==================================================

                string type =
                    intent.Type.Trim().ToLowerInvariant();

                string value =
                    intent.Value?.Trim() ?? "";


                // ==================================================
                // 6. ALLOWED SEARCH TYPES
                // ==================================================

                var allowedTypes = new[]
                {
                    "genre",
                    "top-selling",
                    "popular",
                    "new",
                    "price",
                    "author",
                    "available",
                    "general"
                };


                if (!allowedTypes.Contains(type))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "I couldn't understand that book request."
                    });
                }


                // ==================================================
                // 7. VALIDATE GENRE VALUE
                // ==================================================

                if (type == "genre")
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "Please specify a genre."
                        });
                    }


                    if (value.Length > 50)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "The genre name is too long."
                        });
                    }
                }


                // ==================================================
                // 8. VALIDATE AUTHOR VALUE
                // ==================================================

                if (type == "author")
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "Please specify an author."
                        });
                    }


                    if (value.Length > 100)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "The author name is too long."
                        });
                    }
                }


                // ==================================================
                // 9. BOOK LIST
                // ==================================================

                List<Book> books =
                    new List<Book>();

                string message =
                    "";


                // ==================================================
                // 10. GENRE SEARCH
                // ==================================================

                if (type == "genre")
                {
                    books = await _context.Books
                        .Include(b => b.Genre)
                        .AsNoTracking()
                        .Where(b =>
                            b.Genre != null &&
                            b.Genre.GenreName != null &&
                            b.Genre.GenreName
                                .ToLower()
                                .Contains(value.ToLower()))
                        .Take(5)
                        .ToListAsync();


                    message =
                        $"📚 Here are some {value} books you may enjoy.";
                }


                // ==================================================
                // 11. TOP-SELLING BOOKS
                // ==================================================

                else if (type == "top-selling")
                {
                    books = await _context.OrderDetails
                        .Include(o => o.Book)
                        .ThenInclude(b => b.Genre)
                        .Where(o =>
                            o.Book != null &&
                            o.Quantity > 0)
                        .GroupBy(o => o.BookId)
                        .OrderByDescending(g =>
                            g.Sum(x => x.Quantity))
                        .Take(5)
                        .Select(g =>
                            g.First().Book)
                        .ToListAsync();


                    message =
                        "🔥 Here are our top-selling books.";
                }


                // ==================================================
                // 12. POPULAR BOOKS
                // ==================================================

                else if (type == "popular")
                {
                    books = await _context.OrderDetails
                        .Include(o => o.Book)
                        .ThenInclude(b => b.Genre)
                        .Where(o =>
                            o.Book != null &&
                            o.Quantity > 0)
                        .GroupBy(o => o.BookId)
                        .OrderByDescending(g =>
                            g.Sum(x => x.Quantity))
                        .Take(5)
                        .Select(g =>
                            g.First().Book)
                        .ToListAsync();


                    message =
                        "⭐ Here are some of our most popular books.";
                }


                // ==================================================
                // 13. NEW BOOKS
                // ==================================================

                else if (type == "new")
                {
                    books = await _context.Books
                        .Include(b => b.Genre)
                        .AsNoTracking()
                        .OrderByDescending(b =>
                            b.BookId)
                        .Take(5)
                        .ToListAsync();


                    message =
                        "✨ Here are some of our latest books.";
                }


                // ==================================================
                // 14. PRICE SEARCH
                // ==================================================

                else if (type == "price")
                {
                    // ----------------------------------------------
                    // Validate price
                    // ----------------------------------------------

                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "Please specify a maximum price."
                        });
                    }


                    if (!double.TryParse(
                        value,
                        out double maxPrice))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "Please provide a valid price."
                        });
                    }


                    // Price must be positive
                    if (maxPrice <= 0)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "Price must be greater than zero."
                        });
                    }


                    // Maximum allowed price
                    if (maxPrice > 100000)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message =
                                "Please enter a price below ₹100000."
                        });
                    }


                    // ----------------------------------------------
                    // Search books
                    // ----------------------------------------------

                    books = await _context.Books
                        .Include(b => b.Genre)
                        .AsNoTracking()
                        .Where(b =>
                            b.Price > 0 &&
                            b.Price <= maxPrice)
                        .OrderBy(b =>
                            b.Price)
                        .Take(5)
                        .ToListAsync();


                    message =
                        $"💰 Here are some books under ₹{maxPrice}.";
                }


                // ==================================================
                // 15. AUTHOR SEARCH
                // ==================================================

                else if (type == "author")
                {
                    books = await _context.Books
                        .Include(b => b.Genre)
                        .AsNoTracking()
                        .Where(b =>
                            b.AuthorName != null &&
                            b.AuthorName
                                .ToLower()
                                .Contains(value.ToLower()))
                        .Take(5)
                        .ToListAsync();


                    message =
                        $"📖 Here are some books by {value}.";
                }


                // ==================================================
                // 16. AVAILABLE BOOKS
                // ==================================================

                else if (type == "available")
                {
                    books = await _context.Books
                        .Include(b => b.Genre)
                        .AsNoTracking()
                        .Where(b =>
                            b.Stock > 0)
                        .OrderByDescending(b =>
                            b.Stock)
                        .Take(5)
                        .ToListAsync();


                    message =
                        "📦 Here are books currently available in stock.";
                }


                // ==================================================
                // 17. GENERAL RECOMMENDATION
                // ==================================================

                else
                {
                    books = await _context.Books
                        .Include(b => b.Genre)
                        .AsNoTracking()
                        .Take(5)
                        .ToListAsync();


                    message =
                        "📚 Here are some books you might enjoy.";
                }


                // ==================================================
                // 18. VALIDATE BOOK RESULT
                // ==================================================

                if (books == null)
                {
                    return Json(new
                    {
                        success = true,
                        message =
                            "😔 Sorry, I couldn't find any matching books in our bookstore.",
                        books = new List<object>()
                    });
                }


                // Remove invalid/null books
                books = books
                    .Where(b =>
                        b != null &&
                        b.BookId > 0 &&
                        !string.IsNullOrWhiteSpace(b.BookName))
                    .ToList();


                // ==================================================
                // 19. NO BOOKS FOUND
                // ==================================================

                if (!books.Any())
                {
                    return Json(new
                    {
                        success = true,

                        message =
                            "😔 Sorry, I couldn't find any matching books in our bookstore.",

                        books =
                            new List<object>()
                    });
                }


                // ==================================================
                // 20. RETURN BOOK DATA
                // ==================================================

                var resultBooks =
                    books.Select(b => new
                    {
                        id = b.BookId,

                        name = b.BookName,

                        author = b.AuthorName,

                        price = b.Price,

                        genre = b.Genre?.GenreName,

                        description = b.Description,

                        image = b.Image,

                        stock = b.Stock
                    })
                    .ToList();


                // Validate final result
                if (!resultBooks.Any())
                {
                    return Json(new
                    {
                        success = true,

                        message =
                            "😔 Sorry, I couldn't find any matching books.",

                        books =
                            new List<object>()
                    });
                }


                // ==================================================
                // 21. FINAL JSON RESPONSE
                // ==================================================

                return Json(new
                {
                    success = true,

                    message = message,

                    books = resultBooks
                });
            }


            // ======================================================
            // ARGUMENT VALIDATION ERROR
            // ======================================================

            catch (ArgumentException ex)
            {
                Console.WriteLine(
                    "BOOKNEST AI VALIDATION ERROR");

                Console.WriteLine(
                    ex.ToString());

                return BadRequest(new
                {
                    success = false,

                    message =
                        ex.Message
                });
            }


            // ======================================================
            // DATABASE ERROR
            // ======================================================

            catch (DbUpdateException ex)
            {
                Console.WriteLine(
                    "BOOKNEST DATABASE ERROR");

                Console.WriteLine(
                    ex.ToString());

                return StatusCode(500, new
                {
                    success = false,

                    message =
                        "There was a problem accessing the bookstore database."
                });
            }


            // ======================================================
            // GENERAL ERROR
            // ======================================================

            catch (Exception ex)
            {
                Console.WriteLine(
                    "====================================");

                Console.WriteLine(
                    "BOOKNEST AI ERROR");

                Console.WriteLine(
                    ex.ToString());

                Console.WriteLine(
                    "====================================");

                return StatusCode(500, new
                {
                    success = false,

                    message =
                        "BookNest AI is temporarily unavailable. Please try again."
                });
            }
        }
    }
}