using BookStoreProject.DAL;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BookStoreProject.Tests.RepositoryTests
{
    [TestClass]
    public class CartRepoTests
    {
        private ApplicationDbContext _context = null!;
        private CartRepo _cartRepo = null!;

        // =========================================================
        // SETUP
        // =========================================================
        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _cartRepo = new CartRepo(_context);
        }

        // =========================================================
        // CLEANUP
        // =========================================================
        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        // =========================================================
        // HELPER - CREATE BOOK
        // =========================================================
        private async Task<Book> CreateBook(
            string bookName = "Test Book",
            double price = 500)
        {
            // Check whether Genre already exists
            var genre = await _context.Genres
                .FirstOrDefaultAsync(g => g.Id == 1);

            // Create Genre only if it doesn't exist
            if (genre == null)
            {
                genre = new Genre
                {
                    Id = 1,
                    GenreName = "Fiction"
                };

                _context.Genres.Add(genre);

                await _context.SaveChangesAsync();
            }

            var book = new Book
            {
                BookName = bookName,
                AuthorName = "Test Author",
                Price = price,
                Description = "Test book description",
                Image = "test.jpg",
                GenreId = genre.Id,
                Stock = 20
            };

            _context.Books.Add(book);

            await _context.SaveChangesAsync();

            return book;
        }

        // =========================================================
        // HELPER - CREATE CART
        // =========================================================
        private async Task<Cart> CreateCart(int userId)
        {
            var cart = new Cart
            {
                UserId = userId,
                IsDeleted = false
            };

            _context.Carts.Add(cart);

            await _context.SaveChangesAsync();

            return cart;
        }

        // =========================================================
        // HELPER - CREATE CART DETAIL
        // =========================================================
        private async Task<CartDetail> CreateCartDetail(
            int userId,
            int bookId,
            int quantity)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    !c.IsDeleted);

            if (cart == null)
            {
                cart = await CreateCart(userId);
            }

            var cartDetail = new CartDetail
            {
                CartId = cart.CartId,
                BookId = bookId,
                Quantity = quantity
            };

            _context.CartDetails.Add(cartDetail);

            await _context.SaveChangesAsync();

            return cartDetail;
        }

        // =========================================================
        // 1. AddItem - New Cart
        // =========================================================
        [TestMethod]
        public async Task AddItem_ShouldCreateCartAndAddBook()
        {
            int userId = 1;

            var book = await CreateBook(
                "C# Programming",
                700);

            await _cartRepo.AddItem(
                book.BookId,
                userId);

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    !c.IsDeleted);

            Assert.IsNotNull(cart);

            var cartItem = await _context.CartDetails
                .FirstOrDefaultAsync(c =>
                    c.CartId == cart.CartId &&
                    c.BookId == book.BookId);

            Assert.IsNotNull(cartItem);

            Assert.AreEqual(
                1,
                cartItem.Quantity);
        }

        // =========================================================
        // 2. AddItem - Existing Cart
        // =========================================================
        [TestMethod]
        public async Task AddItem_ShouldAddBookToExistingCart()
        {
            int userId = 2;

            var book = await CreateBook(
                "Book One",
                400);

            var cart = await CreateCart(userId);

            await _cartRepo.AddItem(
                book.BookId,
                userId);

            var cartItems = await _context.CartDetails
                .Where(c => c.CartId == cart.CartId)
                .ToListAsync();

            Assert.AreEqual(
                1,
                cartItems.Count);

            Assert.AreEqual(
                book.BookId,
                cartItems[0].BookId);

            Assert.AreEqual(
                1,
                cartItems[0].Quantity);
        }

        // =========================================================
        // 3. AddItem - Existing Book
        // =========================================================
        [TestMethod]
        public async Task AddItem_ShouldIncreaseQuantity_WhenBookAlreadyExists()
        {
            int userId = 3;

            var book = await CreateBook(
                "Existing Book",
                600);

            await CreateCartDetail(
                userId,
                book.BookId,
                1);

            await _cartRepo.AddItem(
                book.BookId,
                userId);

            var cartItem = await _context.CartDetails
                .FirstOrDefaultAsync(c =>
                    c.BookId == book.BookId);

            Assert.IsNotNull(cartItem);

            Assert.AreEqual(
                2,
                cartItem.Quantity);
        }

        // =========================================================
        // 4. IncreaseQuantity
        // =========================================================
        [TestMethod]
        public async Task IncreaseQuantity_ShouldIncreaseQuantity()
        {
            int userId = 4;

            var book = await CreateBook(
                "Increase Book",
                500);

            var cartItem = await CreateCartDetail(
                userId,
                book.BookId,
                2);

            await _cartRepo.IncreaseQuantity(
                cartItem.CartDetailId);

            var result = await _context.CartDetails
                .FindAsync(cartItem.CartDetailId);

            Assert.IsNotNull(result);

            Assert.AreEqual(
                3,
                result.Quantity);
        }

        // =========================================================
        // 5. IncreaseQuantity - Invalid ID
        // =========================================================
        [TestMethod]
        public async Task IncreaseQuantity_ShouldNotThrow_WhenItemDoesNotExist()
        {
            await _cartRepo.IncreaseQuantity(9999);

            var count = await _context.CartDetails
                .CountAsync();

            Assert.AreEqual(
                0,
                count);
        }

        // =========================================================
        // 6. DecreaseQuantity
        // =========================================================
        [TestMethod]
        public async Task DecreaseQuantity_ShouldDecreaseQuantity()
        {
            int userId = 5;

            var book = await CreateBook(
                "Decrease Book",
                450);

            var cartItem = await CreateCartDetail(
                userId,
                book.BookId,
                3);

            await _cartRepo.DecreaseQuantity(
                cartItem.CartDetailId);

            var result = await _context.CartDetails
                .FindAsync(cartItem.CartDetailId);

            Assert.IsNotNull(result);

            Assert.AreEqual(
                2,
                result.Quantity);
        }

        // =========================================================
        // 7. DecreaseQuantity - Remove At Zero
        // =========================================================
        [TestMethod]
        public async Task DecreaseQuantity_ShouldRemoveItem_WhenQuantityBecomesZero()
        {
            int userId = 6;

            var book = await CreateBook(
                "Remove Book",
                300);

            var cartItem = await CreateCartDetail(
                userId,
                book.BookId,
                1);

            await _cartRepo.DecreaseQuantity(
                cartItem.CartDetailId);

            var result = await _context.CartDetails
                .FindAsync(cartItem.CartDetailId);

            Assert.IsNull(result);
        }

        // =========================================================
        // 8. IncreaseBookQuantity
        // =========================================================
        [TestMethod]
        public async Task IncreaseBookQuantity_ShouldIncreaseQuantity()
        {
            int userId = 7;

            var book = await CreateBook(
                "Home Increase Book",
                550);

            await CreateCartDetail(
                userId,
                book.BookId,
                2);

            await _cartRepo.IncreaseBookQuantity(
                userId,
                book.BookId);

            var result = await _context.CartDetails
                .FirstOrDefaultAsync(c =>
                    c.BookId == book.BookId);

            Assert.IsNotNull(result);

            Assert.AreEqual(
                3,
                result.Quantity);
        }

        // =========================================================
        // 9. DecreaseBookQuantity
        // =========================================================
        [TestMethod]
        public async Task DecreaseBookQuantity_ShouldDecreaseQuantity()
        {
            int userId = 8;

            var book = await CreateBook(
                "Home Decrease Book",
                650);

            await CreateCartDetail(
                userId,
                book.BookId,
                3);

            await _cartRepo.DecreaseBookQuantity(
                userId,
                book.BookId);

            var result = await _context.CartDetails
                .FirstOrDefaultAsync(c =>
                    c.BookId == book.BookId);

            Assert.IsNotNull(result);

            Assert.AreEqual(
                2,
                result.Quantity);
        }

        // =========================================================
        // 10. DecreaseBookQuantity - Remove At Zero
        // =========================================================
        [TestMethod]
        public async Task DecreaseBookQuantity_ShouldRemoveItem_WhenQuantityBecomesZero()
        {
            int userId = 9;

            var book = await CreateBook(
                "Remove From Home Book",
                350);

            await CreateCartDetail(
                userId,
                book.BookId,
                1);

            await _cartRepo.DecreaseBookQuantity(
                userId,
                book.BookId);

            var result = await _context.CartDetails
                .FirstOrDefaultAsync(c =>
                    c.BookId == book.BookId);

            Assert.IsNull(result);
        }

        // =========================================================
        // 11. GetBookQuantity
        // =========================================================
        [TestMethod]
        public async Task GetBookQuantity_ShouldReturnCorrectQuantity()
        {
            int userId = 10;

            var book = await CreateBook(
                "Quantity Book",
                750);

            await CreateCartDetail(
                userId,
                book.BookId,
                5);

            var result = await _cartRepo.GetBookQuantity(
                userId,
                book.BookId);

            Assert.AreEqual(
                5,
                result);
        }

        // =========================================================
        // 12. GetBookQuantity - Item Does Not Exist
        // =========================================================
        [TestMethod]
        public async Task GetBookQuantity_ShouldReturnZero_WhenItemDoesNotExist()
        {
            var result = await _cartRepo.GetBookQuantity(
                999,
                999);

            Assert.AreEqual(
                0,
                result);
        }

        // =========================================================
        // 13. GetCartCount
        // =========================================================
        [TestMethod]
        public async Task GetCartCount_ShouldReturnTotalQuantity()
        {
            int userId = 11;

            var book1 = await CreateBook(
                "Cart Book One",
                300);

            var book2 = await CreateBook(
                "Cart Book Two",
                400);

            await CreateCartDetail(
                userId,
                book1.BookId,
                2);

            await CreateCartDetail(
                userId,
                book2.BookId,
                3);

            var result = await _cartRepo.GetCartCount(
                userId);

            Assert.AreEqual(
                5,
                result);
        }

        // =========================================================
        // 14. GetCartCount - Empty Cart
        // =========================================================
        [TestMethod]
        public async Task GetCartCount_ShouldReturnZero_WhenCartIsEmpty()
        {
            int userId = 12;

            await CreateCart(userId);

            var result = await _cartRepo.GetCartCount(
                userId);

            Assert.AreEqual(
                0,
                result);
        }

        // =========================================================
        // 15. GetUserCart
        // =========================================================
        [TestMethod]
        public async Task GetUserCart_ShouldReturnCartItems()
        {
            int userId = 13;

            var book1 = await CreateBook(
                "Cart Book One",
                500);

            var book2 = await CreateBook(
                "Cart Book Two",
                700);

            await CreateCartDetail(
                userId,
                book1.BookId,
                2);

            await CreateCartDetail(
                userId,
                book2.BookId,
                3);

            var result = await _cartRepo.GetUserCart(
                userId);

            Assert.IsNotNull(result);

            Assert.AreEqual(
                2,
                result.Count);

            var firstItem = result
                .First(c => c.BookId == book1.BookId);

            var secondItem = result
                .First(c => c.BookId == book2.BookId);

            Assert.AreEqual(
                "Cart Book One",
                firstItem.BookName);

            Assert.AreEqual(
                500,
                firstItem.Price);

            Assert.AreEqual(
                2,
                firstItem.Quantity);

            Assert.AreEqual(
                "Cart Book Two",
                secondItem.BookName);

            Assert.AreEqual(
                700,
                secondItem.Price);

            Assert.AreEqual(
                3,
                secondItem.Quantity);
        }

        // =========================================================
        // 16. GetUserCart - Empty Cart
        // =========================================================
        [TestMethod]
        public async Task GetUserCart_ShouldReturnEmptyList_WhenCartIsEmpty()
        {
            int userId = 14;

            await CreateCart(userId);

            var result = await _cartRepo.GetUserCart(
                userId);

            Assert.IsNotNull(result);

            Assert.AreEqual(
                0,
                result.Count);
        }

        // =========================================================
        // 17. IncreaseBookQuantity - Invalid Item
        // =========================================================
        [TestMethod]
        public async Task IncreaseBookQuantity_ShouldDoNothing_WhenItemDoesNotExist()
        {
            await _cartRepo.IncreaseBookQuantity(
                999,
                999);

            var count = await _context.CartDetails
                .CountAsync();

            Assert.AreEqual(
                0,
                count);
        }

        // =========================================================
        // 18. DecreaseBookQuantity - Invalid Item
        // =========================================================
        [TestMethod]
        public async Task DecreaseBookQuantity_ShouldDoNothing_WhenItemDoesNotExist()
        {
            await _cartRepo.DecreaseBookQuantity(
                999,
                999);

            var count = await _context.CartDetails
                .CountAsync();

            Assert.AreEqual(
                0,
                count);
        }
    }
}