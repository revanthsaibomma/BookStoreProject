using BookStoreProject.DAL;
using BookStoreProject.Models;
using BookStoreProject.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuestPDF.Infrastructure;

namespace BookStoreProject.Tests.ServiceTests
{
    [TestClass]
    public class PdfServiceTests
    {
        private ApplicationDbContext _context = null!;
        private PdfService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            // Required for QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new PdfService(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ==========================================================
        // ORDER NOT FOUND
        // ==========================================================

        [TestMethod]
        public async Task GenerateInvoice_ShouldThrowException_WhenOrderDoesNotExist()
        {
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GenerateInvoice(999));

            Assert.AreEqual(
                "Order not found.",
                exception.Message);
        }

        // ==========================================================
        // GENERATE PDF
        // ==========================================================

        [TestMethod]
        public async Task GenerateInvoice_ShouldReturnPdf_WhenOrderExists()
        {
            var order = CreateOrder();

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var result = await _service.GenerateInvoice(order.Id);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public async Task GenerateInvoice_ShouldReturnPdfWithPdfHeader_WhenOrderExists()
        {
            var order = CreateOrder();

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var result = await _service.GenerateInvoice(order.Id);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 4);

            // PDF files start with %PDF
            string header = System.Text.Encoding.ASCII.GetString(
                result.Take(4).ToArray());

            Assert.AreEqual("%PDF", header);
        }

        // ==========================================================
        // ORDER WITH MULTIPLE ITEMS
        // ==========================================================

        [TestMethod]
        public async Task GenerateInvoice_ShouldGeneratePdf_WhenOrderHasMultipleItems()
        {
            var book1 = new Book
            {
                BookId = 1,
                BookName = "Clean Code",
                AuthorName = "Robert Martin",
                Price = 500,
                Description = "A book about clean coding practices.",
                GenreId = 1,
                Stock = 20
            };

            var book2 = new Book
            {
                BookId = 2,
                BookName = "Atomic Habits",
                AuthorName = "James Clear",
                Price = 700,
                Description = "A book about habits and improvement.",
                GenreId = 1,
                Stock = 20
            };

            await _context.Books.AddRangeAsync(book1, book2);

            var order = new Order
            {
                UserId = 1,
                Name = "Test Customer",
                Address = "Hyderabad",
                PhoneNumber = "9999999999",
                PaymentMode = "COD",
                OrderStatusId = 1,
                CreateDate = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        BookId = 1,
                        Book = book1,
                        Quantity = 2,
                        UnitPrice = 500
                    },
                    new OrderDetail
                    {
                        BookId = 2,
                        Book = book2,
                        Quantity = 1,
                        UnitPrice = 700
                    }
                }
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var result = await _service.GenerateInvoice(order.Id);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);

            string header = System.Text.Encoding.ASCII.GetString(
                result.Take(4).ToArray());

            Assert.AreEqual("%PDF", header);
        }

        // ==========================================================
        // ORDER WITH DIFFERENT QUANTITIES
        // ==========================================================

        [TestMethod]
        public async Task GenerateInvoice_ShouldGeneratePdf_WhenQuantityIsGreaterThanOne()
        {
            var book = new Book
            {
                BookId = 1,
                BookName = "The Alchemist",
                AuthorName = "Paulo Coelho",
                Price = 300,
                Description = "A famous inspirational novel.",
                GenreId = 1,
                Stock = 20
            };

            await _context.Books.AddAsync(book);

            var order = new Order
            {
                UserId = 1,
                Name = "Test Customer",
                Address = "Hyderabad",
                PhoneNumber = "9999999999",
                PaymentMode = "COD",
                OrderStatusId = 1,
                CreateDate = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        BookId = 1,
                        Book = book,
                        Quantity = 5,
                        UnitPrice = 300
                    }
                }
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var result = await _service.GenerateInvoice(order.Id);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        // ==========================================================
        // ORDER WITH NO DETAILS
        // ==========================================================

        [TestMethod]
        public async Task GenerateInvoice_ShouldGeneratePdf_WhenOrderHasNoItems()
        {
            var order = new Order
            {
                UserId = 1,
                Name = "Test Customer",
                Address = "Hyderabad",
                PhoneNumber = "9999999999",
                PaymentMode = "COD",
                OrderStatusId = 1,
                CreateDate = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>()
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var result = await _service.GenerateInvoice(order.Id);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        // ==========================================================
        // HELPER METHOD
        // ==========================================================

        private Order CreateOrder()
        {
            var book = new Book
            {
                BookId = 1,
                BookName = "Test Book",
                AuthorName = "Test Author",
                Price = 500,
                Description = "Test book description.",
                GenreId = 1,
                Stock = 20
            };

            return new Order
            {
                UserId = 1,
                Name = "Test Customer",
                Address = "Hyderabad",
                PhoneNumber = "9999999999",
                PaymentMode = "COD",
                OrderStatusId = 1,
                CreateDate = DateTime.UtcNow,

                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        Book = book,
                        BookId = book.BookId,
                        Quantity = 2,
                        UnitPrice = 500
                    }
                }
            };
        }
    }
}