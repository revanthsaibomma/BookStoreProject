using BookStoreProject.Models;

namespace BookStoreProject.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        public int UserId { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<CartDetail> CartDetails { get; set; }
    }
}
