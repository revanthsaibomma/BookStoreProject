using AutoMapper;
using BookStoreProject.Controllers;
using BookStoreProject.DTOs;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;

namespace BookStoreProject.Tests.ControllerTests
{
    [TestClass]
    public class HomeControllerTests
    {
        private Mock<IBookRepo> _bookRepoMock;
        private Mock<ICartRepo> _cartRepoMock;
        private Mock<IMapper> _mapperMock;
        private HomeController _controller;


        // =========================================================
        // TEST INITIALIZATION
        // =========================================================

        [TestInitialize]
        public void Setup()
        {
            // Mock repositories
            _bookRepoMock = new Mock<IBookRepo>();
            _cartRepoMock = new Mock<ICartRepo>();

            // Mock AutoMapper
            _mapperMock = new Mock<IMapper>();

            // Configure Book -> BookDisplayDto mapping
            _mapperMock
                .Setup(m => m.Map<BookDisplayDto>(It.IsAny<Book>()))
                .Returns((Book book) => new BookDisplayDto
                {
                    BookId = book.BookId,
                    BookName = book.BookName,
                    AuthorName = book.AuthorName,
                    Price = book.Price,
                    Description = book.Description,
                    Image = book.Image,
                    GenreId = book.GenreId,
                    GenreName = book.Genre?.GenreName,
                    Stock = book.Stock
                });

            // Create controller
            _controller = new HomeController(
                _bookRepoMock.Object,
                _cartRepoMock.Object,
                _mapperMock.Object);

            // IMPORTANT:
            // Create an empty authenticated/unauthenticated user
            // so Controller.User is not null.
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity())
                }
            };
        }



        // =========================================================
        // DETAIL() TESTS
        // =========================================================

        [TestMethod]
        public async Task Detail_ReturnsBadRequest_WhenIdIsInvalid()
        {
            // Arrange
            int id = 0;

            // Act
            var result = await _controller.Detail(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));
        }


        [TestMethod]
        public async Task Detail_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            int id = 999;

            _bookRepoMock
                .Setup(repo => repo.SearchById(id))
                .ReturnsAsync((Book)null);

            // Act
            var result = await _controller.Detail(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(NotFoundObjectResult));

            // Verify repository method was called once
            _bookRepoMock.Verify(
                repo => repo.SearchById(id),
                Times.Once);
        }


        [TestMethod]
        public async Task Detail_ReturnsView_WhenBookExists()
        {
            // Arrange
            int id = 1;

            var book = new Book
            {
                BookId = 1,
                BookName = "C# Programming",
                AuthorName = "John Smith",
                Price = 500,
                Description = "Learn C# programming",
                Image = "csharp.jpg",
                GenreId = 1,
                Stock = 10,

                Genre = new Genre
                {
                    Id = 1,
                    GenreName = "Programming"
                }
            };

            _bookRepoMock
                .Setup(repo => repo.SearchById(id))
                .ReturnsAsync(book);

            // Act
            var result = await _controller.Detail(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(ViewResult));

            _bookRepoMock.Verify(
                repo => repo.SearchById(id),
                Times.Once);
        }


        [TestMethod]
        public async Task Detail_ReturnsViewWithCorrectBookData_WhenBookExists()
        {
            // Arrange
            int id = 1;

            var book = new Book
            {
                BookId = 1,
                BookName = "ASP.NET Core",
                AuthorName = "Microsoft",
                Price = 750,
                Description = "ASP.NET Core Programming",
                Image = "aspnet.jpg",
                GenreId = 1,
                Stock = 15,

                Genre = new Genre
                {
                    Id = 1,
                    GenreName = "Programming"
                }
            };

            _bookRepoMock
                .Setup(repo => repo.SearchById(id))
                .ReturnsAsync(book);

            // Act
            var result = await _controller.Detail(id);

            // Assert
            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as BookDisplayDto;

            Assert.IsNotNull(model);

            Assert.AreEqual(
                1,
                model.BookId);

            Assert.AreEqual(
                "ASP.NET Core",
                model.BookName);

            Assert.AreEqual(
                "Microsoft",
                model.AuthorName);

            Assert.AreEqual(
                750,
                model.Price);

            Assert.AreEqual(
                "Programming",
                model.GenreName);

            Assert.AreEqual(
                15,
                model.Stock);

            // Verify mapper was called
            _mapperMock.Verify(
                m => m.Map<BookDisplayDto>(book),
                Times.Once);
        }


        [TestMethod]
        public async Task Detail_ReturnsBadRequest_WhenBookIdIsInvalid()
        {
            // Arrange
            int id = 1;

            var book = new Book
            {
                BookId = 0,
                BookName = "Invalid Book",
                AuthorName = "Test Author",
                Price = 500,
                Description = "Invalid book",
                GenreId = 1,
                Stock = 10,

                Genre = new Genre
                {
                    Id = 1,
                    GenreName = "Programming"
                }
            };

            _bookRepoMock
                .Setup(repo => repo.SearchById(id))
                .ReturnsAsync(book);

            // Act
            var result = await _controller.Detail(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(BadRequestObjectResult));

            _bookRepoMock.Verify(
                repo => repo.SearchById(id),
                Times.Once);
        }


        // =========================================================
        // INDEX() TESTS
        // =========================================================

        [TestMethod]
        public async Task Index_ReturnsView_WhenBooksExist()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book
                {
                    BookId = 1,
                    BookName = "C# Programming",
                    AuthorName = "John Smith",
                    Price = 500,
                    Description = "C# book",
                    Image = "csharp.jpg",
                    GenreId = 1,
                    Stock = 10,

                    Genre = new Genre
                    {
                        Id = 1,
                        GenreName = "Programming"
                    }
                },

                new Book
                {
                    BookId = 2,
                    BookName = "ASP.NET Core",
                    AuthorName = "Microsoft",
                    Price = 700,
                    Description = "ASP.NET Core book",
                    Image = "aspnet.jpg",
                    GenreId = 1,
                    Stock = 15,

                    Genre = new Genre
                    {
                        Id = 1,
                        GenreName = "Programming"
                    }
                }
            };

            _bookRepoMock
                .Setup(repo => repo.GetBooks())
                .ReturnsAsync(books);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(ViewResult));

            _bookRepoMock.Verify(
                repo => repo.GetBooks(),
                Times.Once);
        }


        [TestMethod]
        public async Task Index_ReturnsView_WhenNoBooksExist()
        {
            // Arrange
            var books = new List<Book>();

            _bookRepoMock
                .Setup(repo => repo.GetBooks())
                .ReturnsAsync(books);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(ViewResult));

            _bookRepoMock.Verify(
                repo => repo.GetBooks(),
                Times.Once);
        }


        [TestMethod]
        public async Task Index_ReturnsServerError_WhenRepositoryThrowsException()
        {
            // Arrange
            _bookRepoMock
                .Setup(repo => repo.GetBooks())
                .ThrowsAsync(
                    new Exception("Database error"));

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(ObjectResult));

            var objectResult = result as ObjectResult;

            Assert.IsNotNull(objectResult);

            Assert.AreEqual(
                500,
                objectResult.StatusCode);
        }
    }
}