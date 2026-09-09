using AutoMapper;
using BookStoreProject.DTOs;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminGenreController : Controller
    {
        private readonly IGenreRepo _genreRepo;
        private readonly IMapper _mapper;

        public AdminGenreController(
            IGenreRepo genreRepo,
            IMapper mapper)
        {
            _genreRepo = genreRepo;
            _mapper = mapper;
        }


        // GET: /AdminGenre
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var genres = await _genreRepo.GetAllAsync();

                if (genres == null)
                {
                    TempData["Error"] =
                        "Unable to load genres.";

                    return View(new List<GenreDto>());
                }

                var genreDtos =
                    _mapper.Map<IEnumerable<GenreDto>>(genres);

                if (genreDtos == null)
                {
                    TempData["Error"] =
                        "Unable to prepare genre data.";

                    return View(new List<GenreDto>());
                }

                return View(genreDtos);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading genres.";

                return View(new List<GenreDto>());
            }
        }


        // GET: /AdminGenre/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // POST: /AdminGenre/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GenreDto dto)
        {
            try
            {
                // Validate DTO
                if (dto == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid genre data.");

                    return View();
                }

                // Validate DTO properties
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                // Validate genre name
                if (string.IsNullOrWhiteSpace(dto.GenreName))
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreName),
                        "Genre name is required.");

                    return View(dto);
                }

                dto.GenreName = dto.GenreName.Trim();

                // Validate minimum length
                if (dto.GenreName.Length < 2)
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreName),
                        "Genre name must contain at least 2 characters.");

                    return View(dto);
                }

                // Validate maximum length
                if (dto.GenreName.Length > 40)
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreName),
                        "Genre name cannot exceed 40 characters.");

                    return View(dto);
                }

                // Check duplicate genre
                var genres = await _genreRepo.GetAllAsync();

                if (genres != null &&
                    genres.Any(g =>
                        !string.IsNullOrWhiteSpace(g.GenreName) &&
                        g.GenreName.Trim()
                            .Equals(
                                dto.GenreName,
                                StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreName),
                        "This genre already exists.");

                    return View(dto);
                }

                // DTO → Entity
                var genre =
                    _mapper.Map<Genre>(dto);

                if (genre == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to create genre.");

                    return View(dto);
                }

                await _genreRepo.AddAsync(genre);

                TempData["Success"] =
                    "Genre created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "An error occurred while creating the genre.");

                return View(dto);
            }
        }


        // GET: /AdminGenre/Edit/1
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest("Invalid genre ID.");
                }

                var genre =
                    await _genreRepo.GetByIdAsync(id);

                if (genre == null)
                {
                    return NotFound();
                }

                var dto =
                    _mapper.Map<GenreDto>(genre);

                if (dto == null)
                {
                    TempData["Error"] =
                        "Unable to prepare genre data.";

                    return RedirectToAction(nameof(Index));
                }

                return View(dto);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading the genre.";

                return RedirectToAction(nameof(Index));
            }
        }


        // POST: /AdminGenre/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GenreDto dto)
        {
            try
            {
                // Validate DTO
                if (dto == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Invalid genre data.");

                    return View();
                }

                // Validate ID
                if (dto.Id <= 0)
                {
                    ModelState.AddModelError(
                        nameof(dto.Id),
                        "Invalid genre ID.");

                    return View(dto);
                }

                // Validate ModelState
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                // Validate genre name
                if (string.IsNullOrWhiteSpace(dto.GenreName))
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreName),
                        "Genre name is required.");

                    return View(dto);
                }

                dto.GenreName = dto.GenreName.Trim();

                if (dto.GenreName.Length < 2)
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreName),
                        "Genre name must contain at least 2 characters.");

                    return View(dto);
                }

                if (dto.GenreName.Length > 40)
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreName),
                        "Genre name cannot exceed 40 characters.");

                    return View(dto);
                }

                // Check whether genre exists
                var genre =
                    await _genreRepo.GetByIdAsync(dto.Id);

                if (genre == null)
                {
                    return NotFound();
                }

                // Check duplicate genre name
                var genres = await _genreRepo.GetAllAsync();

                if (genres != null &&
                    genres.Any(g =>
                        g.Id != dto.Id &&
                        !string.IsNullOrWhiteSpace(g.GenreName) &&
                        g.GenreName.Trim()
                            .Equals(
                                dto.GenreName,
                                StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(
                        nameof(dto.GenreName),
                        "Another genre with this name already exists.");

                    return View(dto);
                }

                // DTO → existing Entity
                _mapper.Map(dto, genre);

                await _genreRepo.UpdateAsync(genre);

                TempData["Success"] =
                    "Genre updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "An error occurred while updating the genre.");

                return View(dto);
            }
        }


        // GET: /AdminGenre/Delete/1
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest("Invalid genre ID.");
                }

                var genre =
                    await _genreRepo.GetByIdAsync(id);

                if (genre == null)
                {
                    return NotFound();
                }

                var dto =
                    _mapper.Map<GenreDto>(genre);

                if (dto == null)
                {
                    TempData["Error"] =
                        "Unable to prepare genre data.";

                    return RedirectToAction(nameof(Index));
                }

                return View(dto);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading the genre.";

                return RedirectToAction(nameof(Index));
            }
        }


        // POST: /AdminGenre/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Validate ID
                if (id <= 0)
                {
                    return BadRequest("Invalid genre ID.");
                }

                // Check whether genre exists
                var genre =
                    await _genreRepo.GetByIdAsync(id);

                if (genre == null)
                {
                    return NotFound();
                }

                await _genreRepo.DeleteAsync(id);

                TempData["Success"] =
                    "Genre deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to delete the genre. It may be in use by books.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}