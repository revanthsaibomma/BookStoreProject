using AutoMapper;
using BookStoreProject.DTOs;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminBooksController : Controller
    {
        private readonly IBookRepo _bookRepo;
        private readonly IMapper _mapper;

        public AdminBooksController(
            IBookRepo bookRepo,
            IMapper mapper)
        {
            _bookRepo = bookRepo;
            _mapper = mapper;
        }


        // ------------------------------------------------
        // READ LIST
        // ------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var books = await _bookRepo.GetBooks();

                if (books == null)
                {
                    TempData["Error"] = "Unable to load books.";
                    return View(new List<BookDto>());
                }

                var bookDtos =
                    _mapper.Map<List<BookDto>>(books);

                if (bookDtos == null)
                {
                    TempData["Error"] = "Unable to prepare book data.";
                    return View(new List<BookDto>());
                }

                return View(bookDtos);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading books.";

                return View(new List<BookDto>());
            }
        }


        // ------------------------------------------------
        // READ SINGLE
        // ------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest("Invalid book ID.");
                }

                var book =
                    await _bookRepo.SearchById(id);

                if (book == null)
                {
                    return NotFound();
                }

                var bookDto =
                    _mapper.Map<BookDto>(book);

                if (bookDto == null)
                {
                    TempData["Error"] =
                        "Unable to prepare book details.";

                    return RedirectToAction(nameof(Index));
                }

                return View(bookDto);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading book details.";

                return RedirectToAction(nameof(Index));
            }
        }


        // ------------------------------------------------
        // CREATE - GET
        // ------------------------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // ------------------------------------------------
        // CREATE - POST
        // ------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateBookDto dto)
        {
            try
            {
                // Validate DTO
                if (dto == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid book data.");

                    return View();
                }

                // Validate DTO properties
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                // DTO → Entity
                var book =
                    _mapper.Map<Book>(dto);

                if (book == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to create book data.");

                    return View(dto);
                }

                // Validate important values
                if (book.Price <= 0)
                {
                    ModelState.AddModelError(
                        nameof(dto.Price),
                        "Price must be greater than zero.");

                    return View(dto);
                }

                if (book.Stock < 0)
                {
                    ModelState.AddModelError(
                        nameof(dto.Stock),
                        "Stock cannot be negative.");

                    return View(dto);
                }

                if (book.GenreId <= 0)
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreId),
                        "Please select a valid genre.");

                    return View(dto);
                }

                await _bookRepo.AddBook(book);

                await _bookRepo.Save();

                TempData["Success"] =
                    "Book created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "An error occurred while creating the book.");

                return View(dto);
            }
        }


        // ------------------------------------------------
        // UPDATE - GET
        // ------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest("Invalid book ID.");
                }

                var book =
                    await _bookRepo.SearchById(id);

                if (book == null)
                {
                    return NotFound();
                }

                // Entity → DTO
                var dto =
                    _mapper.Map<UpdateBookDto>(book);

                if (dto == null)
                {
                    TempData["Error"] =
                        "Unable to prepare book data.";

                    return RedirectToAction(nameof(Index));
                }

                return View(dto);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading the book.";

                return RedirectToAction(nameof(Index));
            }
        }


        // ------------------------------------------------
        // UPDATE - POST
        // ------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            UpdateBookDto dto)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest("Invalid book ID.");
                }

                // Validate DTO
                if (dto == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid book data.");

                    return View(dto);
                }

                // Prevent ID manipulation
                if (id != dto.BookId)
                {
                    return BadRequest(
                        "Book ID does not match.");
                }

                // Validate DTO properties
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                // Check whether book exists
                var book =
                    await _bookRepo.SearchById(id);

                if (book == null)
                {
                    return NotFound();
                }

                // Validate important values
                if (dto.Price <= 0)
                {
                    ModelState.AddModelError(
                        nameof(dto.Price),
                        "Price must be greater than zero.");

                    return View(dto);
                }

                if (dto.Stock < 0)
                {
                    ModelState.AddModelError(
                        nameof(dto.Stock),
                        "Stock cannot be negative.");

                    return View(dto);
                }

                if (dto.GenreId <= 0)
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreId),
                        "Please select a valid genre.");

                    return View(dto);
                }

                // DTO → existing Entity
                _mapper.Map(dto, book);

                await _bookRepo.UpdateBook(book);

                await _bookRepo.Save();

                TempData["Success"] =
                    "Book updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "An error occurred while updating the book.");

                return View(dto);
            }
        }


        // ------------------------------------------------
        // DELETE - GET
        // ------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest("Invalid book ID.");
                }

                var book =
                    await _bookRepo.SearchById(id);

                if (book == null)
                {
                    return NotFound();
                }

                // Entity → DTO
                var dto =
                    _mapper.Map<BookDto>(book);

                if (dto == null)
                {
                    TempData["Error"] =
                        "Unable to prepare book data.";

                    return RedirectToAction(nameof(Index));
                }

                return View(dto);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading the book.";

                return RedirectToAction(nameof(Index));
            }
        }


        // ------------------------------------------------
        // DELETE - POST
        // ------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest("Invalid book ID.");
                }

                // Check whether book exists
                var book =
                    await _bookRepo.SearchById(id);

                if (book == null)
                {
                    return NotFound();
                }

                await _bookRepo.DeleteBook(id);

                await _bookRepo.Save();

                TempData["Success"] =
                    "Book deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while deleting the book.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}