using BookStoreProject.DAL;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BookStoreProject.Tests.RepositoryTests
{
    [TestClass]
    public class BookRepoTests
    {
        private ApplicationDbContext _context = null!;
        private BookRepo _bookRepo = null!;

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

            _bookRepo = new BookRepo(_context);
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
        // 1. AddBook()
        // =========================================================
        [TestMethod]
        public async Task AddBook_ShouldAddBookToDatabase()
        {
            // Arrange
            var genre = new Genre
            {
                Id = 1,
                GenreName = "Fiction"
            };

            _context.Genres.Add(genre);

            var book = new Book
            {
                BookName = "The Great Book",
                AuthorName = "John Smith",
                Price = 500,
                Description = "A great fiction book",
                Image = "book.jpg",
                GenreId = 1,
                Stock = 10
            };

            // Act
            await _bookRepo.AddBook(book);
            await _bookRepo.Save();

            // Assert
            var result = await _context.Books
                .FirstOrDefaultAsync(b => b.BookName == "The Great Book");

            Assert.IsNotNull(result);

            Assert.AreEqual(
                "The Great Book",
                result.BookName);

            Assert.AreEqual(
                "John Smith",
                result.AuthorName);

            Assert.AreEqual(
                500,
                result.Price);

            Assert.AreEqual(
                10,
                result.Stock);

            Assert.AreEqual(
                "A great fiction book",
                result.Description);
        }

        // =========================================================
        // 2. GetBooks()
        // =========================================================
        [TestMethod]
        public async Task GetBooks_ShouldReturnAllBooks()
        {
            // Arrange
            var genre = new Genre
            {
                Id = 1,
                GenreName = "Fiction"
            };

            _context.Genres.Add(genre);

            var book1 = new Book
            {
                BookName = "Book One",
                AuthorName = "Author One",
                Price = 300,
                Description = "Description One",
                GenreId = 1,
                Stock = 5
            };

            var book2 = new Book
            {
                BookName = "Book Two",
                AuthorName = "Author Two",
                Price = 400,
                Description = "Description Two",
                GenreId = 1,
                Stock = 8
            };

            _context.Books.AddRange(book1, book2);

            await _context.SaveChangesAsync();

            // Act
            var result = await _bookRepo.GetBooks();

            // Assert
            Assert.IsNotNull(result);

            var books = result.ToList();

            Assert.AreEqual(2, books.Count);

            Assert.AreEqual(
                "Book One",
                books[0].BookName);

            Assert.AreEqual(
                "Book Two",
                books[1].BookName);
        }

        // =========================================================
        // 3. GetBooks() - Genre Included
        // =========================================================
        [TestMethod]
        public async Task GetBooks_ShouldIncludeGenre()
        {
            // Arrange
            var genre = new Genre
            {
                Id = 1,
                GenreName = "Science Fiction"
            };

            _context.Genres.Add(genre);

            var book = new Book
            {
                BookName = "Space Journey",
                AuthorName = "Test Author",
                Price = 600,
                Description = "A science fiction book",
                GenreId = 1,
                Stock = 10
            };

            _context.Books.Add(book);

            await _context.SaveChangesAsync();

            // Act
            var result = await _bookRepo.GetBooks();

            // Assert
            var returnedBook = result.First();

            Assert.IsNotNull(returnedBook);

            Assert.IsNotNull(returnedBook.Genre);

            Assert.AreEqual(
                "Science Fiction",
                returnedBook.Genre.GenreName);
        }

        // =========================================================
        // 4. SearchById() - Book Exists
        // =========================================================
        [TestMethod]
        public async Task SearchById_ShouldReturnBook_WhenBookExists()
        {
            // Arrange
            var genre = new Genre
            {
                Id = 1,
                GenreName = "Programming"
            };

            _context.Genres.Add(genre);

            var book = new Book
            {
                BookName = "C# Programming",
                AuthorName = "Robert",
                Price = 700,
                Description = "A C# programming book",
                GenreId = 1,
                Stock = 15
            };

            _context.Books.Add(book);

            await _context.SaveChangesAsync();

            // Get the generated BookId
            var bookId = book.BookId;

            // Act
            var result = await _bookRepo.SearchById(bookId);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(
                bookId,
                result.BookId);

            Assert.AreEqual(
                "C# Programming",
                result.BookName);

            Assert.AreEqual(
                "Robert",
                result.AuthorName);

            Assert.AreEqual(
                700,
                result.Price);

            Assert.IsNotNull(result.Genre);

            Assert.AreEqual(
                "Programming",
                result.Genre.GenreName);
        }

        // =========================================================
        // 5. SearchById() - Book Does Not Exist
        // =========================================================
        [TestMethod]
        public async Task SearchById_ShouldReturnNull_WhenBookDoesNotExist()
        {
            // Act
            var result = await _bookRepo.SearchById(9999);

            // Assert
            Assert.IsNull(result);
        }

        // =========================================================
        // 6. UpdateBook()
        // =========================================================
        [TestMethod]
        public async Task UpdateBook_ShouldUpdateBookDetails()
        {
            // Arrange
            var genre = new Genre
            {
                Id = 1,
                GenreName = "Fiction"
            };

            _context.Genres.Add(genre);

            var book = new Book
            {
                BookName = "Old Book Name",
                AuthorName = "Old Author",
                Price = 300,
                Description = "Old Description",
                GenreId = 1,
                Stock = 5
            };

            _context.Books.Add(book);

            await _context.SaveChangesAsync();

            var bookId = book.BookId;

            // Change values
            book.BookName = "Updated Book Name";
            book.AuthorName = "Updated Author";
            book.Price = 800;
            book.Description = "Updated Description";
            book.Stock = 20;

            // Act
            await _bookRepo.UpdateBook(book);
            await _bookRepo.Save();

            // Assert
            var result = await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            Assert.IsNotNull(result);

            Assert.AreEqual(
                "Updated Book Name",
                result.BookName);

            Assert.AreEqual(
                "Updated Author",
                result.AuthorName);

            Assert.AreEqual(
                800,
                result.Price);

            Assert.AreEqual(
                "Updated Description",
                result.Description);

            Assert.AreEqual(
                20,
                result.Stock);
        }

        // =========================================================
        // 7. DeleteBook()
        // =========================================================
        [TestMethod]
        public async Task DeleteBook_ShouldDeleteBook_WhenBookExists()
        {
            // Arrange
            var genre = new Genre
            {
                Id = 1,
                GenreName = "Fiction"
            };

            _context.Genres.Add(genre);

            var book = new Book
            {
                BookName = "Book To Delete",
                AuthorName = "Test Author",
                Price = 500,
                Description = "This book will be deleted",
                GenreId = 1,
                Stock = 10
            };

            _context.Books.Add(book);

            await _context.SaveChangesAsync();

            var bookId = book.BookId;

            // Act
            await _bookRepo.DeleteBook(bookId);
            await _bookRepo.Save();

            // Assert
            var result = await _context.Books
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            Assert.IsNull(result);
        }

        // =========================================================
        // 8. DeleteBook() - Book Does Not Exist
        // =========================================================
        [TestMethod]
        public async Task DeleteBook_ShouldNotThrowException_WhenBookDoesNotExist()
        {
            // Act
            await _bookRepo.DeleteBook(9999);
            await _bookRepo.Save();

            // Assert
            var count = await _context.Books.CountAsync();

            Assert.AreEqual(0, count);
        }

        // =========================================================
        // 9. Save()
        // =========================================================
        [TestMethod]
        public async Task Save_ShouldPersistChanges()
        {
            // Arrange
            var genre = new Genre
            {
                Id = 1,
                GenreName = "Testing"
            };

            _context.Genres.Add(genre);

            var book = new Book
            {
                BookName = "Save Test Book",
                AuthorName = "Test Author",
                Price = 450,
                Description = "Testing Save method",
                GenreId = 1,
                Stock = 12
            };

            // Act
            await _bookRepo.AddBook(book);

            // Verify entity is tracked before Save
            Assert.AreEqual(
                1,
                _context.Books.Local.Count);

            await _bookRepo.Save();

            // Assert
            var result = await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    b => b.BookName == "Save Test Book");

            Assert.IsNotNull(result);

            Assert.AreEqual(
                "Save Test Book",
                result.BookName);

            Assert.AreEqual(
                "Test Author",
                result.AuthorName);
        }
    }
}