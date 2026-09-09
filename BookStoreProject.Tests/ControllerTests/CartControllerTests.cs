using AutoMapper;
using BookStoreProject.Controllers;
using BookStoreProject.DTOs;
using BookStoreProject.Repository;
using BookStoreProject.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Security.Claims;

namespace BookStoreProject.Tests.ControllerTests
{
    [TestClass]
    public class CartControllerTests
    {
        private Mock<ICartRepo> _cartRepoMock;
        private Mock<IMapper> _mapperMock;
        private CartController _controller;


        // =========================================================
        // TEST INITIALIZATION
        // =========================================================

        [TestInitialize]
        public void Setup()
        {
            // Mock Cart Repository
            _cartRepoMock = new Mock<ICartRepo>();

            // Mock AutoMapper
            _mapperMock = new Mock<IMapper>();

            // Configure ShoppingCartVM -> CartItemDto mapping
            _mapperMock
                .Setup(m => m.Map<List<CartItemDto>>(
                    It.IsAny<List<ShoppingCartVM>>()))
                .Returns((List<ShoppingCartVM> cart) =>
                    cart.Select(item => new CartItemDto
                    {
                        CartDetailId = item.CartDetailId,
                        BookId = item.BookId,
                        BookName = item.BookName,
                        Image = item.Image,
                        Price = item.Price,
                        Quantity = item.Quantity
                    }).ToList());

            // Create Controller
            _controller = new CartController(
                _cartRepoMock.Object,
                _mapperMock.Object);

            // Configure HttpContext
            var httpContext = new DefaultHttpContext();

            // Configure TempData
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());
        }


        // =========================================================
        // HELPER METHOD
        // =========================================================

