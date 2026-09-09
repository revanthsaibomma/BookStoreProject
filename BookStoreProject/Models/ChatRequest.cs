using System.ComponentModel.DataAnnotations;

namespace BookStoreProject.Models
{
    public class ChatRequest
    {
        [Required(ErrorMessage = "Please enter what you are looking for.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Please enter between 2 and 150 characters.")]
        public string Mood { get; set; } = "";
    }
}