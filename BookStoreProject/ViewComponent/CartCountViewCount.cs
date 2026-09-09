using BookStoreProject.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStoreProject.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ICartRepo _cartRepo;

        public CartCountViewComponent(ICartRepo cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string claimId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            int.TryParse(claimId, out int userId);


            int count = await _cartRepo.GetCartCount(userId);

            return View(count);
        }
    }
}