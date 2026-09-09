using System.ComponentModel.DataAnnotations;

namespace BookStoreProject.Models
{
    public class BookSearchIntent
    {
        [Required]
        public string Type { get; set; } = "";

        public string Value { get; set; } = "";
    }
}