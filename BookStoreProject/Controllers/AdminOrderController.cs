using AutoMapper;
using BookStoreProject.DTOs;
using BookStoreProject.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreProject.Controllers
{
    // Protects the entire controller so only Admins can access it
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IMapper _mapper;

        public AdminOrderController(
            IOrderRepo orderRepo,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _mapper = mapper;
        }


        // ==========================
        // ADMIN ORDER PAGE
        // ==========================
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ==========================
        // MANAGE ORDERS
        // ==========================
        [HttpGet]
        public async Task<IActionResult> AdminManageOrders()
        {
            try
            {
                var orders =
                    await _orderRepo.GetAllOrdersAsync();

                // Handle no orders
                if (orders == null)
                {
                    TempData["Error"] =
                        "Unable to load orders.";

                    return View(new List<OrderDto>());
                }

                // Entity → DTO
                var orderDtos =
                    _mapper.Map<List<OrderDto>>(orders);

                // Validate mapping
                if (orderDtos == null)
                {
                    TempData["Error"] =
                        "Unable to prepare order data.";

                    return View(new List<OrderDto>());
                }

                return View(orderDtos);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading orders.";

                return View(new List<OrderDto>());
            }
        }


        // ==========================
        // UPDATE ORDER STATUS
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int orderId,
            int statusId)
        {
            try
            {
                // Validate Order ID
                if (orderId <= 0)
                {
                    TempData["Error"] =
                        "Invalid order ID.";

                    return RedirectToAction(
                        nameof(AdminManageOrders));
                }

                // Validate Status ID
                if (statusId <= 0)
                {
                    TempData["Error"] =
                        "Invalid order status.";

                    return RedirectToAction(
                        nameof(AdminManageOrders));
                }

                // Update order status
                await _orderRepo.UpdateOrderStatusAsync(
                    orderId,
                    statusId);

                TempData["Success"] =
                    "Order status updated successfully.";

                return RedirectToAction(
                    nameof(AdminManageOrders));
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Unable to update the order status.";

                return RedirectToAction(
                    nameof(AdminManageOrders));
            }
        }
    }
}