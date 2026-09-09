using BookStoreProject.DAL;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using Microsoft.EntityFrameworkCore;

namespace BookStoreProject.Repository
{
    public class BookRepo : IBookRepo
    {
        private readonly ApplicationDbContext _context;

        public BookRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetBooks()
        {
            return await _context.Books
                .Include(b => b.Genre)
                .ToListAsync();
        }

        public async Task<Book?> SearchById(int id)
        {
            return await _context.Books
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.BookId == id);
        }

        public async Task AddBook(Book book)
        {
            await _context.Books.AddAsync(book);
        }

        public async Task UpdateBook(Book book)
        {
            _context.Books.Update(book);
        }

        public async Task DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                _context.Books.Remove(book);
            }
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}