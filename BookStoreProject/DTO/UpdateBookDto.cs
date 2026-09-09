using System.ComponentModel.DataAnnotations;

namespace BookStoreProject.DTOs
{
    public class UpdateBookDto
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
        public string? Description { get; set; }

        public string? Image { get; set; }

        [Required]
        public int GenreId { get; set; }

        public int Stock { get; set; }
    }
}