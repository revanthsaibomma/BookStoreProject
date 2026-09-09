using AutoMapper;
using BookStoreProject.Controllers;
using BookStoreProject.DTOs;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BookStoreProject.Tests.ControllerTests
{
    [TestClass]
    public class AdminGenreControllerTests
    {
        private Mock<IGenreRepo> _genreRepoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private AdminGenreController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _genreRepoMock = new Mock<IGenreRepo>();
            _mapperMock = new Mock<IMapper>();

            _controller = new AdminGenreController(
                _genreRepoMock.Object,
                _mapperMock.Object);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }

        // ==========================================================
        // INDEX
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldReturnView_WhenGenresExist()
        {
            var genres = new List<Genre>
            {
                CreateGenre(1, "Fiction"),
                CreateGenre(2, "Science")
            };

            var genreDtos = new List<GenreDto>
            {
                CreateGenreDto(1, "Fiction"),
                CreateGenreDto(2, "Science")
            };

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(genres);

            _mapperMock
                .Setup(x => x.Map<IEnumerable<GenreDto>>(genres))
                .Returns(genreDtos);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(genreDtos, viewResult.Model);
        }

        [TestMethod]
        public async Task Index_ShouldReturnEmptyList_WhenGenresAreNull()
        {
            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync((IEnumerable<Genre>)null!);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<GenreDto>;

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);

            Assert.AreEqual(
                "Unable to load genres.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Index_ShouldReturnEmptyList_WhenMappingReturnsNull()
        {
            var genres = new List<Genre>
            {
                CreateGenre(1, "Fiction")
            };

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(genres);

            _mapperMock
                .Setup(x => x.Map<IEnumerable<GenreDto>>(genres))
                .Returns((IEnumerable<GenreDto>)null!);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<GenreDto>;

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);

            Assert.AreEqual(
                "Unable to prepare genre data.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Index_ShouldReturnEmptyList_WhenRepositoryThrowsException()
        {
            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception());

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<GenreDto>;

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);

            Assert.AreEqual(
                "An error occurred while loading genres.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // CREATE - GET
        // ==========================================================

        [TestMethod]
        public void Create_Get_ShouldReturnView()
        {
            var result = _controller.Create();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        // ==========================================================
        // CREATE - POST
        // ==========================================================

        [TestMethod]
        public async Task Create_Post_ShouldRedirectToIndex_WhenValidGenre()
        {
            var dto = CreateGenreDto(0, "Fiction");
            var genre = CreateGenre(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Genre>());

            _mapperMock
                .Setup(x => x.Map<Genre>(dto))
                .Returns(genre);

            var result = await _controller.Create(dto);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Genre created successfully.",
                _controller.TempData["Success"]);

            _mapperMock.Verify(
                x => x.Map<Genre>(dto),
                Times.Once);

            _genreRepoMock.Verify(
                x => x.AddAsync(genre),
                Times.Once);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenDtoIsNull()
        {
            var result = await _controller.Create(null!);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.IsFalse(_controller.ModelState.IsValid);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(""));
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var dto = CreateGenreDto(0, "Fiction");

            _controller.ModelState.AddModelError(
                "GenreName",
                "Genre name is invalid.");

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            _genreRepoMock.Verify(
                x => x.GetAllAsync(),
                Times.Never);

            _genreRepoMock.Verify(
                x => x.AddAsync(It.IsAny<Genre>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenGenreNameIsEmpty()
        {
            var dto = CreateGenreDto(0, "");

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreName)));
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenGenreNameIsWhitespace()
        {
            var dto = CreateGenreDto(0, "   ");

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreName)));
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenGenreNameIsLessThanTwoCharacters()
        {
            var dto = CreateGenreDto(0, "A");

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreName)));

            Assert.AreEqual(
                "Genre name must contain at least 2 characters.",
                _controller.ModelState[nameof(dto.GenreName)]!
                    .Errors[0].ErrorMessage);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenGenreNameExceeds40Characters()
        {
            var dto = CreateGenreDto(
                0,
                new string('A', 41));

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreName)));
        }

        [TestMethod]
        public async Task Create_Post_ShouldTrimGenreName()
        {
            var dto = CreateGenreDto(0, "  Fiction  ");

            var genre = CreateGenre(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Genre>());

            _mapperMock
                .Setup(x => x.Map<Genre>(dto))
                .Returns(genre);

            await _controller.Create(dto);

            Assert.AreEqual("Fiction", dto.GenreName);
        }

        [TestMethod]
        public async Task Create_Post_ShouldRejectDuplicateGenre()
        {
            var dto = CreateGenreDto(0, "Fiction");

            var genres = new List<Genre>
            {
                CreateGenre(1, "Fiction")
            };

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(genres);

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreName)));

            Assert.AreEqual(
                "This genre already exists.",
                _controller.ModelState[nameof(dto.GenreName)]!
                    .Errors[0].ErrorMessage);

            _genreRepoMock.Verify(
                x => x.AddAsync(It.IsAny<Genre>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Create_Post_ShouldRejectDuplicateGenre_IgnoringCase()
        {
            var dto = CreateGenreDto(0, "fiction");

            var genres = new List<Genre>
            {
                CreateGenre(1, "Fiction")
            };

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(genres);

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreName)));

            Assert.AreEqual(
                "This genre already exists.",
                _controller.ModelState[nameof(dto.GenreName)]!
                    .Errors[0].ErrorMessage);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenMapperReturnsNull()
        {
            var dto = CreateGenreDto(0, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Genre>());

            _mapperMock
                .Setup(x => x.Map<Genre>(dto))
                .Returns((Genre)null!);

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(""));
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenRepositoryThrowsException()
        {
            var dto = CreateGenreDto(0, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ThrowsAsync(new Exception());

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(""));

            Assert.AreEqual(
                "An error occurred while creating the genre.",
                _controller.ModelState[""]!
                    .Errors[0].ErrorMessage);
        }

        // ==========================================================
        // EDIT - GET
        // ==========================================================

        [TestMethod]
        public async Task Edit_Get_ShouldReturnBadRequest_WhenIdIsInvalid()
        {
            var result = await _controller.Edit(0);

            var badRequest = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequest);

            Assert.AreEqual(
                "Invalid genre ID.",
                badRequest.Value);
        }

        [TestMethod]
        public async Task Edit_Get_ShouldReturnNotFound_WhenGenreDoesNotExist()
        {
            _genreRepoMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Genre?)null);

            var result = await _controller.Edit(99);

            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Get_ShouldReturnView_WhenGenreExists()
        {
            var genre = CreateGenre(1, "Fiction");
            var dto = CreateGenreDto(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(genre);

            _mapperMock
                .Setup(x => x.Map<GenreDto>(genre))
                .Returns(dto);

            var result = await _controller.Edit(1);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);
        }

        [TestMethod]
        public async Task Edit_Get_ShouldRedirectToIndex_WhenMappingReturnsNull()
        {
            var genre = CreateGenre(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(genre);

            _mapperMock
                .Setup(x => x.Map<GenreDto>(genre))
                .Returns((GenreDto)null!);

            var result = await _controller.Edit(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Unable to prepare genre data.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Edit_Get_ShouldRedirectToIndex_WhenRepositoryThrowsException()
        {
            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.Edit(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "An error occurred while loading the genre.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // EDIT - POST
        // ==========================================================

        [TestMethod]
        public async Task Edit_Post_ShouldRedirectToIndex_WhenValid()
        {
            var dto = CreateGenreDto(1, "Science");
            var genre = CreateGenre(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(genre);

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Genre>
                {
                    genre
                });

            _mapperMock
                .Setup(x => x.Map(dto, genre))
                .Returns(genre);

            var result = await _controller.Edit(dto);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Genre updated successfully.",
                _controller.TempData["Success"]);

            _mapperMock.Verify(
                x => x.Map(dto, genre),
                Times.Once);

            _genreRepoMock.Verify(
                x => x.UpdateAsync(genre),
                Times.Once);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenDtoIsNull()
        {
            var result = await _controller.Edit(null!);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsFalse(_controller.ModelState.IsValid);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenIdIsInvalid()
        {
            var dto = CreateGenreDto(0, "Fiction");

            var result = await _controller.Edit(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.Id)));

            Assert.AreEqual(
                "Invalid genre ID.",
                _controller.ModelState[nameof(dto.Id)]!
                    .Errors[0].ErrorMessage);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var dto = CreateGenreDto(1, "Fiction");

            _controller.ModelState.AddModelError(
                "GenreName",
                "Invalid genre name.");

            var result = await _controller.Edit(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            _genreRepoMock.Verify(
                x => x.GetByIdAsync(It.IsAny<int>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenGenreNameIsEmpty()
        {
            var dto = CreateGenreDto(1, "");

            var result = await _controller.Edit(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreName)));
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenGenreNameIsTooShort()
        {
            var dto = CreateGenreDto(1, "A");

            var result = await _controller.Edit(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.AreEqual(
                "Genre name must contain at least 2 characters.",
                _controller.ModelState[nameof(dto.GenreName)]!
                    .Errors[0].ErrorMessage);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenGenreNameIsTooLong()
        {
            var dto = CreateGenreDto(
                1,
                new string('A', 41));

            var result = await _controller.Edit(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreName)));
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnNotFound_WhenGenreDoesNotExist()
        {
            var dto = CreateGenreDto(99, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Genre?)null);

            var result = await _controller.Edit(dto);

            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_ShouldRejectDuplicateGenre()
        {
            var dto = CreateGenreDto(1, "Science");

            var genres = new List<Genre>
            {
                CreateGenre(1, "Fiction"),
                CreateGenre(2, "Science")
            };

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(genres[0]);

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(genres);

            var result = await _controller.Edit(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.AreEqual(
                "Another genre with this name already exists.",
                _controller.ModelState[nameof(dto.GenreName)]!
                    .Errors[0].ErrorMessage);

            _genreRepoMock.Verify(
                x => x.UpdateAsync(It.IsAny<Genre>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldAllowSameGenreName_ForSameId()
        {
            var dto = CreateGenreDto(1, "Fiction");

            var genre = CreateGenre(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(genre);

            _genreRepoMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Genre>
                {
                    genre
                });

            _mapperMock
                .Setup(x => x.Map(dto, genre))
                .Returns(genre);

            var result = await _controller.Edit(dto);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenRepositoryThrowsException()
        {
            var dto = CreateGenreDto(1, "Science");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.Edit(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.AreEqual(
                "An error occurred while updating the genre.",
                _controller.ModelState[""]!
                    .Errors[0].ErrorMessage);
        }

        // ==========================================================
        // DELETE - GET
        // ==========================================================

        [TestMethod]
        public async Task Delete_ShouldReturnBadRequest_WhenIdIsInvalid()
        {
            var result = await _controller.Delete(0);

            var badRequest = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequest);

            Assert.AreEqual(
                "Invalid genre ID.",
                badRequest.Value);
        }

        [TestMethod]
        public async Task Delete_ShouldReturnNotFound_WhenGenreDoesNotExist()
        {
            _genreRepoMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Genre?)null);

            var result = await _controller.Delete(99);

            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnView_WhenGenreExists()
        {
            var genre = CreateGenre(1, "Fiction");
            var dto = CreateGenreDto(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(genre);

            _mapperMock
                .Setup(x => x.Map<GenreDto>(genre))
                .Returns(dto);

            var result = await _controller.Delete(1);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);
        }

        [TestMethod]
        public async Task Delete_ShouldRedirectToIndex_WhenMappingReturnsNull()
        {
            var genre = CreateGenre(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(genre);

            _mapperMock
                .Setup(x => x.Map<GenreDto>(genre))
                .Returns((GenreDto)null!);

            var result = await _controller.Delete(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Unable to prepare genre data.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Delete_ShouldRedirectToIndex_WhenRepositoryThrowsException()
        {
            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.Delete(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "An error occurred while loading the genre.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // DELETE - POST
        // ==========================================================

        [TestMethod]
        public async Task DeleteConfirmed_ShouldRedirectToIndex_WhenSuccessful()
        {
            var genre = CreateGenre(1, "Fiction");

            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(genre);

            var result = await _controller.DeleteConfirmed(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Genre deleted successfully.",
                _controller.TempData["Success"]);

            _genreRepoMock.Verify(
                x => x.DeleteAsync(1),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteConfirmed_ShouldReturnBadRequest_WhenIdIsInvalid()
        {
            var result = await _controller.DeleteConfirmed(0);

            var badRequest = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequest);

            Assert.AreEqual(
                "Invalid genre ID.",
                badRequest.Value);
        }

        [TestMethod]
        public async Task DeleteConfirmed_ShouldReturnNotFound_WhenGenreDoesNotExist()
        {
            _genreRepoMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Genre?)null);

            var result = await _controller.DeleteConfirmed(99);

            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));

            _genreRepoMock.Verify(
                x => x.DeleteAsync(It.IsAny<int>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteConfirmed_ShouldRedirectToIndex_WhenRepositoryThrowsException()
        {
            _genreRepoMock
                .Setup(x => x.GetByIdAsync(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.DeleteConfirmed(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Unable to delete the genre. It may be in use by books.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // HELPER METHODS
        // ==========================================================

        private static Genre CreateGenre(
            int id,
            string name)
        {
            return new Genre
            {
                Id = id,
                GenreName = name
            };
        }

        private static GenreDto CreateGenreDto(
            int id,
            string name)
        {
            return new GenreDto
            {
                Id = id,
                GenreName = name
            };
        }
    }
}