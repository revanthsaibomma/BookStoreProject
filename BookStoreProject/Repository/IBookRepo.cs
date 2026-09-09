using BookStoreProject.Models;

namespace BookStoreProject.Repository
{
    public interface IBookRepo
    {
        Task AddBook(Book book);
        Task DeleteBook(int id);
        Task<IEnumerable<Book>> GetBooks();
        Task<Book> SearchById(int id);
        Task UpdateBook(Book book);
        Task Save();
    }
}
