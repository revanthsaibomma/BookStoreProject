using BookStoreProject.ViewModels;

namespace BookStoreProject.Repository
{
    public interface ICartRepo
    {
        Task AddItem(int bookId, int userId);

        Task IncreaseQuantity(int cartDetailId);

        Task DecreaseQuantity(int cartDetailId);

        Task<List<ShoppingCartVM>> GetUserCart(int userId);

        Task<int> GetCartCount(int userId);

        Task<int> GetBookQuantity(int userId, int bookId);

        Task IncreaseBookQuantity(int userId, int bookId);

        Task DecreaseBookQuantity(int userId, int bookId);

    }
}
