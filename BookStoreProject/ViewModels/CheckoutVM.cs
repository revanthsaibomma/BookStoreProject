using System.ComponentModel.DataAnnotations;

namespace BookStoreProject.ViewModels
{
    public class CheckoutVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string PaymentMode { get; set; }
    }
}
