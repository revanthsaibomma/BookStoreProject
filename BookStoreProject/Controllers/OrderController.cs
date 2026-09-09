using AutoMapper;
using BookStoreProject.DTOs;
using BookStoreProject.Repository;
using BookStoreProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookStoreProject.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IPdfService _pdfService;
        private readonly IMapper _mapper;

        public OrderController(
            IOrderRepo orderRepo,
            IPdfService pdfService,
            IMapper mapper)
        {
            _orderRepo = orderRepo;
            _pdfService = pdfService;
            _mapper = mapper;
        }


        // ==========================================
        // GET: ORDER INDEX
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Checkout));
        }


        // ==========================================
        // GET: CHECKOUT PAGE
        // ==========================================

        [HttpGet]
        public IActionResult Checkout()
        {
            return View(new CheckoutDto());
        }


        // ==========================================
        // POST: PROCESS CHECKOUT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
            CheckoutDto model)
        {
            // ======================================
            // MODEL VALIDATION
            // ======================================

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ======================================
            // GET LOGGED-IN USER ID
            // ======================================

            string? claimId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (string.IsNullOrWhiteSpace(claimId))
            {
                TempData["ErrorMessage"] =
                    "Your session has expired. Please login again.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (!int.TryParse(
                claimId,
                out int userId))
            {
                TempData["ErrorMessage"] =
                    "Invalid user information.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            if (userId <= 0)
            {
                TempData["ErrorMessage"] =
                    "Invalid user ID.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // ======================================
            // EXTRA INPUT VALIDATION
            // ======================================

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "Name is required.");
            }


            if (string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError(
                    nameof(model.Address),
                    "Address is required.");
            }


            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                ModelState.AddModelError(
                    nameof(model.PhoneNumber),
                    "Phone number is required.");
            }


            if (string.IsNullOrWhiteSpace(model.PaymentMode))
            {
                ModelState.AddModelError(
                    nameof(model.PaymentMode),
                    "Please select a payment method.");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ======================================
            // PLACE ORDER
            // ======================================

            try
            {
                int orderId =
                    await _orderRepo.PlaceOrderAsync(
                        userId,
                        model.Name!.Trim(),
                        model.Address!.Trim(),
                        model.PhoneNumber!.Trim(),
                        model.PaymentMode!.Trim()
                    );


                // ==================================
                // VALIDATE CREATED ORDER ID
                // ==================================

                if (orderId <= 0)
                {
                    TempData["ErrorMessage"] =
                        "The order could not be created.";

                    return RedirectToAction(
                        "Index",
                        "Cart");
                }


                return RedirectToAction(
                    nameof(OrderSuccess),
                    new { id = orderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "====================================");

                Console.WriteLine(
                    "ORDER CHECKOUT ERROR");

                Console.WriteLine(ex.ToString());

                Console.WriteLine(
                    "====================================");


                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    "Index",
                    "Cart");
            }
        }


        // ==========================================
        // GET: ORDER SUCCESS
        // ==========================================

        [HttpGet]
        public IActionResult OrderSuccess(int id)
        {
            // Validate order ID
            if (id <= 0)
            {
                return BadRequest(
                    "Invalid order ID.");
            }


            ViewBag.OrderId = id;

            return View();
        }


        // ==========================================
        // GET: ORDER HISTORY
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> History()
        {
            try
            {
                // ==================================
                // GET USER ID
                // ==================================

                string? claimId =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);


                if (string.IsNullOrWhiteSpace(claimId))
                {
                    return RedirectToAction(
                        "Login",
                        "Account");
                }


                if (!int.TryParse(
                    claimId,
                    out int userId))
                {
                    return BadRequest(
                        "Invalid user information.");
                }


                if (userId <= 0)
                {
                    return BadRequest(
                        "Invalid user ID.");
                }


                // ==================================
                // GET USER ORDERS
                // ==================================

                var orders =
                    await _orderRepo
                        .GetUserOrdersAsync(userId);


                if (orders == null)
                {
                    return View(
                        new List<OrderDto>());
                }


                // ==================================
                // MAP ORDERS
                // ==================================

                var orderDtos =
                    _mapper.Map<List<OrderDto>>(
                        orders);


                return View(orderDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "====================================");

                Console.WriteLine(
                    "ORDER HISTORY ERROR");

                Console.WriteLine(ex.ToString());

                Console.WriteLine(
                    "====================================");


                return StatusCode(
                    500,
                    "Unable to load your order history.");
            }
        }


        // ==========================================
        // DOWNLOAD INVOICE
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> DownloadInvoice(
            int id)
        {
            try
            {
                // ==================================
                // VALIDATE ORDER ID
                // ==================================

                if (id <= 0)
                {
                    return BadRequest(
                        "Invalid order ID.");
                }


                // ==================================
                // GENERATE PDF
                // ==================================

                var pdf =
                    await _pdfService.GenerateInvoice(id);


                // ==================================
                // VALIDATE PDF
                // ==================================

                if (pdf == null ||
                    pdf.Length == 0)
                {
                    return NotFound(
                        "Invoice could not be generated.");
                }


                return File(
                    pdf,
                    "application/pdf",
                    $"Invoice_{id}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "====================================");

                Console.WriteLine(
                    "INVOICE ERROR");

                Console.WriteLine(ex.ToString());

                Console.WriteLine(
                    "====================================");


                return StatusCode(
                    500,
                    "Unable to generate the invoice.");
            }
        }
    }
}