        private void SetUser(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                "TestAuthentication");

            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = principal;
        }


        // =========================================================
        // INDEX() TESTS
        // =========================================================

        [TestMethod]
        public async Task Index_RedirectsToLogin_WhenUserIsNotLoggedIn()
        {
            // Arrange
            // No user claim is provided.

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(LocalRedirectResult));

            var redirectResult = result as LocalRedirectResult;

            Assert.AreEqual(
                "/Identity/Account/Login",
                redirectResult.Url);
        }


        [TestMethod]
        public async Task Index_ReturnsView_WhenCartExists()
        {
            // Arrange
            SetUser(1);

            var cart = new List<ShoppingCartVM>
            {
                new ShoppingCartVM
                {
                    CartDetailId = 1,
                    BookId = 10,
                    BookName = "C# Programming",
                    Image = "csharp.jpg",
                    Price = 500,
                    Quantity = 2
                }
            };

            _cartRepoMock
                .Setup(repo => repo.GetUserCart(1))
                .ReturnsAsync(cart);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(ViewResult));

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(
                viewResult.Model,
                typeof(List<CartItemDto>));

            var model = viewResult.Model as List<CartItemDto>;

            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("C# Programming", model[0].BookName);
            Assert.AreEqual(500, model[0].Price);
            Assert.AreEqual(2, model[0].Quantity);

            // Verify repository
            _cartRepoMock.Verify(
                repo => repo.GetUserCart(1),
                Times.Once);

            // Verify mapper
            _mapperMock.Verify(
                m => m.Map<List<CartItemDto>>(cart),
                Times.Once);
        }


        [TestMethod]
        public async Task Index_ReturnsEmptyView_WhenCartIsNull()
        {
            // Arrange
            SetUser(1);

            _cartRepoMock
                .Setup(repo => repo.GetUserCart(1))
                .ReturnsAsync((List<ShoppingCartVM>)null);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(ViewResult));

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<CartItemDto>;

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);
        }


        [TestMethod]
        public async Task Index_ReturnsEmptyView_WhenRepositoryThrowsException()
        {
            // Arrange
            SetUser(1);

            _cartRepoMock
                .Setup(repo => repo.GetUserCart(1))
                .ThrowsAsync(
                    new Exception("Database error"));

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(ViewResult));

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as List<CartItemDto>;

            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Count);

            Assert.AreEqual(
                "Unable to load your cart.",
                _controller.TempData["Error"]);
        }


        // =========================================================
        // AddToCart() TESTS
        // =========================================================

        [TestMethod]
        public async Task AddToCart_RedirectsToHome_WhenBookIdIsInvalid()
        {
            // Arrange
            int bookId = 0;

            // Act
            var result = await _controller.AddToCart(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Home",
                redirectResult.ControllerName);

            Assert.AreEqual(
                "Invalid book selected.",
                _controller.TempData["Error"]);
        }


        [TestMethod]
        public async Task AddToCart_RedirectsToLogin_WhenUserIsNotLoggedIn()
        {
            // Arrange
            int bookId = 10;

            // Act
            var result = await _controller.AddToCart(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(LocalRedirectResult));

            var redirectResult = result as LocalRedirectResult;

            Assert.AreEqual(
                "/Identity/Account/Login",
                redirectResult.Url);

            // Repository should not be called
            _cartRepoMock.Verify(
                repo => repo.AddItem(
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }


        [TestMethod]
        public async Task AddToCart_AddsBookAndRedirectsToCart_WhenValid()
        {
            // Arrange
            int bookId = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo => repo.AddItem(bookId, userId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddToCart(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Cart",
                redirectResult.ControllerName);

            Assert.AreEqual(
                "Book added to cart.",
                _controller.TempData["Success"]);

            // Verify repository call
            _cartRepoMock.Verify(
                repo => repo.AddItem(bookId, userId),
                Times.Once);
        }


        [TestMethod]
        public async Task AddToCart_RedirectsToHome_WhenRepositoryThrowsException()
        {
            // Arrange
            int bookId = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo => repo.AddItem(bookId, userId))
                .ThrowsAsync(
                    new Exception("Database error"));

            // Act
            var result = await _controller.AddToCart(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Home",
                redirectResult.ControllerName);

            Assert.AreEqual(
                "Unable to add the book to your cart.",
                _controller.TempData["Error"]);
        }


        // =========================================================
        // IncreaseFromHome() TESTS
        // =========================================================

        [TestMethod]
        public async Task IncreaseFromHome_RedirectsToHome_WhenBookIdIsInvalid()
        {
            // Arrange
            int bookId = 0;

            // Act
            var result = await _controller.IncreaseFromHome(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Home",
                redirectResult.ControllerName);

            Assert.AreEqual(
                "Invalid book selected.",
                _controller.TempData["Error"]);
        }


        [TestMethod]
        public async Task IncreaseFromHome_RedirectsToLogin_WhenUserIsNotLoggedIn()
        {
            // Arrange
            int bookId = 10;

            // Act
            var result = await _controller.IncreaseFromHome(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(LocalRedirectResult));

            var redirectResult = result as LocalRedirectResult;

            Assert.AreEqual(
                "/Identity/Account/Login",
                redirectResult.Url);
        }


        [TestMethod]
        public async Task IncreaseFromHome_IncreasesQuantityAndRedirectsToHome()
        {
            // Arrange
            int bookId = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo =>
                    repo.IncreaseBookQuantity(userId, bookId))
                .Returns(Task.CompletedTask);

            // Act
            var result =
                await _controller.IncreaseFromHome(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Home",
                redirectResult.ControllerName);

            _cartRepoMock.Verify(
                repo =>
                    repo.IncreaseBookQuantity(userId, bookId),
                Times.Once);
        }


        [TestMethod]
        public async Task IncreaseFromHome_RedirectsToHome_WhenRepositoryThrowsException()
        {
            // Arrange
            int bookId = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo =>
                    repo.IncreaseBookQuantity(userId, bookId))
                .ThrowsAsync(
                    new Exception("Database error"));

            // Act
            var result =
                await _controller.IncreaseFromHome(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Home",
                redirectResult.ControllerName);

            Assert.AreEqual(
                "Unable to increase the book quantity.",
                _controller.TempData["Error"]);
        }


        // =========================================================
        // DecreaseFromHome() TESTS
        // =========================================================

        [TestMethod]
        public async Task DecreaseFromHome_RedirectsToHome_WhenBookIdIsInvalid()
        {
            // Arrange
            int bookId = 0;

            // Act
            var result = await _controller.DecreaseFromHome(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Home",
                redirectResult.ControllerName);
        }


        [TestMethod]
        public async Task DecreaseFromHome_RedirectsToLogin_WhenUserIsNotLoggedIn()
        {
            // Arrange
            int bookId = 10;

            // Act
            var result =
                await _controller.DecreaseFromHome(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(LocalRedirectResult));

            var redirectResult = result as LocalRedirectResult;

            Assert.AreEqual(
                "/Identity/Account/Login",
                redirectResult.Url);
        }


        [TestMethod]
        public async Task DecreaseFromHome_DecreasesQuantityAndRedirectsToHome()
        {
            // Arrange
            int bookId = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo =>
                    repo.DecreaseBookQuantity(userId, bookId))
                .Returns(Task.CompletedTask);

            // Act
            var result =
                await _controller.DecreaseFromHome(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Home",
                redirectResult.ControllerName);

            _cartRepoMock.Verify(
                repo =>
                    repo.DecreaseBookQuantity(userId, bookId),
                Times.Once);
        }


        [TestMethod]
        public async Task DecreaseFromHome_RedirectsToHome_WhenRepositoryThrowsException()
        {
            // Arrange
            int bookId = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo =>
                    repo.DecreaseBookQuantity(userId, bookId))
                .ThrowsAsync(
                    new Exception("Database error"));

            // Act
            var result =
                await _controller.DecreaseFromHome(bookId);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Home",
                redirectResult.ControllerName);

            Assert.AreEqual(
                "Unable to decrease the book quantity.",
                _controller.TempData["Error"]);
        }


        // =========================================================
        // Increase() TESTS
        // =========================================================

        [TestMethod]
        public async Task Increase_RedirectsToCart_WhenIdIsInvalid()
        {
            // Arrange
            int id = 0;

            // Act
            var result = await _controller.Increase(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.IsNull(
                redirectResult.ControllerName);

            Assert.AreEqual(
                "Invalid cart item.",
                _controller.TempData["Error"]);
        }


        [TestMethod]
        public async Task Increase_RedirectsToLogin_WhenUserIsNotLoggedIn()
        {
            // Arrange
            int id = 10;

            // Act
            var result = await _controller.Increase(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(LocalRedirectResult));

            var redirectResult = result as LocalRedirectResult;

            Assert.AreEqual(
                "/Identity/Account/Login",
                redirectResult.Url);
        }


        [TestMethod]
        public async Task Increase_IncreasesQuantityAndRedirectsToCart()
        {
            // Arrange
            int id = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo => repo.IncreaseQuantity(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Increase(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.IsNull(
                redirectResult.ControllerName);

            _cartRepoMock.Verify(
                repo => repo.IncreaseQuantity(id),
                Times.Once);
        }


        [TestMethod]
        public async Task Increase_RedirectsToCart_WhenRepositoryThrowsException()
        {
            // Arrange
            int id = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo => repo.IncreaseQuantity(id))
                .ThrowsAsync(
                    new Exception("Database error"));

            // Act
            var result = await _controller.Increase(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Unable to increase the book quantity.",
                _controller.TempData["Error"]);
        }


        // =========================================================
        // Decrease() TESTS
        // =========================================================

        [TestMethod]
        public async Task Decrease_RedirectsToCart_WhenIdIsInvalid()
        {
            // Arrange
            int id = 0;

            // Act
            var result = await _controller.Decrease(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.IsNull(
                redirectResult.ControllerName);

            Assert.AreEqual(
                "Invalid cart item.",
                _controller.TempData["Error"]);
        }


        [TestMethod]
        public async Task Decrease_RedirectsToLogin_WhenUserIsNotLoggedIn()
        {
            // Arrange
            int id = 10;

            // Act
            var result = await _controller.Decrease(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(LocalRedirectResult));

            var redirectResult = result as LocalRedirectResult;

            Assert.AreEqual(
                "/Identity/Account/Login",
                redirectResult.Url);
        }


        [TestMethod]
        public async Task Decrease_DecreasesQuantityAndRedirectsToCart()
        {
            // Arrange
            int id = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo => repo.DecreaseQuantity(id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Decrease(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.IsNull(
                redirectResult.ControllerName);

            _cartRepoMock.Verify(
                repo => repo.DecreaseQuantity(id),
                Times.Once);
        }


        [TestMethod]
        public async Task Decrease_RedirectsToCart_WhenRepositoryThrowsException()
        {
            // Arrange
            int id = 10;
            int userId = 1;

            SetUser(userId);

            _cartRepoMock
                .Setup(repo => repo.DecreaseQuantity(id))
                .ThrowsAsync(
                    new Exception("Database error"));

            // Act
            var result = await _controller.Decrease(id);

            // Assert
            Assert.IsInstanceOfType(
                result,
                typeof(RedirectToActionResult));

            var redirectResult = result as RedirectToActionResult;

            Assert.AreEqual(
                "Index",
                redirectResult.ActionName);

            Assert.AreEqual(
                "Unable to decrease the book quantity.",
                _controller.TempData["Error"]);
        }
    }
}