
using BookStoreProject.DAL;
using BookStoreProject.Models;
using BookStoreProject.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookStoreProject.Repository
{
    public class CartRepo : ICartRepo
    {
        private readonly ApplicationDbContext _context;

        public CartRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task IncreaseBookQuantity(int userId, int bookId)
        {
            var item = await _context.CartDetails
                .Include(c => c.Cart)
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c =>
                    c.Cart.UserId == userId &&
                    c.BookId == bookId &&
                    !c.Cart.IsDeleted);

            if (item != null)
            {
                if (item.Quantity < item.Book.Stock)
                {
                    item.Quantity++;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task DecreaseBookQuantity(int userId, int bookId)
        {
            var item = await _context.CartDetails
                .Include(c => c.Cart)
                .FirstOrDefaultAsync(c =>
                    c.Cart.UserId == userId &&
                    c.BookId == bookId &&
                    !c.Cart.IsDeleted);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    _context.CartDetails.Remove(item);
                }

                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> GetBookQuantity(int userId, int bookId)
        {
            var item = await _context.CartDetails
                .Include(c => c.Cart)
                .FirstOrDefaultAsync(c =>
                    c.Cart.UserId == userId &&
                    c.BookId == bookId &&
                    !c.Cart.IsDeleted);

            if (item == null)
                return 0;

            return item.Quantity;
        }
        public async Task AddItem(int bookId, int userId)
        {
            // Find user's cart
            var cart = await _context.Carts
                .Include(c => c.CartDetails)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            // Create cart if it doesn't exist
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    IsDeleted = false
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check if book already exists in cart
            var cartItem = await _context.CartDetails
                .FirstOrDefaultAsync(c =>
                    c.CartId == cart.CartId &&
                    c.BookId == bookId);

            if (cartItem == null)
            {
                _context.CartDetails.Add(new CartDetail
                {
                    CartId = cart.CartId,
                    BookId = bookId,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ShoppingCartVM>> GetUserCart(int userId)
        {
            var cartItems = await _context.CartDetails
                .Include(c => c.Book)
                .Include(c => c.Cart)
                .Where(c => c.Cart.UserId == userId && !c.Cart.IsDeleted)
                .Select(c => new ShoppingCartVM
                {
                    CartDetailId = c.CartDetailId,
                    BookId = c.BookId,
                    BookName = c.Book.BookName,
                    Image = c.Book.Image,
                    Price = c.Book.Price,
                    Stock = c.Book.Stock,
                    Quantity = c.Quantity
                })
                .ToListAsync();

            return cartItems;
        }

        public async Task<int> GetCartCount(int userId)
        {
            return await _context.CartDetails
                .Include(c => c.Cart)
                .Where(c => c.Cart.UserId == userId && !c.Cart.IsDeleted)
                .SumAsync(c => c.Quantity);
        }

        public async Task IncreaseQuantity(int cartDetailId)
        {
            var item = await _context.CartDetails.FindAsync(cartDetailId);

            if (item != null)
            {
                item.Quantity++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DecreaseQuantity(int cartDetailId)
        {
            var item = await _context.CartDetails.FindAsync(cartDetailId);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    _context.CartDetails.Remove(item);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}