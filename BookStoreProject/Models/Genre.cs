using System.ComponentModel.DataAnnotations;

namespace BookStoreProject.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string GenreName { get; set; }
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
