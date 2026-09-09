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
    public class AdminBooksControllerTests
    {
        private Mock<IBookRepo> _bookRepoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private AdminBooksController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _bookRepoMock = new Mock<IBookRepo>();
            _mapperMock = new Mock<IMapper>();

            _controller = new AdminBooksController(
                _bookRepoMock.Object,
                _mapperMock.Object);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        }

        // ==========================================================
        // INDEX
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldReturnView_WhenBooksExist()
        {
            var books = new List<Book>
            {
                CreateBook(1, "Book One"),
                CreateBook(2, "Book Two")
            };

            var bookDtos = new List<BookDto>
            {
                new BookDto { BookId = 1, BookName = "Book One" },
                new BookDto { BookId = 2, BookName = "Book Two" }
            };

            _bookRepoMock
                .Setup(x => x.GetBooks())
                .ReturnsAsync(books);

            _mapperMock
                .Setup(x => x.Map<List<BookDto>>(books))
                .Returns(bookDtos);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(List<BookDto>));

            var model = (List<BookDto>)viewResult.Model!;

            Assert.AreEqual(2, model.Count);
            Assert.AreEqual("Book One", model[0].BookName);
        }

        [TestMethod]
        public async Task Index_ShouldReturnEmptyList_WhenBooksAreNull()
        {
            _bookRepoMock
                .Setup(x => x.GetBooks())
                .ReturnsAsync((IEnumerable<Book>?)null);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<BookDto>;

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);

            Assert.AreEqual(
                "Unable to load books.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Index_ShouldReturnEmptyList_WhenMappingReturnsNull()
        {
            var books = new List<Book>
            {
                CreateBook(1, "Test Book")
            };

            _bookRepoMock
                .Setup(x => x.GetBooks())
                .ReturnsAsync(books);

            _mapperMock
                .Setup(x => x.Map<List<BookDto>>(books))
                .Returns((List<BookDto>?)null);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<BookDto>;

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);

            Assert.AreEqual(
                "Unable to prepare book data.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Index_ShouldReturnEmptyList_WhenRepositoryThrowsException()
        {
            _bookRepoMock
                .Setup(x => x.GetBooks())
                .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<BookDto>;

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);

            Assert.AreEqual(
                "An error occurred while loading books.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // DETAILS
        // ==========================================================

        [TestMethod]
        public async Task Details_ShouldReturnBadRequest_WhenIdIsInvalid()
        {
            var result = await _controller.Details(0);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            var badRequest = (BadRequestObjectResult)result;

            Assert.AreEqual(
                "Invalid book ID.",
                badRequest.Value);
        }

        [TestMethod]
        public async Task Details_ShouldReturnBadRequest_WhenIdIsNegative()
        {
            var result = await _controller.Details(-1);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Details_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            _bookRepoMock
                .Setup(x => x.SearchById(100))
                .ReturnsAsync((Book?)null);

            var result = await _controller.Details(100);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Details_ShouldReturnView_WhenBookExists()
        {
            var book = CreateBook(1, "Test Book");

            var dto = new BookDto
            {
                BookId = 1,
                BookName = "Test Book"
            };

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            _mapperMock
                .Setup(x => x.Map<BookDto>(book))
                .Returns(dto);

            var result = await _controller.Details(1);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);
        }

        [TestMethod]
        public async Task Details_ShouldRedirectToIndex_WhenMappingReturnsNull()
        {
            var book = CreateBook(1, "Test Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            _mapperMock
                .Setup(x => x.Map<BookDto>(book))
                .Returns((BookDto?)null);

            var result = await _controller.Details(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Unable to prepare book details.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Details_ShouldRedirectToIndex_WhenRepositoryThrowsException()
        {
            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.Details(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "An error occurred while loading book details.",
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
        public async Task Create_Post_ShouldRedirectToIndex_WhenBookIsValid()
        {
            var dto = CreateBookDto();

            var book = CreateBook(
                1,
                dto.BookName!);

            _mapperMock
                .Setup(x => x.Map<Book>(dto))
                .Returns(book);

            var result = await _controller.Create(dto);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Book created successfully.",
                _controller.TempData["Success"]);

            _bookRepoMock.Verify(
                x => x.AddBook(book),
                Times.Once);

            _bookRepoMock.Verify(
                x => x.Save(),
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
            var dto = CreateBookDto();

            _controller.ModelState.AddModelError(
                "BookName",
                "Book name is required.");

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            _bookRepoMock.Verify(
                x => x.AddBook(It.IsAny<Book>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenMapperReturnsNull()
        {
            var dto = CreateBookDto();

            _mapperMock
                .Setup(x => x.Map<Book>(dto))
                .Returns((Book?)null);

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenPriceIsZero()
        {
            var dto = CreateBookDto();
            dto.Price = 0;

            var book = CreateBook(1, "Test Book");
            book.Price = 0;

            _mapperMock
                .Setup(x => x.Map<Book>(dto))
                .Returns(book);

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.Price)));

            _bookRepoMock.Verify(
                x => x.AddBook(It.IsAny<Book>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenStockIsNegative()
        {
            var dto = CreateBookDto();
            dto.Stock = -1;

            var book = CreateBook(1, "Test Book");
            book.Stock = -1;

            _mapperMock
                .Setup(x => x.Map<Book>(dto))
                .Returns(book);

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.Stock)));

            _bookRepoMock.Verify(
                x => x.AddBook(It.IsAny<Book>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenGenreIdIsInvalid()
        {
            var dto = CreateBookDto();
            dto.GenreId = 0;

            var book = CreateBook(1, "Test Book");
            book.GenreId = 0;

            _mapperMock
                .Setup(x => x.Map<Book>(dto))
                .Returns(book);

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreId)));

            _bookRepoMock.Verify(
                x => x.AddBook(It.IsAny<Book>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Create_Post_ShouldReturnView_WhenRepositoryThrowsException()
        {
            var dto = CreateBookDto();
            var book = CreateBook(1, "Test Book");

            _mapperMock
                .Setup(x => x.Map<Book>(dto))
                .Returns(book);

            _bookRepoMock
                .Setup(x => x.AddBook(book))
                .ThrowsAsync(new Exception());

            var result = await _controller.Create(dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(""));
        }

        // ==========================================================
        // EDIT - GET
        // ==========================================================

        [TestMethod]
        public async Task Edit_Get_ShouldReturnBadRequest_WhenIdIsInvalid()
        {
            var result = await _controller.Edit(0);

            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Edit_Get_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            _bookRepoMock
                .Setup(x => x.SearchById(99))
                .ReturnsAsync((Book?)null);

            var result = await _controller.Edit(99);

            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Get_ShouldReturnView_WhenBookExists()
        {
            var book = CreateBook(1, "Test Book");

            var dto = new UpdateBookDto
            {
                BookId = 1,
                BookName = "Test Book",
                AuthorName = "Test Author",
                Price = 500,
                Description = "Test description",
                GenreId = 1,
                Stock = 10
            };

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            _mapperMock
                .Setup(x => x.Map<UpdateBookDto>(book))
                .Returns(dto);

            var result = await _controller.Edit(1);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);
        }

        [TestMethod]
        public async Task Edit_Get_ShouldRedirectToIndex_WhenMappingReturnsNull()
        {
            var book = CreateBook(1, "Test Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            _mapperMock
                .Setup(x => x.Map<UpdateBookDto>(book))
                .Returns((UpdateBookDto?)null);

            var result = await _controller.Edit(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Unable to prepare book data.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Edit_Get_ShouldRedirectToIndex_WhenRepositoryThrowsException()
        {
            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.Edit(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "An error occurred while loading the book.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // EDIT - POST
        // ==========================================================

        [TestMethod]
        public async Task Edit_Post_ShouldRedirectToIndex_WhenValid()
        {
            var dto = CreateUpdateBookDto(1);
            var book = CreateBook(1, "Old Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            _mapperMock
                .Setup(x => x.Map(dto, book))
                .Returns(book);

            var result = await _controller.Edit(1, dto);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Book updated successfully.",
                _controller.TempData["Success"]);

            _bookRepoMock.Verify(
                x => x.UpdateBook(book),
                Times.Once);

            _bookRepoMock.Verify(
                x => x.Save(),
                Times.Once);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnBadRequest_WhenIdIsInvalid()
        {
            var dto = CreateUpdateBookDto(1);

            var result = await _controller.Edit(0, dto);

            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.Edit(1, null!);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.IsFalse(_controller.ModelState.IsValid);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnBadRequest_WhenIdsDoNotMatch()
        {
            var dto = CreateUpdateBookDto(2);

            var result = await _controller.Edit(1, dto);

            var badRequest = result as BadRequestObjectResult;

            Assert.IsNotNull(badRequest);

            Assert.AreEqual(
                "Book ID does not match.",
                badRequest.Value);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            var dto = CreateUpdateBookDto(1);

            _controller.ModelState.AddModelError(
                "BookName",
                "Invalid book name.");

            var result = await _controller.Edit(1, dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            _bookRepoMock.Verify(
                x => x.SearchById(It.IsAny<int>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            var dto = CreateUpdateBookDto(1);

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync((Book?)null);

            var result = await _controller.Edit(1, dto);

            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenPriceIsInvalid()
        {
            var dto = CreateUpdateBookDto(1);
            dto.Price = 0;

            var book = CreateBook(1, "Test Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            var result = await _controller.Edit(1, dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.Price)));

            _bookRepoMock.Verify(
                x => x.UpdateBook(It.IsAny<Book>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenStockIsNegative()
        {
            var dto = CreateUpdateBookDto(1);
            dto.Stock = -1;

            var book = CreateBook(1, "Test Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            var result = await _controller.Edit(1, dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.Stock)));

            _bookRepoMock.Verify(
                x => x.UpdateBook(It.IsAny<Book>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenGenreIdIsInvalid()
        {
            var dto = CreateUpdateBookDto(1);
            dto.GenreId = 0;

            var book = CreateBook(1, "Test Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            var result = await _controller.Edit(1, dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(nameof(dto.GenreId)));

            _bookRepoMock.Verify(
                x => x.UpdateBook(It.IsAny<Book>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Edit_Post_ShouldReturnView_WhenRepositoryThrowsException()
        {
            var dto = CreateUpdateBookDto(1);
            var book = CreateBook(1, "Test Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            _bookRepoMock
                .Setup(x => x.UpdateBook(book))
                .ThrowsAsync(new Exception());

            _mapperMock
                .Setup(x => x.Map(dto, book))
                .Returns(book);

            var result = await _controller.Edit(1, dto);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);

            Assert.IsTrue(
                _controller.ModelState.ContainsKey(""));
        }

        // ==========================================================
        // DELETE - GET
        // ==========================================================

        [TestMethod]
        public async Task Delete_ShouldReturnBadRequest_WhenIdIsInvalid()
        {
            var result = await _controller.Delete(0);

            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            _bookRepoMock
                .Setup(x => x.SearchById(99))
                .ReturnsAsync((Book?)null);

            var result = await _controller.Delete(99);

            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Delete_ShouldReturnView_WhenBookExists()
        {
            var book = CreateBook(1, "Test Book");

            var dto = new BookDto
            {
                BookId = 1,
                BookName = "Test Book"
            };

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            _mapperMock
                .Setup(x => x.Map<BookDto>(book))
                .Returns(dto);

            var result = await _controller.Delete(1);

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.AreSame(dto, viewResult.Model);
        }

        [TestMethod]
        public async Task Delete_ShouldRedirectToIndex_WhenMappingReturnsNull()
        {
            var book = CreateBook(1, "Test Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            _mapperMock
                .Setup(x => x.Map<BookDto>(book))
                .Returns((BookDto?)null);

            var result = await _controller.Delete(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Unable to prepare book data.",
                _controller.TempData["Error"]);
        }

        [TestMethod]
        public async Task Delete_ShouldRedirectToIndex_WhenRepositoryThrowsException()
        {
            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.Delete(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "An error occurred while loading the book.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // DELETE - POST
        // ==========================================================

        [TestMethod]
        public async Task DeleteConfirmed_ShouldRedirectToIndex_WhenSuccessful()
        {
            var book = CreateBook(1, "Test Book");

            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ReturnsAsync(book);

            var result = await _controller.DeleteConfirmed(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "Book deleted successfully.",
                _controller.TempData["Success"]);

            _bookRepoMock.Verify(
                x => x.DeleteBook(1),
                Times.Once);

            _bookRepoMock.Verify(
                x => x.Save(),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteConfirmed_ShouldReturnBadRequest_WhenIdIsInvalid()
        {
            var result = await _controller.DeleteConfirmed(0);

            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task DeleteConfirmed_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            _bookRepoMock
                .Setup(x => x.SearchById(99))
                .ReturnsAsync((Book?)null);

            var result = await _controller.DeleteConfirmed(99);

            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundResult));

            _bookRepoMock.Verify(
                x => x.DeleteBook(It.IsAny<int>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteConfirmed_ShouldRedirectToIndex_WhenRepositoryThrowsException()
        {
            _bookRepoMock
                .Setup(x => x.SearchById(1))
                .ThrowsAsync(new Exception());

            var result = await _controller.DeleteConfirmed(1);

            var redirectResult = result as RedirectToActionResult;

            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);

            Assert.AreEqual(
                "An error occurred while deleting the book.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // HELPER METHODS
        // ==========================================================

        private static Book CreateBook(
            int id,
            string name)
        {
            return new Book
            {
                BookId = id,
                BookName = name,
                AuthorName = "Test Author",
                Price = 500,
                Description = "Test book description.",
                Image = "test.jpg",
                GenreId = 1,
                Stock = 20
            };
        }

        private static CreateBookDto CreateBookDto()
        {
            return new CreateBookDto
            {
                BookName = "Test Book",
                AuthorName = "Test Author",
                Price = 500,
                Description = "Test book description.",
                Image = "test.jpg",
                GenreId = 1,
                Stock = 20
            };
        }

        private static UpdateBookDto CreateUpdateBookDto(int id)
        {
            return new UpdateBookDto
            {
                BookId = id,
                BookName = "Updated Book",
                AuthorName = "Updated Author",
                Price = 600,
                Description = "Updated book description.",
                Image = "updated.jpg",
                GenreId = 1,
                Stock = 25
            };
        }
    }
}