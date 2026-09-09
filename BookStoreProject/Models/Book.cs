using BookStoreProject.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStoreProject.Models
{
    public class Book
    {
        public int BookId { get; set; }

        [Required]
        [MaxLength(40)]
        public string? BookName { get; set; }

        [Required]
        [MaxLength(40)]
        public string? AuthorName { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]

        public string Description { get; set; }
        public string? Image { get; set; }
        [Required]
        public int GenreId { get; set; }
        public Genre Genre { get; set; }

        //public List<OrderDetail> OrderDetail { get; set; }
        public ICollection<CartDetail> CartDetails { get; set; }
        public int Stock { get; set; }
    }
}
